using System.Collections;
using System.Linq.Expressions;

using Terminal.Gui;

namespace VegaExpress.Agent.Controls
{
    public class ListView<T> : ListView
    {
        private List<T> data = new List<T>();
        private List<string> columns = new List<string>();

        public ListView() : base()
        {
        }

        public void AddColumns(Expression<Func<T, object>> columnSelector)
        {
            var newColumns = GetPropertyNames(columnSelector);
            columns.AddRange(newColumns);
            UpdateView();
        }

        public void AddRow(T row)
        {
            data.Add(row);
            UpdateView();
        }

        public void RemoveRowByIndex(int index)
        {
            if (index >= 0 && index < data.Count)
            {
                data.RemoveAt(index);
                UpdateView();
            }
        }

        public int GetIndexOf(T key)
        {
            return data.IndexOf(key);
        }
        public int GetIndexOf(Func<T, bool> predicate)
        {
            return data.IndexOf(data.FirstOrDefault(predicate)!);
        }

        public void UpdateRowByIndex(int index, T newRow)
        {
            if (index >= 0 && index < data.Count)
            {
                data[index] = newRow;
                UpdateView();
            }
        }
        public T GetRow(Func<T, bool> predicate)
        {
            return data.FirstOrDefault(predicate)!;
        }
        public T GetRowByIndex(int index)
        {
            if (index >= 0 && index < data.Count)
            {
                return data[index];
            }
            return default!;
        }

        private void UpdateView()
        {
            var viewData = new List<string>();
            viewData.AddRange(columns);
            viewData.AddRange(data.Select(ObjectToRow));
            SetSource(viewData);
        }

        private static string ObjectToRow(T obj)
        {
            var properties = typeof(T).GetProperties();
            var values = properties.Select(p => p.GetValue(obj)?.ToString() ?? string.Empty);
            return string.Join("    ", values);
        }
        private static IEnumerable<string> GetPropertyNames(Expression<Func<T, object>> expression)
        {
            var memberExpression = expression.Body as NewExpression;
            if (memberExpression != null)
            {
                return memberExpression.Members!.Select(m => m.Name);
            }
            throw new ArgumentException("Expression is not a 'NewExpression'.");
        }
    }

}
