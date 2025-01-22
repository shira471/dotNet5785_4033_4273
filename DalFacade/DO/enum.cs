namespace DO
{
    /// <summary>
    /// Enumerator עבור סוגים שונים של "חמל" (לוגיסטיקה, ציוד, תחזוקה ועוד)
    /// </summary>
    public enum Hamal
    {
        handeled,
        cancelByVolunteer,
        cancelByManager,
        handelExpired,
        //Breakfast,            // חמל לארוחת בוקר
        //lunch,                // חמל לארוחת צהריים
        //dinner,               // חמל לארוחת ערב
        //madication,           // חמל לתרופות
        //medicalEquipment,     // חמל לציוד רפואי
        //militaryEquipment,    // חמל לציוד צבאי
        //OriginLocation,       // מיקום המקור
<<<<<<< HEAD
        ////role               // סוג תפקיד (מחובר לתפקיד המתנדב או המנהל)
=======
        ////role               // סוג תפקיד (מחובר לתפקיד המתנדב או המנהל
>>>>>>> c84dcb7de050db0682e47b6f35f66d201e5f2fe7
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

    public enum CallType
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

}
