using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore.Kernel;
using Modeling_and_Simulation_Clinic.Entities;

namespace Modeling_and_Simulation_Clinic.ViewModels;

/// <summary>
/// Everything the Results page shows. The values are averages over all runs of the last
/// Compute; the "sum / count" texts come from the first run. Q8 compares the runs with
/// 2 doctors against the same runs (same seeds) with a third doctor.
/// </summary>
public class StatisticsViewModel : INotifyPropertyChanged
{
    /// <summary>The one instance the Results page binds to.</summary>
    public static StatisticsViewModel Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private List<Clinic> runs = new();                  // clinic with 2 doctors
    private List<Clinic> runsWithThirdDoctor = new();   // same seeds, 3 doctors (Q8)

    /// <summary>First run, used for the "sum / count" text that only makes sense for one run.</summary>
    public Clinic? Clinic => runs.Count > 0 ? runs[0] : null;

    /// <summary>Number of runs the averages are taken over.</summary>
    public int RunCount => runs.Count;

    /// <summary>Stores all runs and refreshes every value and chart on the page.</summary>
    /// <param name="newRuns">Runs of the clinic as described (2 doctors).</param>
    /// <param name="newRunsWithThirdDoctor">The same runs with a third doctor (Q8).</param>
    public void ConnectRuns(List<Clinic> newRuns, List<Clinic> newRunsWithThirdDoctor)
    {
        runs = newRuns;
        runsWithThirdDoctor = newRunsWithThirdDoctor;
        RefreshStatistics();
    }

    /// <summary>Rebuilds the chart values, then tells the page that every property changed.</summary>
    public void RefreshStatistics()
    {
        UpdateQ1ExamTimes();
        UpdateQ2MeanWaitTimes();
        UpdateQ2WaitHistogram();
        UpdateQ3MaxQueue();
        UpdateQ6();
        UpdateQ7();
        UpdateQ7Frequency();
        UpdateQ8();

        // An empty name means "all properties changed", so every TextBlock reads its value again
        OnPropertyChanged(string.Empty);
    }

    /// <summary>Raises PropertyChanged so WPF updates the bindings.</summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>Average of one statistic over a list of runs, rounded to 2 decimals.</summary>
    private static double Average(List<Clinic> list, Func<Clinic, double> statistic) =>
        list.Count == 0 ? 0 : Math.Round(list.Average(statistic), 2);

    /// <summary>Average of one statistic over the runs with 2 doctors.</summary>
    private double Avg(Func<Clinic, double> statistic) => Average(runs, statistic);

    /// <summary>"sum / count" of the first run, or an empty text before the first Compute.</summary>
    private string SumOverCount(Func<Clinic, int> sum, Func<Clinic, int> count) =>
        Clinic == null ? "" : $"{sum(Clinic)} / {count(Clinic)}";

    #region Q1

    public string Q1TotalExamTime => SumOverCount(c => c.ExamTimeSum, c => c.ExamCount);
    public string Q1TotalMiddleExamTime => SumOverCount(c => c.MiddleExamTimeSum, c => c.MiddleExamCount);
    public string Q1TotalElderlyExamTime => SumOverCount(c => c.ElderlyExamTimeSum, c => c.ElderlyExamCount);

    public double Q1MeanTotalExamTime => Avg(c => c.MeanExamTime);
    public double Q1MeanMiddleExamTime => Avg(c => c.MeanMiddleExamTime);
    public double Q1MeanElderlyExamTime => Avg(c => c.MeanElderlyExamTime);

    public ObservableCollection<double> Q1ExamTimes { get; } = new();

    public string[] Q1ExamTimeLabels { get; } = ["Elderly", "Middle-age", "Total"];

    /// <summary>Chart values, in the same order as Q1ExamTimeLabels.</summary>
    public void UpdateQ1ExamTimes()
    {
        Q1ExamTimes.Clear();
        Q1ExamTimes.Add(Q1MeanElderlyExamTime);
        Q1ExamTimes.Add(Q1MeanMiddleExamTime);
        Q1ExamTimes.Add(Q1MeanTotalExamTime);
    }

    #endregion

    #region Q2

    public string Q2TotalWaitAverage => SumOverCount(c => c.WaitSum, c => c.PatientCount);
    public string Q2TotalWaitAverageMiddleAge => SumOverCount(c => c.MiddleWaitSum, c => c.MiddleCount);
    public string Q2TotalWaitAverageElderly => SumOverCount(c => c.ElderlyWaitSum, c => c.ElderlyCount);

    public double Q2MeanWaitAverage => Avg(c => c.MeanWait);
    public double Q2MeanWaitAverageMiddleAge => Avg(c => c.MeanWaitMiddleAge);
    public double Q2MeanWaitAverageElderly => Avg(c => c.MeanWaitElderly);

    /// <summary>Patients who left without seeing a doctor, averaged per run.</summary>
    public string Q2LeftText =>
        $"{Avg(c => c.LeftCount):0.##} patients per run left without seeing a doctor " +
        $"({Avg(c => c.LeftPercent):0.##}% of middle-age patients).";

    public ObservableCollection<double> Q2MeanWaitTimes { get; } = new();

    /// <summary>Chart values, in the same order as Q1ExamTimeLabels.</summary>
    public void UpdateQ2MeanWaitTimes()
    {
        Q2MeanWaitTimes.Clear();
        Q2MeanWaitTimes.Add(Q2MeanWaitAverageElderly);
        Q2MeanWaitTimes.Add(Q2MeanWaitAverageMiddleAge);
        Q2MeanWaitTimes.Add(Q2MeanWaitAverage);
    }

    // Histogram of the total waiting time of every patient in every run
    private static readonly int[] WaitBinUpperLimits = [0, 2, 5, 10, 15, 20, int.MaxValue];

    public string[] Q2WaitHistogramLabels { get; } = ["0", "1-2", "3-5", "6-10", "11-15", "16-20", "> 20"];

    /// <summary>% of all patients (all runs together) whose wait falls in each bin.</summary>
    public ObservableCollection<double> Q2WaitHistogram { get; } = new();

    /// <summary>Counts how many patients fall in each waiting-time bin.</summary>
    public void UpdateQ2WaitHistogram()
    {
        int[] counts = new int[WaitBinUpperLimits.Length];
        int total = 0;

        foreach (Clinic run in runs)
        {
            foreach (Patient p in run.patients)
            {
                int bin = 0;
                while (p.totalWait > WaitBinUpperLimits[bin])
                    bin++;
                counts[bin]++;
                total++;
            }
        }

        Q2WaitHistogram.Clear();
        foreach (int count in counts)
            Q2WaitHistogram.Add(total == 0 ? 0 : Math.Round(100.0 * count / total, 1));
    }

    #endregion

    #region Q3

    public double Q3MaxNurseQueue => Avg(c => c.maxNurseQueue);
    public double Q3MaxMiddleAgeQueue => Avg(c => c.maxMiddleAgeQueue);
    public double Q3MaxElderlyQueue => Avg(c => c.maxElderlyQueue);

    public ObservableCollection<double> Q3MaxQueue { get; } = new();

    /// <summary>Chart values: elderly (D2), middle-age (D1), nurse.</summary>
    public void UpdateQ3MaxQueue()
    {
        Q3MaxQueue.Clear();
        Q3MaxQueue.Add(Q3MaxElderlyQueue);
        Q3MaxQueue.Add(Q3MaxMiddleAgeQueue);
        Q3MaxQueue.Add(Q3MaxNurseQueue);
    }

    #endregion

    #region Q4

    public string Q4NurseWaitCount => SumOverCount(c => c.NurseWaitCount, c => c.PatientCount);
    public string Q4MiddleDoctorWaitCount => SumOverCount(c => c.MiddleDoctorWaitCount, c => c.MiddleCount);
    public string Q4ElderlyDoctorWaitCount => SumOverCount(c => c.ElderlyDoctorWaitCount, c => c.ElderlyCount);

    public double Q4ProbNurseWait => Avg(c => c.ProbNurseWait);
    public string Q4ProbMiddleDoctorWait => Avg(c => c.ProbMiddleDoctorWait).ToString("0.00");
    public string Q4ProbElderlyDoctorWait => Avg(c => c.ProbElderlyDoctorWait).ToString("0.00");

    #endregion

    #region Q5

    public double Q5PortionNurseIdleDouble => Avg(c => c.PortionNurseIdle);
    public double Q5PortionMiddleDoctorIdleDouble => Avg(c => c.PortionMiddleDoctorIdle);
    public double Q5PortionElderlyDoctorIdleDouble => Avg(c => c.PortionElderlyDoctorIdle);

    #endregion

    #region Q6

    public double Q6NurseMiddleAgeAvgTime => Avg(c => c.NurseMiddleAgeAvgTime);
    public double Q6NurseElderlyAvgTime => Avg(c => c.NurseElderlyAvgTime);
    public double Q6NurseTheoreticalMiddleAgeAvgTime => 2;
    public double Q6NurseTheoreticalElderlyAvgTime => 4;
    public double Q6DoctorTheoreticalMiddleAvgTime => Math.Round(Clinic?.MiddleAgedExamTime.TheoreticalMean ?? 0, 2);
    public double Q6DoctorTheoreticalElderlyAvgTime => Math.Round(Clinic?.ElderlyExamTime.TheoreticalMean ?? 0, 2);

    public ObservableCollection<double> Q6TheoreticalTimes { get; } = new();
    public ObservableCollection<double> Q6ActualTimes { get; } = new();

    public string[] Q6Labels { get; } = ["Nurse Middle-age", "Nurse Elderly", "Doctor Middle-age", "Doctor Elderly"];

    /// <summary>Chart values, in the same order as Q6Labels.</summary>
    public void UpdateQ6()
    {
        Q6TheoreticalTimes.Clear();
        Q6ActualTimes.Clear();

        Q6TheoreticalTimes.Add(Q6NurseTheoreticalMiddleAgeAvgTime);
        Q6TheoreticalTimes.Add(Q6NurseTheoreticalElderlyAvgTime);
        Q6TheoreticalTimes.Add(Q6DoctorTheoreticalMiddleAvgTime);
        Q6TheoreticalTimes.Add(Q6DoctorTheoreticalElderlyAvgTime);

        Q6ActualTimes.Add(Q6NurseMiddleAgeAvgTime);
        Q6ActualTimes.Add(Q6NurseElderlyAvgTime);
        Q6ActualTimes.Add(Q1MeanMiddleExamTime);
        Q6ActualTimes.Add(Q1MeanElderlyExamTime);
    }

    #endregion

    #region Q7

    public double Q7MeanIat => Avg(c => c.MeanIat);
    public double Q7TheoreticalIAT => Math.Round(Clinic?.IAT.TheoreticalMean ?? 0, 2);

    /// <summary>|actual - theoretical| / theoretical, in %.</summary>
    public double Q7IATErrorDouble =>
        Q7TheoreticalIAT > 0 ? Math.Abs(Q7MeanIat - Q7TheoreticalIAT) / Q7TheoreticalIAT * 100 : 0;

    public string Q7IATError => Q7IATErrorDouble.ToString("0.00") + " %";

    public ObservableCollection<double> Q7IATTimes { get; } = new();

    public string[] Q7Labels { get; } = ["Theoretical", "Actual"];

    /// <summary>Chart values, in the same order as Q7Labels.</summary>
    public void UpdateQ7()
    {
        Q7IATTimes.Clear();
        Q7IATTimes.Add(Q7TheoreticalIAT);
        Q7IATTimes.Add(Q7MeanIat);
    }

    // How often each inter-arrival time was generated, next to its probability in the table
    public ObservableCollection<double> Q7ExpectedFrequency { get; } = new();
    public ObservableCollection<double> Q7ObservedFrequency { get; } = new();
    public string[] Q7FrequencyLabels { get; private set; } = [];

    /// <summary>
    /// Relative frequency of each inter-arrival value over all runs (the first patient of a
    /// run has no gap, so it is skipped), next to the probability from the table.
    /// </summary>
    public void UpdateQ7Frequency()
    {
        Q7ExpectedFrequency.Clear();
        Q7ObservedFrequency.Clear();
        Q7FrequencyLabels = [];
        if (Clinic == null)
            return;

        DiscreteDistribution table = Clinic.IAT;
        List<int> gaps = runs.SelectMany(c => c.patients.Skip(1)).Select(p => p.iat).ToList();

        for (int i = 0; i < table.Values.Count; i++)
        {
            int value = table.Values[i];
            int times = gaps.Count(g => g == value);
            Q7ExpectedFrequency.Add(Math.Round(table.Probabilities[i], 2));
            Q7ObservedFrequency.Add(gaps.Count == 0 ? 0 : Math.Round((double)times / gaps.Count, 2));
        }
        Q7FrequencyLabels = table.Values.Select(v => v + " min").ToArray();
    }

    #endregion

    #region Q8

    public double Q8WaitMiddleTwo => Q2MeanWaitAverageMiddleAge;
    public double Q8WaitElderlyTwo => Q2MeanWaitAverageElderly;
    public double Q8WaitAllTwo => Q2MeanWaitAverage;

    public double Q8WaitMiddleThree => Average(runsWithThirdDoctor, c => c.MeanWaitMiddleAge);
    public double Q8WaitElderlyThree => Average(runsWithThirdDoctor, c => c.MeanWaitElderly);
    public double Q8WaitAllThree => Average(runsWithThirdDoctor, c => c.MeanWait);

    /// <summary>Change in the average wait when the third doctor is added (negative = shorter).</summary>
    public string Q8WaitMiddleChange => Change(Q8WaitMiddleTwo, Q8WaitMiddleThree);
    public string Q8WaitElderlyChange => Change(Q8WaitElderlyTwo, Q8WaitElderlyThree);
    public string Q8WaitAllChange => Change(Q8WaitAllTwo, Q8WaitAllThree);

    /// <summary>Idle portion of the third doctor, to show what the extra room costs.</summary>
    public double Q8ExtraDoctorIdle => Average(runsWithThirdDoctor, c => c.PortionExtraDoctorIdle);

    public ObservableCollection<double> Q8TwoDoctors { get; } = new();
    public ObservableCollection<double> Q8ThreeDoctors { get; } = new();

    public string[] Q8Labels { get; } = ["Elderly", "Middle-age", "All"];

    /// <summary>"three - two" with a sign, for example "-0.71 min".</summary>
    private static string Change(double two, double three) => (three - two).ToString("+0.00;-0.00;0.00") + " min";

    /// <summary>Chart values, in the same order as Q8Labels.</summary>
    public void UpdateQ8()
    {
        Q8TwoDoctors.Clear();
        Q8ThreeDoctors.Clear();

        Q8TwoDoctors.Add(Q8WaitElderlyTwo);
        Q8TwoDoctors.Add(Q8WaitMiddleTwo);
        Q8TwoDoctors.Add(Q8WaitAllTwo);

        Q8ThreeDoctors.Add(Q8WaitElderlyThree);
        Q8ThreeDoctors.Add(Q8WaitMiddleThree);
        Q8ThreeDoctors.Add(Q8WaitAllThree);
    }

    #endregion

    public Func<ChartPoint, string> GaugeFormatter { get; } =
        point => point.Coordinate.PrimaryValue.ToString("0.00");

    public Func<ChartPoint, string> GaugeFormatterPercentages { get; } =
        point => (point.Coordinate.PrimaryValue * 100).ToString("0") + "%";
}
