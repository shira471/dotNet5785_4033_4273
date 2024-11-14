
namespace DO;

public record Volunteer(
    int idVol,
    string adress,
    string name,
    string email ,
    double phoneNumber ,
    string password ,
    double latitude,
    double longitude,
    double limitDestenation,
    bool isActive = false,
    Hamal? role=null,
    Hamal? distanceType = null
)
{
  public Volunteer() : this(0, "", "", "", 0, "", 0, 0, 0) { }
}
