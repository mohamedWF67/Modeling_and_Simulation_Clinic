namespace Modeling_and_Simulation_Clinic.Entities;
/// <summary>
///  Discrete Distribution.
///  Values are sampled from a discrete distribution.
/// </summary>
public class DiscreteDistribution
{
    //region Fields
    private readonly List<int> values, rnLow, rnHigh;
    private readonly List<double> probs, cumulative;
    private double _mean;
    
    //region Constructor
    /// <summary>
    /// Creates a new Discrete Distribution.
    /// </summary>
    /// <param name="values">Values to be sampled.</param>
    /// <param name="probs">Probabilities of each value.</param>
    /// <exception cref="ArgumentException">Throw if input is invalid.</exception>
    public DiscreteDistribution(int[] values, double[] probs)
    {
        if (values.Length != probs.Length) throw new ArgumentException("Length mismatch");
        if (Math.Abs(probs.Sum() - 1.0) > 1e-9) throw new ArgumentException("Probabilities do not sum to 1");
        if (probs.Any(p => p < 0)) throw new ArgumentException("Probabilities cannot be negative");
        
        this.values = values.ToList();
        this.probs = probs.ToList();
        cumulative = new ();
        rnLow = new ();
        rnHigh = new ();
        Build();
    }
    /// <summary> Computes the cumulative values and the ranges of their ranges.</summary>
    private void Build()
    {
        int prevEnd = 0;
        for (int i = 0; i < values.Count; i++)
        {
            double prev = i == 0 ? 0 : cumulative[i - 1];
            cumulative.Add(prev + probs[i]);
            
            int start = prevEnd + 1;
            int end = (int)Math.Round(cumulative[i] * 100);
            
            rnLow.Add(start);
            rnHigh.Add(end);
            
            prevEnd = end;
            _mean += values[i] * probs[i];
        }
    }

    #region Public Methods
    /// <summary>
    /// Maps the random number (1-100) to a value using ranges
    /// </summary>
    /// <param name="rn">Random number in [1, 100]</param>
    /// <returns>The sampled Value</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throw if rn is not from 1 to 100.</exception>
    public int Sample(int rn)
    {
        for (int i = 0; i < values.Count; i++)
            if (rn >= rnLow[i] && rn <= rnHigh[i])
                return values[i];
        throw new ArgumentOutOfRangeException(nameof(rn), "Random number out of range");
    }
    /// <summary>
    /// Maps the random number (1-100) to a value using ranges
    /// </summary>
    /// <param name="rn">Random number in [1, 100]</param>
    /// <returns>The sampled Value's index</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throw if rn is not from 1 to 100.</exception>
    public int SampleIndex(int rn)
    {
        for (int i = 0; i < values.Count; i++)
            if (rn >= rnLow[i] && rn <= rnHigh[i])
                return i;
        throw new ArgumentOutOfRangeException(nameof(rn), "Random number out of range");
    }
    /// <summary>
    /// Prints the distribution table to the console
    /// </summary>
    public void Print()
    {
        Console.WriteLine($"Values\tProbability\tCumulative\tRange");
        for (int i = 0; i < values.Count; i++)
        {
            string s = $"{values[i]}\t{probs[i].ToString("0.00")}\t\t{cumulative[i].ToString("0.00")}";
            s += $"\t\t{rnLow[i]:00}-{rnHigh[i]:00}";
            Console.WriteLine(s);
        }
        Console.WriteLine($"Mean: {_mean}");
    }
    #endregion

    #region Public Properties
    public IReadOnlyList<int> Values => values;
    public IReadOnlyList<double> Probabilities => probs;
    public IReadOnlyList<double> Cumulative => cumulative;
    public IReadOnlyList<int> RnLow => rnLow;
    public IReadOnlyList<int> RnHigh => rnHigh;
    /// <summary>Value x Probability.</summary>
    public double TheoreticalMean => _mean;
    #endregion
}