using System.ComponentModel;
using System.Runtime.CompilerServices;

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
    
    public string Q1MeanTotalExamTime
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = Clinic.MeanExamTime.ToString("0.00");
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

    public string Q1MeanMiddleExamTime
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = Clinic.MeanMiddleExamTime.ToString("0.00");
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
    
    public string Q1MeanElderlyExamTime
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = Clinic.MeanElderlyExamTime.ToString("0.00");
            }
            return result;
        }
    }
    
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

    public string Q2MeanWaitAverage
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = Clinic.MeanWaitAverage.ToString("0.00");
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

    public string Q2MeanWaitAverageMiddleAge
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = Clinic.MeanWaitAverageMiddleAge.ToString("0.00");
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

    public string Q2MeanWaitAverageElderly
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                result = Clinic.MeanWaitAverageElderly.ToString("0.00");
            }
            return result;
        }
    }

    #endregion

    #region Q3

    public string Q3maxNurseQueue => Clinic?.maxNurseQueue.ToString() ?? "";
    public string Q3maxMiddleAgeQueue => Clinic?.maxMiddleAgeQueue.ToString() ?? "";
    public string Q3maxElderlyQueue => Clinic?.maxElderlyQueue.ToString() ?? "";

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
    
    public string ProbNurseWait => Clinic?.ProbNurseWait.ToString("0.00") ?? "";
    public string ProbMiddleDoctorWait => Clinic?.ProbMiddleDoctorWait.ToString("0.00") ?? "";
    public string ProbElderlyDoctorWait => Clinic?.ProbElderlyDoctorWait.ToString("0.00") ?? "";
    
    #endregion
    
    #region Q5
    
    public string Q5PortionNurseIdle => Clinic?.PortionNurseIdle.ToString("0.00") ?? "";
    public string Q5PortionMiddleDoctorIdle => Clinic?.PortionMiddleDoctorIdle.ToString("0.00") ?? "";
    public string Q5PortionElderlyDoctorIdle => Clinic?.PortionElderlyDoctorIdle.ToString("0.00") ?? "";
    
    #endregion

    #region Q6

    public string Q6NurseMiddleAgeAvgTime => Clinic?.NurseMiddleAgeAvgTime.ToString("0.00") ?? "";
    public string Q6NurseElderlyAvgTime => Clinic?.NurseElderlyAvgTime.ToString("0.00") ?? "";
    public string Q6NurseTheoreticalMiddleAgeAvgTime => 2.ToString();
    public string Q6NurseTheoreticalElderlyAvgTime => 4.ToString();
    public string Q6DoctorTheoreticalMiddleAvgTime => Clinic?.MiddleAgedExamTime.TheoreticalMean.ToString("0.00") ?? "";
    public string Q6DoctorTheoreticalElderlyAvgTime => Clinic?.ElderlyExamTime.TheoreticalMean.ToString("0.00") ?? "";

    #endregion

    #region Q7

    public string Q7MeanIat => Clinic?.MeanIat.ToString("0.00") ?? "";
    public string Q7TheoreticalIAT => Clinic?.IAT.TheoreticalMean.ToString("0.00") ?? "";

    public string Q7IATError
    {
        get
        {
            string result = "";
            if (Clinic != null)
            {
                double error = Math.Abs(Clinic.MeanIat - Clinic.IAT.TheoreticalMean) / Clinic.IAT.TheoreticalMean * 100;
                result = error.ToString("0.00");
            }
            return result;
        }
    }

    #endregion
}