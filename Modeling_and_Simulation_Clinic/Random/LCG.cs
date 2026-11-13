using System;

namespace Modeling_and_Simulation_Clinic.Random;

/// <summary>
/// Linear Congruential Generator.
/// </summary>
public class LCG
{
   /// <summary>Seed used to control the randomness</summary>
   private ulong seed;
   private ulong a = 16807;
   private ulong c = 0;
   private ulong m = 2147483647;

   /// <summary>
   /// Creates a new LCG instance.
   /// </summary>
   /// <param name="state">control the seed of this instance.</param>
   public LCG(ulong state) => seed = state;
   
   /// <summary>
   /// Creates a new LCG instance with a random seed.
   /// </summary>
   public LCG() => seed = (ulong)DateTime.Now.Ticks;
   
   /// <summary>
   /// Generates a random integer number.
   /// </summary>
   /// <param name="min">Minimum Value for the generated number.</param>
   /// <param name="max">Maximum Value for the generated number.</param>
   /// <returns>Random integer number.</returns>
   public int NextInt(int min = 0, int max = 2147483647)
   {
      if (min > max) 
         throw new ArgumentException("min must be less than or equal to max");
      
      double r = NextDouble();
      double n = r * (max - min + 1) + min;
      return (int)n;
   }

   /// <summary>
   /// Generates a random floating-point(double) number.
   /// </summary>
   /// <returns>Random floating-point(double) number.</returns>
   public double NextDouble()
   {
      seed = (a * seed + c) % m;
      double r = (double)seed / m;
      return r;
   }
   /// <summary>
   /// Generates a random Boolean value.
   /// </summary>
   /// <returns>Random Boolean value.</returns>
   public bool NextBool() => NextDouble() < 0.5;
}