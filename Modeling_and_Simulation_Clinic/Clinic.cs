using Modeling_and_Simulation_Clinic.Entities;
using Modeling_and_Simulation_Clinic.Random;
using patientType = Modeling_and_Simulation_Clinic.Entities.Patient.patientType;
namespace Modeling_and_Simulation_Clinic;
public class Clinic
{
    public string name;
    private LCG rng = new (123456);
    
    private DiscreteDistribution Patientype;
    private DiscreteDistribution IAT;
    private DiscreteDistribution MiddleAgedExamTime;
    private DiscreteDistribution ElderlyExamTime;
    
    public List<Patient> patients = new ();
    public List<Doctor> Doctors = new();
    
    public int last_patient_id = -1;
    public int last_arrival_time = 0;
    public int last_nurse_end_time = 0;

    public Clinic(string name) => this.name = name;

    public void BuildDistributionTable()
    {
        Patientype = new DiscreteDistribution
        (
            [0, 1], [0.70, 0.30]
        );

        IAT = new DiscreteDistribution
        (
            [2,5, 7, 10], [0.15, 0.25, 0.30,0.30]
        );
        
        MiddleAgedExamTime = new DiscreteDistribution
        (
            [5, 7, 10], [0.20, 0.40, 0.40]
        );
        
        ElderlyExamTime = new DiscreteDistribution
        (
            [7, 10, 15], [0.20, 0.50, 0.30]
        );
    }

    public void InitializeDoctors()
    {
        Doctors.Add(
            new Doctor("D1", false, true)
            );
        Doctors.Add(
            new Doctor("D2", true, true)
            );
    }

    public void CreatePatient(bool print = false)
    {
        Patient p = new Patient(patients.Count + 1);
        
        p.R_patient_type = NextInt();
        p.type = (patientType)Patientype.Sample(p.R_patient_type);

        if (patients.Count == 0)
        {
            p.R_patient_iat = -1;
            p.iat = 0;
        }
        else
        {
            p.R_patient_iat = NextInt();
            p.iat = IAT.Sample(p.R_patient_iat);
        }
        
        p.R_patient_exam_time = NextInt();
        p.exam_time = 
            p.type == patientType.MiddleAge 
                ?
                MiddleAgedExamTime.Sample(p.R_patient_exam_time) 
                :
                ElderlyExamTime.Sample(p.R_patient_exam_time);

        patients.Add(p);
        if (print)
            patients.Last().print();
    }

    public void createPatients(int amount, bool print = false)
    {
        if (amount <= 0) 
            throw new ArgumentException("Amount must be greater than 0");
        for (int i = 0; i < amount; i++)
        {
            CreatePatient(print);
        }
    }

    public void computePatientOutcomes()
    {
        last_patient_id++;
        patients[last_patient_id].ComputeNurse(last_arrival_time, last_nurse_end_time);
        assignDoctor(last_patient_id);
        last_arrival_time = patients[last_patient_id].arrivalTime;
        last_nurse_end_time = patients[last_patient_id].n_end;

        patients[last_patient_id].print();
    }

    public void computePatientOutcomes(int amount)
    {
        if (amount <= 0) 
            throw new ArgumentException("Amount must be greater than 0");
        if (amount > patients.Count) 
            amount = patients.Count;
        
        for (int i = 0; i < amount; i++)
        {
            computePatientOutcomes();
        }
    }

    public void assignDoctor(int patient_id)
    {
        if (patient_id < 0 || patient_id > patients.Count)
            throw new ArgumentException("Patient ID must be between 1 and the number of patients");

        Patient p = patients[patient_id];

        Doctor d = null;
        if (p.type == patientType.MiddleAge)
        {
            d = Doctors.Find(d => d.curesMiddleAge);
        }
        else
        {
            d = Doctors.Find(d => d.curesElderly);
        }
        
        if (d == null)
            throw new Exception("No doctor available");
        
        patients[patient_id].ComputeDoctor(d);
    }
    
    private int NextInt() => rng.NextInt(1, 100);
}