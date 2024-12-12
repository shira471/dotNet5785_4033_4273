using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    internal static class ExceptionsManager
    {
        /// <summary>
        /// דוגמה לטיפול בשגיאות בצורה ממוקדת עבור שכבת ה-BL
        /// </summary>
        internal static string HandleException(Exception ex)
        {
            return $"Error: {ex.Message}"; // דוגמה: החזרת הודעת שגיאה מפורטת
        }
    }
}
