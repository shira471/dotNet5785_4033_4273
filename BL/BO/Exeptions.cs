
namespace BO;
    using System;

    // חריגה במקרה של אובייקט לא קיים
    [Serializable]
    public class BlDoesNotExistException : Exception
    {
        // קונסטרוקטור שמקבל הודעת חריגה
        public BlDoesNotExistException(string? message) : base(message) { }

        // קונסטרוקטור שמקבל גם חריגה פנימית (exception)
        public BlDoesNotExistException(string message, Exception innerException)
                : base(message, innerException) { }
    }

    // חריגה במקרה של תכונה עם ערך null
    [Serializable]
    public class BlNullPropertyException : Exception
    {
        // קונסטרוקטור שמקבל הודעת חריגה
        public BlNullPropertyException(string? message) : base(message) { }
    }

    // חריגה במקרה של מספר מזהה קיים כבר
    [Serializable]
    public class BlAlreadyExistsException : Exception
    {
        // קונסטרוקטור שמקבל הודעת חריגה
        public BlAlreadyExistsException(string? message) : base(message) { }

        // קונסטרוקטור שמקבל גם חריגה פנימית (exception)
        public BlAlreadyExistsException(string message, Exception innerException)
                : base(message, innerException) { }
    }

    // חריגה במקרה של בעיה בתאריך או סדר תאריכים
    [Serializable]
    public class BlInvalidDateException : Exception
    {
        // קונסטרוקטור שמקבל הודעת חריגה
        public BlInvalidDateException(string? message) : base(message) { }
    }

    // חריגה במקרה של ערך לא תקין
    [Serializable]
    public class BlInvalidValueException : Exception
    {
    public BlInvalidValueException(string? message) : base(message)
    {
    }

    // קונסטרוקטור שמקבל הודעת חריגה
    public BlInvalidValueException(string? message, Exception ex) : base(message) { }
    }


