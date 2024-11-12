
namespace DO;

public record Volunteer(
    int idVol=0,
    string? adress = null,
    string name="",
    string email = "",
    int phoneNumber = 0,
    string? password = null,
    double? latitude=null,
    double? longitude=null,
    bool isActive = false,
    double? limitDestenation = null,
    Hamal role=0,
    Hamal distanceType = 0
)
{
  
}
