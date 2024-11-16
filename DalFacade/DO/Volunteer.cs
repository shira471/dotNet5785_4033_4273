//namespace DO;

//public record Volunteer(
//    int idVol,
//    string adress,
//    string name,
//    string email ,
//    double phoneNumber ,
//    string password ,
//    double latitude,
//    double longitude,
//    double limitDestenation,
//    bool isActive = false,
//    Hamal? role=null,
//    Hamal? distanceType = null
//)
//{
//  public Volunteer() : this(0, "", "", "", 0, "", 0, 0, 0) { }
//}


namespace DO;

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
    bool isActive = false,         // האם פעיל
    Hamal? role = null,            // תפקיד
    Hamal? distanceType = null     // סוג המרחק
)
{
    // בנאי ברירת מחדל
    public Volunteer() : this(0, "", "", "", "", "", 0, 0, 0) { }
}
