using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BL.Helpers
{
    internal static class Tools
    {

        // פונקציה זו יוצרת מחרוזת המפרטת את כל התכונות של אובייקט מסוים
        // כולל ערכיהן, ואף נכנסת לעומק אם מדובר באוספים
        public static string ToStringProperty<T>(this T t)
        {
            if (t == null)
                return string.Empty;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var sb = new StringBuilder();

            sb.AppendLine($"Properties of {typeof(T).Name}:");

            foreach (var property in properties)
            {
                var value = property.GetValue(t);
                if (value is System.Collections.IEnumerable enumerable && !(value is string))
                {
                    sb.AppendLine($"  {property.Name}: [");
                    foreach (var item in enumerable)
                        sb.AppendLine($"    {item},");
                    sb.AppendLine("  ]");
                }
                else
                {
                    sb.AppendLine($"  {property.Name}: {value}");
                }
            }

            return sb.ToString();
        }

        // פונקציה שבודקת אם אוסף (collection) הוא ריק או null
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
        {
            return collection == null || !collection.Any();
        }

        // פונקציה זו מבצעת שכפול עמוק של אובייקט
        // על ידי סריאליזציה ל-JSON ודסיריאליזציה חזרה
        public static T? DeepClone<T>(this T obj)
        {
            if (obj == null)
                return default;

            // ממיר את האובייקט ל-JSON
            var json = System.Text.Json.JsonSerializer.Serialize(obj);

            // ממיר את ה-JSON בחזרה לאובייקט
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }

    }




   
}
