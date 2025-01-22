using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL;

/// <summary>
/// חלון המאפשר למשתמש לבחור פעולה (עדכון, מחיקה או ביטול) עבור ישות מסוימת.
/// </summary>
public partial class ActionSelectionWindow : Window
{
    // משתנה המציין אם המשתמש בחר בפעולה של עדכון.
    public bool IsUpdate { get; private set; }

    // משתנה המציין אם המשתמש בחר בפעולה של מחיקה.
    public bool IsDelete { get; private set; }

    // משתנה המציין אם המשתמש בחר לבטל את הפעולה.
    public bool IsCancel { get; private set; }

    // הודעה מותאמת אישית המוצגת למשתמש עם שם הישות הרלוונטית.
    public string ActionMessage { get; set; }

    /// <summary>
    /// קונסטרקטור המקבל את שם הישות עליה מבוצעת הפעולה.
    /// </summary>
    /// <param name="entityName">שם הישות שעבורה מתבצעת הבחירה.</param>
    public ActionSelectionWindow(string entityName)
    {
        InitializeComponent();

        // הגדרת ההודעה שתוצג למשתמש עם שם הישות.
        ActionMessage = $"Select an action for the {entityName}:";

        // הגדרת ה-DataContext של החלון כדי לאפשר Binding בין הנתונים לממשק המשתמש.
        DataContext = this;
    }

    /// <summary>
    /// פעולה שנקראת כאשר המשתמש לוחץ על כפתור "Update".
    /// </summary>
    private void Update_Click(object sender, RoutedEventArgs e)
    {
        IsUpdate = true; // סימון שהמשתמש בחר בעדכון.
        DialogResult = true; // החזרת ערך חיובי לדיאלוג.
        Close(); // סגירת החלון.
    }

    /// <summary>
    /// פעולה שנקראת כאשר המשתמש לוחץ על כפתור "Delete".
    /// </summary>
    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        IsDelete = true; // סימון שהמשתמש בחר במחיקה.
        DialogResult = true; // החזרת ערך חיובי לדיאלוג.
        Close(); // סגירת החלון.
    }

    /// <summary>
    /// פעולה שנקראת כאשר המשתמש לוחץ על כפתור "Cancel".
    /// </summary>
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        IsCancel = true; // סימון שהמשתמש בחר בביטול.
        DialogResult = false; // החזרת ערך שלילי לדיאלוג (מציין ביטול פעולה).
        Close(); // סגירת החלון.
    }
}
