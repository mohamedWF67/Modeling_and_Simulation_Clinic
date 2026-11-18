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

    /// <summary>A middle-age patient may leave if the doctor wait would exceed this (minutes).</summary>
    public const int MaxWaitBeforeLeaving = 20;
    /// <summary>Chance (in %) that such a patient actually leaves: RN 1-20 out of 1-100 = 0.2.</summary>
    public const int LeaveProbabilityPercent = 20;
    

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

    /// <summary>
    /// Runs the nurse and doctor stages for the first <paramref name="amount"/> patients.
    /// The nurse serves elderly patients first: whenever she becomes free, any elderly
    /// patient already waiting goes in before waiting middle-age patients
    /// (first come, first served within each type).
    /// </summary>
    /// <param name="amount">Number of patients to simulate.</param>
    /// <param name="print">Print each patient after the doctor stage.</param>
    public void computePatientsOutcomes(int amount, bool print = true)
    {
        if (amount <= 0) 
            throw new ArgumentException("Amount must be greater than 0");
        if (amount > patients.Count) 
            amount = patients.Count;

        List<Patient> batch = patients.Take(amount).ToList();

        // 1) Arrival times: running sum of inter-arrival times (first patient arrives at 0).
        int clock = 0;
        foreach (Patient p in batch)
        {
            clock += p.iat;
            p.arrivalTime = clock;
        }

        // 2) Nurse stage with elderly priority.
        List<Patient> waiting = new(batch);   // patients not yet seen by the nurse (arrival order)
        List<Patient> nurseOrder = new();     // order in which the nurse served them
        int nurseFreeAt = 0;

        while (waiting.Count > 0)
        {
            // If nobody has arrived yet, the nurse idles until the next arrival.
            if (waiting[0].arrivalTime > nurseFreeAt)
                nurseFreeAt = waiting[0].arrivalTime;

            // Among those already in the clinic, take the earliest elderly patient; otherwise the earliest middle-age one.
            Patient next =
                waiting.FirstOrDefault(w => w.arrivalTime <= nurseFreeAt && w.type == patientType.Elderly)
                ?? waiting[0];

            next.ComputeNurse(nurseFreeAt);
            nurseFreeAt = next.n_end;

            waiting.Remove(next);
            nurseOrder.Add(next);
        }

        // 3) Doctor stage: patients reach the doctors in the order they leave the nurse.
        foreach (Patient p in nurseOrder)
        {
            assignDoctor(patients.IndexOf(p));
            if (print)
                p.print();
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
            // Middle-age patients belong to Doctor 1 (the doctor that does not treat elderly).
            Doctor main = Doctors.Find(doc => doc.curesMiddleAge && !doc.curesElderly);
            // Doctor 2 (treats elderly) may help, but only if no elderly patient is waiting.
            Doctor helper = Doctors.Find(doc => doc.curesMiddleAge && doc.curesElderly);

            bool mainBusy = main != null && main.freeAt > p.n_end;
            // Doctor 2 free when this patient is ready means every elderly patient
            // assigned so far is already done, so no elderly patient is waiting.
            bool helperIdle = helper != null && helper.freeAt <= p.n_end;

            d = (mainBusy && helperIdle) ? helper : main;
        }
        else
        {
            d = Doctors.Find(doc => doc.curesElderly);
        }
        
        if (d == null)
            throw new Exception("No doctor available");

        // Reneging: a middle-age patient who would wait more than 20 minutes for the
        // doctor leaves with probability 0.2 (random number 1-20). The doctor is not
        // occupied by a patient who leaves.
        if (p.type == patientType.MiddleAge)
        {
            int expectedWait = Math.Max(d.freeAt, p.n_end) - p.n_end;
            if (expectedWait > MaxWaitBeforeLeaving)
            {
                p.R_patient_leave = NextInt();
                if (p.R_patient_leave <= LeaveProbabilityPercent)
                {
                    p.Leave(p.n_end + MaxWaitBeforeLeaving);
                    return;
                }
            }
        }

        p.ComputeDoctor(d);
    }
    
    private int NextInt() => rng.NextInt(1, 100);
}