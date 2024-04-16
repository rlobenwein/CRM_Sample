using System.Linq;

namespace RLBW_ERP.Common
{
    public class TrimStrings
    {
        public static object TrimStringsFunction(object obj)
        {
            var stringProperties = obj.GetType().GetProperties()
              .Where(p => p.PropertyType == typeof(string) && p.GetValue(obj) != null && p.Name != "FullName");

            foreach (var stringProperty in stringProperties)
            {
                string currentValue = (string)stringProperty.GetValue(obj);
                stringProperty.SetValue(obj, currentValue.Trim(), null);

            }
            return obj;
        }
    }
}
