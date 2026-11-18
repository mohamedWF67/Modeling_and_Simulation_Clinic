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

    // ===================== Simulation (minute by minute) =====================

    // Queues (patients in the order they joined)
    public List<Patient> nurseQueue = new();
    public List<Patient> middleAgeQueue = new();   // waiting for Doctor 1 (or Doctor 2 if free)
    public List<Patient> elderlyQueue = new();     // waiting for Doctor 2

    // Longest each queue ever got
    public int maxNurseQueue = 0;
    public int maxMiddleAgeQueue = 0;
    public int maxElderlyQueue = 0;

    // Nurse state
    public Patient nursePatient = null;   // patient with the nurse now (null = nurse free)
    public int nurseFreeAt = 0;           // time the nurse finishes the current patient
    public int nurseBusyMinutes = 0;

    // Calendar table: every event in the order it happened
    public List<CalendarEntry> calendar = new();

    // Time the last patient finished (length of the simulation)
    public int simulationEndTime = 0;

    /// <summary>
    /// Runs the simulation for all created patients.
    /// The clock moves forward one minute at a time. At each minute we:
    /// 1) finish the nurse and the doctors whose service ends now,
    /// 2) add the patients who arrive now to the nurse queue,
    /// 3) start the nurse and free doctors on the next patients,
    /// 4) check whether a middle-age patient waited more than 20 minutes and leaves,
    /// 5) update the max queue lengths and the busy minutes.
    /// All times in this problem are whole minutes, so stepping by 1 minute is exact.
    /// </summary>
    public void RunSimulation(bool print = false)
    {
        // Arrival time = previous arrival + inter-arrival time (first patient arrives at 0)
        int arrival = 0;
        foreach (Patient p in patients)
        {
            arrival += p.iat;
            p.arrivalTime = arrival;
        }

        int finished = 0;   // patients that were examined or left
        int nextArrival = 0; // index of the next patient to arrive
        int time = 0;

        while (finished < patients.Count)
        {
            // ---- 1) End of service ----
            if (nursePatient != null && nurseFreeAt == time)
            {
                AddEvent(time, "Nurse end", nursePatient, "goes to doctor queue");
                if (nursePatient.type == patientType.Elderly)
                    elderlyQueue.Add(nursePatient);
                else
                    middleAgeQueue.Add(nursePatient);
                nursePatient = null;
            }

            foreach (Doctor d in Doctors)
            {
                if (d.currentPatient != null && d.freeAt == time)
                {
                    AddEvent(time, "Doctor end", d.currentPatient, d.name);
                    d.currentPatient = null;
                    finished++;
                }
            }

            // ---- 2) Arrivals ----
            while (nextArrival < patients.Count && patients[nextArrival].arrivalTime == time)
            {
                Patient p = patients[nextArrival];
                nurseQueue.Add(p);
                AddEvent(time, "Arrival", p, p.type.ToString());
                nextArrival++;
            }

            // ---- 3) Start of service ----
            // Nurse: elderly patients first, otherwise the first in line
            if (nursePatient == null && nurseQueue.Count > 0)
            {
                Patient p = nurseQueue.Find(x => x.type == patientType.Elderly);
                if (p == null)
                    p = nurseQueue[0];

                nurseQueue.Remove(p);
                p.StartNurse(time);
                nursePatient = p;
                nurseFreeAt = p.n_end;
                AddEvent(time, "Nurse start", p, "");
            }

            // Doctors: Doctor 1 comes first in the list, so Doctor 2 only takes a
            // middle-age patient when Doctor 1 is busy and no elderly patient is waiting.
            foreach (Doctor d in Doctors)
            {
                if (d.currentPatient != null)
                    continue;

                Patient p = null;
                if (d.curesElderly && elderlyQueue.Count > 0)
                {
                    p = elderlyQueue[0];
                    elderlyQueue.RemoveAt(0);
                }
                else if (d.curesMiddleAge && middleAgeQueue.Count > 0)
                {
                    p = middleAgeQueue[0];
                    middleAgeQueue.RemoveAt(0);
                }

                if (p != null)
                {
                    p.StartDoctor(d, time);
                    d.currentPatient = p;
                    d.freeAt = p.d_end;
                    AddEvent(time, "Doctor start", p, d.name);
                }
            }

            // ---- 4) Leaving ----
            // A middle-age patient who has waited more than 20 minutes for the doctor
            // draws one random number; 1-20 (probability 0.2) means he leaves.
            foreach (Patient p in middleAgeQueue.ToList())
            {
                bool waitedTooLong = time - p.n_end > MaxWaitBeforeLeaving;
                if (waitedTooLong && p.R_patient_leave == Patient.Empty)
                {
                    p.R_patient_leave = NextInt();
                    if (p.R_patient_leave <= LeaveProbabilityPercent)
                    {
                        middleAgeQueue.Remove(p);
                        p.Leave(time);
                        finished++;
                        AddEvent(time, "Leave", p, "RN = " + p.R_patient_leave);
                    }
                }
            }

            // ---- 5) Statistics for this minute ----
            maxNurseQueue = Math.Max(maxNurseQueue, nurseQueue.Count);
            maxMiddleAgeQueue = Math.Max(maxMiddleAgeQueue, middleAgeQueue.Count);
            maxElderlyQueue = Math.Max(maxElderlyQueue, elderlyQueue.Count);

            if (nursePatient != null)
                nurseBusyMinutes++;
            foreach (Doctor d in Doctors)
                if (d.currentPatient != null)
                    d.busyMinutes++;

            if (finished < patients.Count)
                time++;
        }

        simulationEndTime = time;

        if (print)
        {
            foreach (Patient p in patients)
                p.print();
            foreach (CalendarEntry e in calendar)
                e.print();
        }
    }

    /// <summary>Adds one row to the calendar table.</summary>
    private void AddEvent(int time, string eventName, Patient p, string details)
    {
        calendar.Add(new CalendarEntry(time, eventName, p.id, details));
    }

    /// <summary>Random number from 1 to 100.</summary>
    private int NextInt() => rng.NextInt(1, 100);
}
