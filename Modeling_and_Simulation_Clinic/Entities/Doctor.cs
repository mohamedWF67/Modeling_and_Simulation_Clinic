namespace Modeling_and_Simulation_Clinic.Entities;

/// <summary>
/// A doctor in one examination room.
/// curesElderly / curesMiddleAge say which patients this doctor can examine.
/// </summary>
public class Doctor
{
    public string name;
    public bool curesElderly;
    public bool curesMiddleAge;

    public Patient currentPatient = null;   // patient being examined now (null = free)
    public int freeAt = 0;                  // time the current examination ends
    public int busyMinutes = 0;             // total minutes spent examining (for idle time)

    public Doctor(string name, bool curesElderly, bool curesMiddleAge)
    {
        this.name = name;
        this.curesElderly = curesElderly;
        this.curesMiddleAge = curesMiddleAge;
    }
}
