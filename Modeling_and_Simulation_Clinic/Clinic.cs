using Modeling_and_Simulation_Clinic.Entities;
using Modeling_and_Simulation_Clinic.Random;
using patientType = Modeling_and_Simulation_Clinic.Entities.Patient.patientType;
namespace Modeling_and_Simulation_Clinic;

/// <summary>
/// The medical clinic: one nurse, two doctors (three for Q8), the queues, the rules of
/// the problem, the minute-by-minute simulation and the statistics of one run.
/// </summary>
public class Clinic
{
    public string name;
    /// <summary>Seed used when no seed is given to the constructor.</summary>
    public const ulong DefaultSeed = 123456;

    private LCG rng = new LCG(DefaultSeed);

    public DiscreteDistribution PatientType { get; private set; }
    public DiscreteDistribution IAT { get; private set; }
    public DiscreteDistribution MiddleAgedExamTime { get; private set; }
    public DiscreteDistribution ElderlyExamTime { get; private set; }
    
    public List<Patient> patients = new ();
    public List<Doctor> Doctors = new();

    /// <summary>A middle-age patient may leave if the doctor wait would exceed this (minutes).</summary>
    public const int MaxWaitBeforeLeaving = 20;
    /// <summary>Chance (in %) that such a patient actually leaves: RN 1-20 out of 1-100 = 0.2.</summary>
    public const int LeaveProbabilityPercent = 20;
    

    /// <summary>Creates a clinic that uses the default seed.</summary>
    public Clinic(string name) => this.name = name;

    /// <summary>Creates a clinic whose random numbers start from the given seed.</summary>
    public Clinic(string name, ulong seed) : this(name) => rng = new LCG(seed);

    /// <summary>
    /// Uses the given distribution tables (for example ones edited in the GUI).
    /// Call this instead of BuildDistributionTable, before creating the patients.
    /// </summary>
    public void SetDistributionTables(DiscreteDistribution patientType, DiscreteDistribution iat,
        DiscreteDistribution middleAgedExamTime, DiscreteDistribution elderlyExamTime)
    {
        PatientType = patientType;
        IAT = iat;
        MiddleAgedExamTime = middleAgedExamTime;
        ElderlyExamTime = elderlyExamTime;
    }

    /// <summary>Builds the four distribution tables given in the project brief.</summary>
    public void BuildDistributionTable()
    {
        PatientType = new DiscreteDistribution
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

    /// <summary>
    /// Creates the doctors. D1 examines middle-age patients only. D2 examines elderly patients
    /// first and helps with middle-age patients when no elderly patient is waiting.
    /// With 3 doctors (Q8) the extra doctor D3 is a second middle-age doctor.
    /// The list order matters: free doctors take patients in this order, so D2 helps last.
    /// </summary>
    /// <param name="doctorCount">2 (the clinic as described) or 3 (Q8).</param>
    public void InitializeDoctors(int doctorCount = 2)
    {
        if (doctorCount < 2 || doctorCount > 3)
            throw new ArgumentException("The clinic has 2 or 3 doctors.");

        Doctors.Add(new Doctor("D1", false, true));
        if (doctorCount == 3)
            Doctors.Add(new Doctor("D3", false, true));
        Doctors.Add(new Doctor("D2", true, true));
    }

    /// <summary>
    /// Creates the next patient: draws the random numbers for the type, the inter-arrival
    /// time (not for the first patient, who arrives at 0) and the exam time.
    /// </summary>
    public void CreatePatient(bool print = false)
    {
        Patient p = new Patient(patients.Count + 1);
        
        p.R_patient_type = NextInt();
        p.type = (patientType)PatientType.Sample(p.R_patient_type);

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

    /// <summary>Creates <paramref name="amount"/> patients (call before RunSimulation).</summary>
    public void CreatePatients(int amount, bool print = false)
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

            // Doctors: D2 is last in the list, so it only takes a middle-age patient when
            // every middle-age doctor is busy and no elderly patient is waiting.
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
        ComputeStatistics();
        if (print)
        {
            foreach (Patient p in patients)
                p.print();
            foreach (CalendarEntry e in calendar)
                e.print();
        }
    }
    
    #region Statistics

    // Sums and counts of one run (shown as "sum / count" for the first run)
    public int ExamTimeSum, ExamCount;                    // Q1: examined patients only
    public int MiddleExamTimeSum, MiddleExamCount;
    public int ElderlyExamTimeSum, ElderlyExamCount;

    public int WaitSum, PatientCount;                     // Q2: every patient, also those who left
    public int MiddleWaitSum, MiddleCount;
    public int ElderlyWaitSum, ElderlyCount;

    public int NurseWaitCount;                            // Q4: patients who waited for the nurse
    public int MiddleDoctorWaitCount;                     // middle-age patients who waited for a doctor
    public int ElderlyDoctorWaitCount;                    // elderly patients who waited for a doctor

    public int NurseMiddleTimeSum, NurseElderlyTimeSum;   // Q6: time with the nurse
    public int IatSum;                                    // Q7: inter-arrival times
    public int LeftCount;                                 // middle-age patients who left without examination

    // Results of one run
    public double MeanExamTime, MeanMiddleExamTime, MeanElderlyExamTime;        // Q1
    public double MeanWait, MeanWaitMiddleAge, MeanWaitElderly;                 // Q2
    public double ProbNurseWait, ProbMiddleDoctorWait, ProbElderlyDoctorWait;   // Q4
    public double PortionNurseIdle;                                             // Q5
    public double PortionMiddleDoctorIdle;    // D1
    public double PortionElderlyDoctorIdle;   // D2
    public double PortionExtraDoctorIdle;     // D3 (0 when the clinic has 2 doctors)
    public double NurseMiddleAgeAvgTime, NurseElderlyAvgTime;                   // Q6
    public double MeanIat;                                                      // Q7
    public double LeftPercent;                // % of middle-age patients who left

    /// <summary>
    /// Computes the answers to Q1-Q7 for this run. Called once at the end of RunSimulation.
    /// Every division goes through Divide, so a run with (for example) no elderly patients
    /// gives 0 instead of NaN.
    /// </summary>
    private void ComputeStatistics()
    {
        foreach (Patient p in patients)
        {
            // Q2, Q4, Q6 (nurse) and Q7 count every patient, also the ones who left
            PatientCount++;
            WaitSum += p.totalWait;
            IatSum += p.iat;
            if (p.n_wait > 0)
                NurseWaitCount++;

            if (p.type == patientType.MiddleAge)
            {
                MiddleCount++;
                MiddleWaitSum += p.totalWait;
                NurseMiddleTimeSum += p.n_time;
                if (p.d_wait > 0)
                    MiddleDoctorWaitCount++;
                if (p.leftClinic)
                    LeftCount++;
            }
            else
            {
                ElderlyCount++;
                ElderlyWaitSum += p.totalWait;
                NurseElderlyTimeSum += p.n_time;
                if (p.d_wait > 0)
                    ElderlyDoctorWaitCount++;
            }

            // Q1 only counts patients who were examined by a doctor
            if (p.leftClinic)
                continue;

            ExamCount++;
            ExamTimeSum += p.exam_time;
            if (p.type == patientType.MiddleAge)
            {
                MiddleExamCount++;
                MiddleExamTimeSum += p.exam_time;
            }
            else
            {
                ElderlyExamCount++;
                ElderlyExamTimeSum += p.exam_time;
            }
        }

        MeanExamTime = Divide(ExamTimeSum, ExamCount);
        MeanMiddleExamTime = Divide(MiddleExamTimeSum, MiddleExamCount);
        MeanElderlyExamTime = Divide(ElderlyExamTimeSum, ElderlyExamCount);

        MeanWait = Divide(WaitSum, PatientCount);
        MeanWaitMiddleAge = Divide(MiddleWaitSum, MiddleCount);
        MeanWaitElderly = Divide(ElderlyWaitSum, ElderlyCount);

        ProbNurseWait = Divide(NurseWaitCount, PatientCount);
        ProbMiddleDoctorWait = Divide(MiddleDoctorWaitCount, MiddleCount);
        ProbElderlyDoctorWait = Divide(ElderlyDoctorWaitCount, ElderlyCount);

        // Q5: idle = 1 - busy minutes / length of the run, worked out for every doctor
        PortionNurseIdle = 1 - Divide(nurseBusyMinutes, simulationEndTime);
        foreach (Doctor d in Doctors)
            d.idlePortion = 1 - Divide(d.busyMinutes, simulationEndTime);
        PortionMiddleDoctorIdle = FindDoctor("D1")?.idlePortion ?? 0;
        PortionElderlyDoctorIdle = FindDoctor("D2")?.idlePortion ?? 0;
        PortionExtraDoctorIdle = FindDoctor("D3")?.idlePortion ?? 0;

        NurseMiddleAgeAvgTime = Divide(NurseMiddleTimeSum, MiddleCount);
        NurseElderlyAvgTime = Divide(NurseElderlyTimeSum, ElderlyCount);

        // n patients have only n - 1 gaps between them (the first patient arrives at 0)
        MeanIat = Divide(IatSum, PatientCount - 1);
        LeftPercent = Divide(LeftCount, MiddleCount) * 100;
    }

    /// <summary>a / b, or 0 when b is 0.</summary>
    private static double Divide(double a, double b) => b > 0 ? a / b : 0;

    /// <summary>The doctor with this name, or null if the clinic has none.</summary>
    private Doctor? FindDoctor(string doctorName) => Doctors.Find(d => d.name == doctorName);

    #endregion

    /// <summary>Adds one row to the calendar table.</summary>
    private void AddEvent(int time, string eventName, Patient p, string details)
    {
        calendar.Add(new CalendarEntry(time, eventName, p.id, details));
    }

    /// <summary>Random number from 1 to 100.</summary>
    private int NextInt() => rng.NextInt(1, 100);
}
