using NStack;
using ReactiveUI;

namespace VegaExpress.Agent.Converters
{
    public class IntToUStringConverter : IBindingTypeConverter
    {
        public int GetAffinityForObjects(Type fromType, Type toType)
        {
            if (fromType == typeof(int) && toType == typeof(ustring))
            {
                return 100; // return a number > 0 for a valid conversion
            }

            return 0; // return 0 for an invalid conversion
        }

        public bool TryConvert(object? from, Type toType, object? conversionHint, out object result)
        {
            if (toType == typeof(ustring) && from is int)
            {
                result = ustring.Make((int)from);
                return true;
            }

            result = null!;
            return false;
        }
    }
}
