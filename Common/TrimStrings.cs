using System.Linq;

namespace CRM_Sample.Common
{
    public class TrimStrings
    {
        public static void TrimStringsFunction(object obj)
        {
            var stringProperties = obj.GetType().GetProperties()
              .Where(p => p.PropertyType == typeof(string) && p.GetValue(obj) != null && p.Name != "FullName");

            foreach (var stringProperty in stringProperties)
            {
                string currentValue = (string)stringProperty.GetValue(obj);
                stringProperty.SetValue(obj, currentValue.Trim(), null);

            }
        }
    }
}
