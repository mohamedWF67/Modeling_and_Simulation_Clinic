using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;

namespace Modeling_and_Simulation_Clinic.ViewModels;

public class statisticsViewModel : INotifyPropertyChanged
{
    public static statisticsViewModel Instance { get; } = new();
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private List<Clinic> runs = new();

    /// <summary>First run, used for the "sum / count" text that only makes sense for one run.</summary>
    public Clinic? Clinic => runs.Count > 0 ? runs[0] : null;

    /// <summary>Number of runs the averages are taken over.</summary>
    public int RunCount => runs.Count;

    /// <summary>Stores all runs and refreshes every value on the page.</summary>
    public void ConnectRuns(List<Clinic> newRuns)
    {
        runs = newRuns;
        RefreshStatistics();
    }

    /// <summary>Average of one statistic over all runs, rounded to 2 decimals.</summary>
    private double Avg(Func<Clinic, double> statistic) =>
        runs.Count == 0 ? 0 : Math.Round(runs.Average(statistic), 2);
    
    public statisticsViewModel() { }
    
    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    public void RefreshStatistics()
    {
        OnPropertyChanged(string.Empty);
        
        UpdateQ1ExamTimes();
        UpdateQ2MeanWaitTimes();
        UpdateQ3MaxQueue();
        UpdateQ6();
        UpdateQ7();
    }

    #region Q1
    public string Q1TotalExamTime
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = $"{Clinic.TotalExamTime.ToString()} / {Clinic.totalCount}";
            }
            return result;
        }
    }
    public string Q1TotalMiddleExamTime
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = $"{Clinic.TotalMiddleExamTime.ToString()} / {Clinic.totalMiddleCount}";
            }
            return result;
        }
    }


    public string Q1TotalElderlyExamTime
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = $"{Clinic.TotalElderlyExamTime.ToString()} / {Clinic.totalElderlyCount}";
            }
            return result;
        }
    }
    public double Q1MeanTotalExamTime => Avg(c => c.MeanExamTime);
    public double Q1MeanMiddleExamTime => Avg(c => c.MeanMiddleExamTime);
    public double Q1MeanElderlyExamTime => Avg(c => c.MeanElderlyExamTime);
    
    public ObservableCollection<double> Q1ExamTimes { get; } = new();

    public void UpdateQ1ExamTimes()
    {
        Q1ExamTimes.Clear();
        
        Q1ExamTimes.Add(Q1MeanElderlyExamTime);
        Q1ExamTimes.Add(Q1MeanMiddleExamTime);
        Q1ExamTimes.Add(Q1MeanTotalExamTime);
    }
    
    public string[] Q1ExamTimeLabels { get; } =
    [        
        "Elderly",
        "Middle-age",
        "Total",
    ];
    
    #endregion

    #region Q2

    public string Q2TotalWaitAverage
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = $"{Clinic.TotalWaitAverage.ToString()} / {Clinic.Totalwaitcount}";
            }
            return result;
        }
    }
    
    public string Q2TotalWaitAverageMiddleAge
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = $"{Clinic.TotalWaitAverageMiddleAge.ToString()} / {Clinic.totalMiddleWaitcount}";
            }
            return result;
        }
    }
    
    public string Q2TotalWaitAverageElderly
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = $"{Clinic.TotalWaitAverageElderly.ToString()} / {Clinic.totalElderlyWaitcount}";
            }
            return result;
        }
    }
    
    public double Q2MeanWaitAverage => Avg(c => c.MeanWaitAverage);
    public double Q2MeanWaitAverageMiddleAge => Avg(c => c.MeanWaitAverageMiddleAge);
    public double Q2MeanWaitAverageElderly => Avg(c => c.MeanWaitAverageElderly);
    
    public ObservableCollection<double> Q2MeanWaitTimes { get; } = new();

    public void UpdateQ2MeanWaitTimes()
    {
        Q2MeanWaitTimes.Clear();
        
        Q2MeanWaitTimes.Add(Q2MeanWaitAverageElderly);
        Q2MeanWaitTimes.Add(Q2MeanWaitAverageMiddleAge);
        Q2MeanWaitTimes.Add(Q2MeanWaitAverage);
    }

    #endregion

    #region Q3

    public double Q3maxNurseQueue => Avg(c => c.maxNurseQueue);
    public double Q3maxMiddleAgeQueue => Avg(c => c.maxMiddleAgeQueue);
    public double Q3maxElderlyQueue => Avg(c => c.maxElderlyQueue);
    
    public ObservableCollection<double> Q3MaxQueue { get; } = new();

    public void UpdateQ3MaxQueue()
    {
        Q3MaxQueue.Clear();
        
        Q3MaxQueue.Add(Q3maxElderlyQueue);
        Q3MaxQueue.Add(Q3maxMiddleAgeQueue);
        Q3MaxQueue.Add(Q3maxNurseQueue);
    }

    #endregion
    
    #region Q4

    public string Q4NurseWaitCount
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = $"{Clinic.nurseWaitCount.ToString()} / {Clinic.Totalwaitcount}";
            }
            return result;
        }
    }

    public string Q4middleDoctorWaitCount
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = $"{Clinic.middleDoctorWaitCount.ToString()} / {Clinic.totalMiddleWaitcount}";
            }
            return result;
        }
    }

    public string Q4ElderlyDoctorWaitCount
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = $"{Clinic.elderlyDoctorWaitCount.ToString()} / {Clinic.totalElderlyWaitcount}";
            }
            return result;
        }
    }
    
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
    public double Q6DoctorTheoreticalMiddleAvgTime => Math.Round(Clinic?.MiddleAgedExamTime.TheoreticalMean ?? 0,2);
    public double Q6DoctorTheoreticalElderlyAvgTime => Math.Round(Clinic?.ElderlyExamTime.TheoreticalMean ?? 0,2);
    
    public ObservableCollection<double> Q6TheoreticalTimes { get; } = new();
    public ObservableCollection<double> Q6ActualTimes { get; } = new();

    public string[] Q6Labels { get; } =
    [
        "Nurse Middle-age",
        "Nurse Elderly",
        "Doctor Middle-age",
        "Doctor Elderly"
    ];

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
    public double Q7TheoreticalIAT => Math.Round(Clinic?.IAT.TheoreticalMean ?? 0,2);
    
    public double Q7IATErrorDouble =>  Math.Abs(Q7MeanIat - Q7TheoreticalIAT) / Q7TheoreticalIAT * 100;

    public string Q7IATError => Q7IATErrorDouble.ToString("0.00") + " %";
    
    public ObservableCollection<double> Q7IATTimes { get; } = new();
    
    public string[] Q7Labels { get; } =
    [
        "Theoretical",
        "Actual"
    ];
    
    public void UpdateQ7()
    {
        Q7IATTimes.Clear();
        
        Q7IATTimes.Add(Q7MeanIat);
        Q7IATTimes.Add(Q7TheoreticalIAT);
    }

    #endregion
    
    public Func<ChartPoint, string> GaugeFormatter { get; } =
        point => point.Coordinate.PrimaryValue.ToString("0.00");
    
    public Func<ChartPoint, string> GaugeFormatterPercentages { get; } =
        point => (point.Coordinate.PrimaryValue * 100).ToString("0") + "%";
}