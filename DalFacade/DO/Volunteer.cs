﻿namespace DO;

public record Volunteer(
    int idVol,                     // מזהה המתנדב
    string adress,                 // כתובת המתנדב
    string name,                   // שם המתנדב
    string email,                  // דוא"ל המתנדב
    string phoneNumber,            // מספר טלפון המתנדב
    string password,               // סיסמת המתנדב
    double latitude,               // קו רוחב
    double longitude,              // קו אורך
    double limitDestenation,       // מגבלת המרחק
    bool isActive =false,         // האם פעיל  
    Role? role =Role.Volunteer, // תפקיד
    TypeDistance? distanceType = TypeDistance.air   // סוג המרחק
)
{
    // בנאי ברירת מחדל
    public Volunteer() : this(0, "", "", "", "", "", 0, 0, 0,false, Role.Volunteer, TypeDistance.air) {
    }
}