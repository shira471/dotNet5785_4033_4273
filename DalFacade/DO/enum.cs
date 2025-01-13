namespace DO
{
    /// <summary>
    /// Enumerator עבור סוגים שונים של "חמל" (לוגיסטיקה, ציוד, תחזוקה ועוד)
    /// </summary>
    public enum Hamal
    {
        Breakfast,            // חמל לארוחת בוקר
        lunch,                // חמל לארוחת צהריים
        dinner,               // חמל לארוחת ערב
        madication,           // חמל לתרופות
        medicalEquipment,     // חמל לציוד רפואי
        militaryEquipment,    // חמל לציוד צבאי
        OriginLocation,       // מיקום המקור
        //role               // סוג תפקיד (מחובר לתפקיד המתנדב או המנהל)
    }

    /// <summary>
    /// Enumerator עבור תפקידי משתמשים במערכת
    /// </summary>
    public enum Role
    {
        Manager,    // תפקיד של מנהל
        Volunteer   // תפקיד של מתנדב
    }

    /// <summary>
    /// Defines the type of distance calculation methods available.
    /// </summary>
    public enum TypeDistance
    {
        air,
        walking,
        driving
    }
}
