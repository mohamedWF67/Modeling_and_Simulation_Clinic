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
    /// <summary>Random number for the leave decision; only drawn if the doctor wait would exceed 20 min.</summary>
    public int R_patient_leave = Empty;

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

    /// <summary>True if the patient left without seeing a doctor.</summary>
    public bool leftClinic;
    /// <summary>Time the patient left without examination (Empty if examined).</summary>
    public int leaveTime = Empty;

    public Patient() => id = Guid.NewGuid().GetHashCode();

    public Patient(int id) => this.id = id;

    public void print()
    {
        Console.WriteLine($"Patient {id} Info:");
        Console.WriteLine($"Type: {R_patient_type} IAT: {R_patient_iat} Exam Time: {R_patient_exam_time}");
        Console.WriteLine($"Type: {type} IAT: {iat} Exam Time: {exam_time}");
        Console.WriteLine($"{arrivalTime} {n_start} {n_time} {n_end} {n_wait}");
        Console.WriteLine($"{d_name} {d_start} {d_end} {d_wait} {totalWait} {timeInSystem}");
        if (R_patient_leave != Empty)
            Console.WriteLine($"Leave RN: {R_patient_leave} Left: {leftClinic} Leave time: {(leftClinic ? leaveTime : Empty)}");
        Console.WriteLine();
    }

    /// <summary>
    /// Computes the nurse stage. Arrival time must already be set.
    /// </summary>
    /// <param name="nurseFreeAt">Time the nurse finishes her previous patient.</param>
    public void ComputeNurse(int nurseFreeAt)
    {
        n_start = Math.Max(arrivalTime, nurseFreeAt);
        n_time = type == patientType.MiddleAge ? 2 : 4;
        n_end = n_start + n_time;
        n_wait = n_start - arrivalTime;
    }

    public void ComputeDoctor(Doctor d)
    {
        d_name = d.name;
        d_start = d.freeAt >= n_end ? n_end + (d.freeAt - n_end) : n_end;
        d_end = d_start + exam_time;
        d_wait = d_start - n_end;
        
        d.freeAt = d_end;
        
        totalWait = n_wait + d_wait;
        timeInSystem = d_end - arrivalTime;
    }

    /// <summary>
    /// Marks the patient as leaving the clinic without a doctor examination.
    /// The patient waited up to the leave time, so that wait is still counted.
    /// </summary>
    /// <param name="time">Clock time the patient leaves.</param>
    public void Leave(int time)
    {
        leftClinic = true;
        leaveTime = time;

        d_name = "Left";
        d_start = Empty;
        d_end = Empty;
        d_wait = time - n_end;

        totalWait = n_wait + d_wait;
        timeInSystem = time - arrivalTime;
    }
}