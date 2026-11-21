using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Modeling_and_Simulation_Clinic.Entities;
using Wpf.Ui.Controls;

namespace Modeling_and_Simulation_Clinic.ViewModels;

/// <summary>
/// Shared state of the window: the distribution tables, the inputs of a run and the results
/// of the last run (simulation table and event calendar). Every page binds to <see cref="Instance"/>.
/// </summary>
public partial class SimulationViewModel : ObservableObject
{
    public const string ClinicName = "El Dorado";
    public const int MaxPatients = 5000;
    [ObservableProperty]
    private double? replications = 10;

    public const int MaxReplications = 500;

    /// <summary>The one instance used by the window and all pages.</summary>
    public static SimulationViewModel Instance { get; } = new();

    private SimulationViewModel()
    {
        PatientType = new DistributionTableViewModel(
            "Patient type", "Which kind of patient arrives", SymbolRegular.People20,
            value => (Patient.patientType)value == Patient.patientType.Elderly ? "Elderly" : "Middle-age",
            showsMean: false);
        InterArrivalTime = new DistributionTableViewModel(
            "Inter-arrival time", "Minutes between two arrivals", SymbolRegular.PersonClock20);
        MiddleAgeExamTime = new DistributionTableViewModel(
            "Middle-age exam time", "Minutes with the doctor", SymbolRegular.Stethoscope20);
        ElderlyExamTime = new DistributionTableViewModel(
            "Elderly exam time", "Minutes with the doctor", SymbolRegular.HeartPulse20);

        Distributions = [PatientType, InterArrivalTime, MiddleAgeExamTime, ElderlyExamTime];

        LoadDefaultDistributions();
        Compute();
    }

    // ---------------- Distributions ----------------

    public DistributionTableViewModel PatientType { get; }
    public DistributionTableViewModel InterArrivalTime { get; }
    public DistributionTableViewModel MiddleAgeExamTime { get; }
    public DistributionTableViewModel ElderlyExamTime { get; }

    /// <summary>The four tables in the order they are shown.</summary>
    public IReadOnlyList<DistributionTableViewModel> Distributions { get; }

    // ---------------- Inputs ----------------

    /// <summary>Number of patients to simulate (NumberBox values are doubles).</summary>
    [ObservableProperty]
    private double? patientCount = 20;

    /// <summary>Seed of the random number generator.</summary>
    [ObservableProperty]
    private double? seed = Clinic.DefaultSeed;

    // ---------------- Results of the last run ----------------

    /// <summary>One row per patient (shown on the Simulation table page).</summary>
    [ObservableProperty]
    private IReadOnlyList<PatientRow> patients = [];

    /// <summary>Every event in the order it happened (shown on the right of the window).</summary>
    [ObservableProperty]
    private IReadOnlyList<CalendarRow> calendar = [];

    /// <summary>One line describing the last run.</summary>
    [ObservableProperty]
    private string summary = "";

    /// <summary>Why the last compute failed, or null if it worked.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => ErrorMessage != null;

    /// <summary>Which table the Simulation table page shows: 0 combined, 1 nurse, 2 doctors.</summary>
    [ObservableProperty]
    private int selectedTableIndex;

    // ---------------- Commands ----------------

    /// <summary>
    /// Builds the distribution tables from the probabilities (cumulative column and RN ranges),
    /// then simulates the patients and refreshes the simulation table and the event calendar.
    /// </summary>
    [RelayCommand]
    private void Compute()
    {
        try
        {
            int count = ReadWholeNumber(PatientCount, "Patients", 1, MaxPatients);
            int seedValue = ReadWholeNumber(Seed, "Seed", 1, int.MaxValue - MaxReplications);
            int reps = ReadWholeNumber(Replications, "Replications", 1, MaxReplications); 
            // ... build the four distributions as before ...

            DiscreteDistribution type = PatientType.Build();
            DiscreteDistribution iat = InterArrivalTime.Build();
            DiscreteDistribution middleAge = MiddleAgeExamTime.Build();
            DiscreteDistribution elderly = ElderlyExamTime.Build();
            
            List<Clinic> runs = new();
            Console.Write($"Seed value ({reps}): ");
            for (int r = 0; r < reps; r++)
            {
                // every run gets its own seed, so every run is different but repeatable
                var clinic = new Clinic(ClinicName, (ulong)(seedValue + r));
                clinic.SetDistributionTables(type, iat, middleAge, elderly);
                clinic.InitializeDoctors();
                clinic.createPatients(count);
                clinic.RunSimulation();
                runs.Add(clinic);
                Console.Write($"{seedValue + r},");
            }
            Console.WriteLine();
            
            // the tables and the calendar still show one run (the first one)
            Clinic first = runs[0];
            Patients = first.patients.Select(PatientRow.From).ToList();
            Calendar = first.calendar
                .Select((entry, i) => CalendarRow.From(entry, i == 0 ? null : first.calendar[i - 1]))
                .ToList();
            Summary = $"{count} patients run {reps} times.";
            ErrorMessage = null;
            statisticsViewModel.Instance.ConnectRuns(runs);
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>Puts the original distribution tables back and computes again.</summary>
    [RelayCommand]
    private void ResetDistributions()
    {
        LoadDefaultDistributions();
        Compute();
    }

    // ---------------- Helpers ----------------

    /// <summary>Loads the tables from Clinic.BuildDistributionTable.</summary>
    private void LoadDefaultDistributions()
    {
        var defaults = new Clinic(ClinicName);
        defaults.BuildDistributionTable();

        PatientType.Load(defaults.Patientype);
        InterArrivalTime.Load(defaults.IAT);
        MiddleAgeExamTime.Load(defaults.MiddleAgedExamTime);
        ElderlyExamTime.Load(defaults.ElderlyExamTime);
    }

    /// <summary>Reads a NumberBox value that must be a whole number between min and max.</summary>
    private static int ReadWholeNumber(double? value, string name, int min, int max)
    {
        if (value is not { } number || number % 1 != 0 || number < min || number > max)
            throw new ArgumentException($"{name} must be a whole number from {min} to {max}.");
        return (int)number;
    }
}
