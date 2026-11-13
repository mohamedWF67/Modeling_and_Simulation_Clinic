using Modeling_and_Simulation_Clinic.Random;

namespace Modeling_and_Simulation_Clinic.Entities;

public class Patient
{
    public const int Empty = -1;

    public enum patientType
    {
        MiddleAge = 0,
        Elderly = 1
    }

    public int id;

    public int R_patient_type;
    public int R_patient_iat;
    public int R_patient_exam_time;

    public patientType type;
    public int iat;
    public int exam_time;

    public int arrivalTime;
    public int n_start;
    public int n_time;
    public int n_end;
    public int n_wait;

    public string d_name;
    public int d_start;
    public int d_end;
    public int d_wait;

    public int totalWait;
    public int timeInSystem;

    public Patient() => id = Guid.NewGuid().GetHashCode();

    public Patient(int id) => this.id = id;

    public void print()
    {
        Console.WriteLine($"Patient {id} Info:");
        Console.WriteLine($"Type: {R_patient_type} IAT: {R_patient_iat} Exam Time: {R_patient_exam_time}");
        Console.WriteLine($"Type: {type} IAT: {iat} Exam Time: {exam_time}");
        Console.WriteLine($"{arrivalTime} {n_start} {n_time} {n_end} {n_wait}");
        Console.WriteLine($"{d_name} {d_start} {d_end} {d_wait} {totalWait} {timeInSystem}");
        Console.WriteLine();
    }

    public void ComputeNurse(int alast, int nlast)
    {
        arrivalTime = iat == 0 ? 0 : alast + iat;
        n_start = arrivalTime >= nlast ? arrivalTime : nlast;
        n_time = type == patientType.MiddleAge ? 2 : 4;
        n_end = n_start + n_time;
        n_wait = n_end - arrivalTime - n_time;
    }

    public void ComputeDoctor(Doctor d)
    {
        d_name = d.name;
        d_start = d.freeAt >= n_end ? n_end + (d.freeAt - n_end) : n_end;
        d_end = d_start + exam_time;
        d_wait = d_end - exam_time - d_start;
        
        d.freeAt = d_end;
        
        totalWait = n_wait + d_wait;
        timeInSystem = d_end - arrivalTime;
    }

}