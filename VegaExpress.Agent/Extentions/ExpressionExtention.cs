
using DynamicData;
using DynamicData.Binding;

using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace VegaExpress.Agent.Extentions
{
    public static class ExpressionExtention
    {
        public static IEnumerable<LambdaExpression> NewExpressionToUnaryExpressionArray<T>(Expression<Func<T, object>> propertyLambda)
        {
            var memberExpression = propertyLambda.Body as NewExpression;
            if (memberExpression != null)
            {
                int index = 0;
                foreach (var member in memberExpression.Members!)
                {
                    var propertyType = member.DeclaringType;
                    if (propertyType != null)
                    {
                        string nodeName = "x";
                        var expression = memberExpression.Arguments[index];
                        var propertyPath = ExpressionToString(expression);
                        propertyPath = propertyPath!.Replace($"{nodeName}.", "");

                        var lambda = ParseLambda(typeof(T), propertyPath, nodeName);

                        yield return (lambda);
                    }
                    index++;
                }
            }
        }
        public static IEnumerable<(string FieldName, string Path, Expression<Func<T, object>>[] GetExpression, Expression<Action<T, object>>[] SetExpression)> GetLambdaExpression<T>(Expression<Func<T, object>> propertyLambda)
        {
            switch (propertyLambda.Body)
            {
                case NewExpression newExpression:
                    foreach (var argument in newExpression.Arguments)yield return ProcessExpression<T>(argument, propertyLambda.Parameters);
                    break;
                case MemberExpression memberExpression:
                    yield return ProcessExpression<T>(memberExpression, propertyLambda.Parameters);
                    break;
                case UnaryExpression unaryExpression when unaryExpression.Operand is MemberExpression unaryMemberExpression:
                    yield return ProcessExpression<T>(unaryMemberExpression, propertyLambda.Parameters);
                    break;
                case MethodCallExpression methodCallExpression:
                    {
                        var getExpressions = new List<Expression<Func<T, object>>>();
                        var setExpressions = new List<Expression<Action<T, object>>>();

                        if (methodCallExpression.Method.DeclaringType == typeof(string) &&
                        methodCallExpression.Method.Name == "Concat")
                        {
                            var parameter = Expression.Parameter(typeof(T), "x");

                            foreach (var argument in methodCallExpression.Arguments)
                            {
                                var memberExpression = argument as MemberExpression;
                                if (memberExpression == null) continue;

                                var expression = Expression.Lambda<Func<T, object>>(memberExpression, parameter);
                                var property = GetLambdaExpression<T>(expression).First();
                                getExpressions.Add(property.GetExpression[0]);
                                setExpressions.Add(property.SetExpression[0]);
                            }
                            yield return ("Expression", null, getExpressions.ToArray()!, setExpressions.ToArray())!;
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException($"Unsupported expression type: {propertyLambda.Body.GetType().Name}");
            }
            (string FieldName, string Path, Expression<Func<T, object>>[] GetExpression, Expression<Action<T, object>>[] SetExpression) ProcessExpression<T>(Expression expression, ReadOnlyCollection<ParameterExpression> parameters)
            {
                string propertyName, propertyPath = ExpressionToString(expression);

                var dotIndex = propertyPath.IndexOf('.');
                var lastDotIndex = propertyPath.LastIndexOf('.');

                propertyName = propertyPath.Substring(lastDotIndex + 1, propertyPath.Length - (lastDotIndex + 1));
                propertyPath = propertyPath.Remove(0, dotIndex + 1);

                #region getExpression
                var getExpression = Expression.Lambda<Func<T, object>>(Expression.Convert(expression, typeof(object)), parameters);
                #endregion

                #region setExpression                
                ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");
                BinaryExpression assignExpression = Expression.Assign(expression, Expression.Convert(valueParameter, expression.Type));
                var setExpression = Expression.Lambda<Action<T, object>>(assignExpression, parameters[0], valueParameter);
                #endregion

                return (propertyName, propertyPath, new[] { getExpression }, new[] { setExpression });
            }
        }
        public static LambdaExpression ParseLambda(Type type, string propertyPath, string nodeName = "x")
        {
            var parameter = Expression.Parameter(type, nodeName);
            var expression = DynamicExpressionParser.ParseLambda(new[] { parameter }, null, $"{nodeName}.{propertyPath}");
            return expression;
        }
        public static string ExpressionToString(Expression expression)
        {
            switch (expression)
            {
                case MemberExpression memberExpression:
                    return $"{ExpressionToString(memberExpression.Expression!)}.{memberExpression.Member.Name}";
                case MethodCallExpression methodCallExpression when methodCallExpression.Method.Name == "get_Item":
                    return $"{ExpressionToString(methodCallExpression.Object!)}[{ExpressionToString(methodCallExpression.Arguments[0])}]";
                case BinaryExpression binaryExpression:
                    return binaryExpression.ToString()!;
                case ParameterExpression parameterExpression:
                    return parameterExpression.Name!;
                case ConstantExpression constantExpression:
                    return constantExpression.Value!.ToString()!;
                default:
                    throw new NotSupportedException($"Unsupported expression type: {expression.GetType().Name}");
            }
        }
        public static object CompileGetExpression<T>(LambdaExpression getExpression, T instance)
        {
            Func<T, object> compiledExpression = ((Expression<Func<T, object>>)getExpression).Compile();
            object value  = compiledExpression(instance);
            //object value = compiledExpression.DynamicInvoke(instance)!;
            return value;
        }
        public static void CompileSetExpression<T, TValue>(LambdaExpression setExpression, T instance, TValue value)
        {
            Action<T, TValue> compiledExpression = ((Expression<Action<T,TValue>>)setExpression).Compile();
            compiledExpression(instance, value);
        }
    }
    public class PropertyExpressionVisitor : ExpressionVisitor
    {
        public PropertyExpressionVisitor(string propertyName) => PropertyName = propertyName;
        public string? PropertyName { get; private set; }
        public Type? ContainingType { get; private set; }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.Name == PropertyName)
            {
                ContainingType = node.Member.DeclaringType;
            }
            return base.VisitMember(node);
        }
    }
}
