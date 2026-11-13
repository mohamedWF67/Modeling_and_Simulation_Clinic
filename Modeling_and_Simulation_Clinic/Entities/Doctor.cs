namespace Modeling_and_Simulation_Clinic.Entities;

public class Doctor
{
    public string name;
    public int freeAt = 0;
    public bool curesElderly;
    public bool curesMiddleAge;

    public Doctor(string name, bool curesElderly, bool curesMiddleAge)
    {
        this.name = name;
        this.curesElderly = curesElderly;
        this.curesMiddleAge = curesMiddleAge;
    }
}