using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;

namespace Modeling_and_Simulation_Clinic.ViewModels;

public class statisticsViewModel
{
    public static statisticsViewModel Instance { get; } = new();
    public event PropertyChangedEventHandler? PropertyChanged;

    private Clinic? _clinic;

    public Clinic? Clinic
    {
        get => _clinic;
        private set
        {
            _clinic = value;
            RefreshStatistics();
        }
    }
    
    public statisticsViewModel() { }
    public void ConnectClinic(Clinic c)
    {
        Clinic = c;
    }
    
    public void Reset()
    {
        Clinic = null;
    }
    
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
    public double Q1MeanTotalExamTime => Math.Round(Clinic?.MeanExamTime ?? 0,2);
    public double Q1MeanMiddleExamTime => Math.Round(Clinic?.MeanMiddleExamTime ?? 0,2);

    public double Q1MeanElderlyExamTime => Math.Round(Clinic?.MeanElderlyExamTime ?? 0,2);
    
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
    
    public double Q2MeanWaitAverage => Math.Round(Clinic?.MeanWaitAverage ?? 0,2);
    public double Q2MeanWaitAverageMiddleAge => Math.Round(Clinic?.MeanWaitAverageMiddleAge ?? 0,2);
    public double Q2MeanWaitAverageElderly => Math.Round(Clinic?.MeanWaitAverageElderly ?? 0,2);
    
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

    public double Q3maxNurseQueue => Clinic?.maxNurseQueue ?? 0;
    public double Q3maxMiddleAgeQueue => Clinic?.maxMiddleAgeQueue ?? 0;
    public double Q3maxElderlyQueue => Clinic?.maxElderlyQueue ?? 0;
    
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
    
    public double Q4ProbNurseWait => Math.Round(Clinic?.ProbNurseWait ?? 0,2);
    public string Q4ProbMiddleDoctorWait => Math.Round(Clinic?.ProbMiddleDoctorWait ?? 0,2).ToString("0.00");
    public string Q4ProbElderlyDoctorWait => Math.Round(Clinic?.ProbElderlyDoctorWait ?? 0,2).ToString("0.00");
    
    #endregion
    
    #region Q5
    
    public double Q5PortionNurseIdleDouble => Math.Round(Clinic?.PortionNurseIdle ?? 0,2);
    public double Q5PortionMiddleDoctorIdleDouble => Math.Round(Clinic?.PortionMiddleDoctorIdle ?? 0,2);
    public double Q5PortionElderlyDoctorIdleDouble => Math.Round(Clinic?.PortionElderlyDoctorIdle ?? 0,2);
    
    #endregion

    #region Q6

    public double Q6NurseMiddleAgeAvgTime => Math.Round(Clinic?.NurseMiddleAgeAvgTime ?? 0,2);
    public double Q6NurseElderlyAvgTime => Math.Round(Clinic?.NurseElderlyAvgTime ?? 0,2);
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

    public double Q7MeanIat => Math.Round(Clinic?.MeanIat ?? 0,2);
    public double Q7TheoreticalIAT => Math.Round(Clinic?.IAT.TheoreticalMean ?? 0,2);

    public string Q7IATError
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                double error = Math.Abs(Clinic.MeanIat - Clinic.IAT.TheoreticalMean) / Clinic.IAT.TheoreticalMean * 100;
                result = error.ToString("0.00") + " %";
            }
            return result;
        }
    }
    
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