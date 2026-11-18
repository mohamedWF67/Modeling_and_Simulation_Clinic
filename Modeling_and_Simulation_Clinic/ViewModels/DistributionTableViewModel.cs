using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Modeling_and_Simulation_Clinic.Entities;
using Wpf.Ui.Controls;

namespace Modeling_and_Simulation_Clinic.ViewModels;

/// <summary>
/// One distribution table on the Distribution page: value, probability, cumulative probability
/// and random-number range. The probabilities can be edited; the cumulative column, the ranges
/// and the mean are rebuilt when the user presses Compute.
/// </summary>
public partial class DistributionTableViewModel : ObservableObject
{
    private readonly Func<int, string> _formatValue;

    /// <param name="title">Name of the distribution.</param>
    /// <param name="description">One line saying what the value means.</param>
    /// <param name="icon">Icon shown next to the title.</param>
    /// <param name="formatValue">How a value is shown in the table (default: the number).</param>
    /// <param name="showsMean">False when the mean has no meaning (patient type).</param>
    public DistributionTableViewModel(string title, string description, SymbolRegular icon,
        Func<int, string>? formatValue = null, bool showsMean = true)
    {
        Title = title;
        Description = description;
        Icon = icon;
        ShowsMean = showsMean;
        _formatValue = formatValue ?? (value => value.ToString());
    }

    public string Title { get; }
    public string Description { get; }
    public SymbolRegular Icon { get; }
    public bool ShowsMean { get; }

    public ObservableCollection<DistributionRowViewModel> Rows { get; } = [];

    /// <summary>Theoretical mean (sum of value x probability) of the last computed table.</summary>
    [ObservableProperty]
    private double mean;

    /// <summary>Sum of the probabilities as they are typed in now.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSumValid))]
    private double probabilitySum;

    /// <summary>True when a probability was edited after the last compute.</summary>
    [ObservableProperty]
    private bool hasPendingChanges;

    /// <summary>True when the probabilities add up to 1.</summary>
    public bool IsSumValid => Math.Abs(ProbabilitySum - 1.0) <= 1e-9;

    /// <summary>Fills the table from a built distribution (cumulative column and RN ranges included).</summary>
    public void Load(DiscreteDistribution distribution)
    {
        // Same values as before (the usual case after Compute): update the rows in place,
        // which is much cheaper for the DataGrid than removing and re-adding them.
        bool sameValues = Rows.Select(r => r.Value).SequenceEqual(distribution.Values);
        if (!sameValues)
        {
            foreach (DistributionRowViewModel row in Rows)
                row.PropertyChanged -= OnRowChanged;
            Rows.Clear();

            foreach (int value in distribution.Values)
            {
                var row = new DistributionRowViewModel(value, _formatValue(value), 0);
                row.PropertyChanged += OnRowChanged;
                Rows.Add(row);
            }
        }

        for (int i = 0; i < Rows.Count; i++)
        {
            int low = distribution.RnLow[i];
            int high = distribution.RnHigh[i];
            Rows[i].Probability = distribution.Probabilities[i];
            Rows[i].Cumulative = distribution.Cumulative[i];
            // A probability of 0 gets no random numbers at all
            Rows[i].Range = low <= high ? $"{low:00} - {high:00}" : "—";
        }

        Mean = distribution.TheoreticalMean;
        ProbabilitySum = Rows.Sum(r => r.Probability);
        HasPendingChanges = false;
    }

    /// <summary>Builds a distribution from the values and the (edited) probabilities.</summary>
    /// <exception cref="ArgumentException">The probabilities do not form a valid distribution.</exception>
    public DiscreteDistribution Build()
    {
        if (Rows.Any(r => r.Probability is < 0 or > 1))
            throw new ArgumentException($"{Title}: every probability must be between 0 and 1.");
        if (!IsSumValid)
            throw new ArgumentException(
                $"{Title}: the probabilities add up to {ProbabilitySum:0.###}, but they must add up to 1.");

        return new DiscreteDistribution(
            Rows.Select(r => r.Value).ToArray(),
            Rows.Select(r => r.Probability).ToArray());
    }

    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DistributionRowViewModel.Probability))
            return;

        ProbabilitySum = Rows.Sum(r => r.Probability);
        HasPendingChanges = true;
    }
}

/// <summary>One row of a distribution table.</summary>
public partial class DistributionRowViewModel : ObservableObject
{
    public DistributionRowViewModel(int value, string label, double probability)
    {
        Value = value;
        Label = label;
        Probability = probability;
    }

    public int Value { get; }

    /// <summary>The value as shown in the table ("Elderly" for the patient type, minutes otherwise).</summary>
    public string Label { get; }

    [ObservableProperty]
    private double probability;

    [ObservableProperty]
    private double cumulative;

    [ObservableProperty]
    private string range = "";
}
