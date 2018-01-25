using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace IncrementalMeanVarianceAccumulator
{
    /// <summary>
    /// Accumulates the count, mean and variance of a sequence of weighted values. Values may be optionally weighted, in which case this struct accumulates the 
    /// sum-of-weights, the weighted-mean, and the weighted-variance.
    /// 
    /// For greater efficiency, most methods are aggressively inlined, and no heap allocation is used.
    /// 
    /// Note that the struct is IMMUTABLE, so:
    /// 
    /// MeanVarianceAccumulator.Init(3.0).Add(2.0).Add(1.0).Mean == 2.0
    /// 
    /// but
    /// 
    /// var x = MeanVarianceAccumulator.Empty;
    /// x.Add(1.0);
    /// x.Mean == 0.0
    /// 
    /// You can accumulate accumulators, which allows parallelization. e.g.
    /// MeanVarianceAccumulator.Init(1.0).Add(3.0).Add(
    ///         MeanVarianceAccumulator.Init(5.0).Add(7.0)
    ///     ).Mean == 4.0
    ///     
    /// The algorithm is based on https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Weighted_incremental_algorithm
    /// </summary>
    public struct MeanVarianceAccumulator
    {
        readonly double weightSum, sX, meanX;
        MeanVarianceAccumulator(double _weightSum, double _sX, double _meanX) { meanX = _meanX; sX = _sX; weightSum = _weightSum; }

        /// <summary>
        /// Combines the current set of accumulated values with a new value (optionally weighted), and returns the result.  This method does not alter the current accumulation; use the return value.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MeanVarianceAccumulator Add(double val, double weight = 1.0)
        {
            if (weight == 0.0)
                return this;//ignore zero-weight stuff...
            double newWeightSum = weightSum + weight;
            double mScale = weight / newWeightSum;
            double sScale = weightSum * weight / newWeightSum;
            return new MeanVarianceAccumulator(newWeightSum, sX + (val - meanX) * (val - meanX) * sScale, meanX + (val - meanX) * mScale);
        }

        /// <summary>
        /// Combines the current set of accumulated values with another accumulation, and returns the result.  This method does not alter the current accumulation; use the return value.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MeanVarianceAccumulator Add(MeanVarianceAccumulator other)
        {
            double newWeightSum = weightSum + other.weightSum;
            double mScale = other.weightSum / newWeightSum;
            double sScale = weightSum * other.weightSum / newWeightSum;
            return new MeanVarianceAccumulator(newWeightSum, sX + other.sX + (other.meanX - meanX) * (other.meanX - meanX) * sScale, meanX + (other.meanX - meanX) * mScale);
        }

        /// <summary>
        /// The current (weighted) number of values that have been accumulated.  
        /// This is equivalent to the sum of the weights. 
        /// If all values have the default weight 1.0, this is equivalent to the count.
        /// </summary>
        [Pure]
        public double WeightSum => weightSum;
        /// <summary>
        /// The currently accumulated (weighted) mean.  Returns 0.0 if empty.
        /// </summary>
        [Pure]
        public double Mean => meanX;
        /// <summary>
        /// The (weighted) variance of the accumulated values.  Returns NaN if empty.  Only the relative weights between the values matter; so two sets of identical values with weights that
        /// differ only by some constant, positive scaling factor will result in the same variance (within a small margin of error due to floating point precision).
        /// </summary>
        [Pure]
        public double Variance => sX / weightSum;
        
        /// <summary>
        /// The square root of the variance.
        /// </summary>
        [Pure]
        public double StandardDeviation => Math.Sqrt(Variance);
        /// <summary>
        /// The sample variance is "sum of values divided by one less than the count"
        /// This is only meaningful if weights are strictly stochastic, which really means: don't use this if you use weights.
        /// The sample variance is undefined if WeightSum is 1.0 or less (it will return a meaningless value).
        /// </summary>
        [Pure]
        public double SampleVariance => sX / (weightSum - 1.0);
        /// <summary>
        /// The sample standard deviation is the square root of the sample variance.
        /// This is only meaningful if weights are strictly stochastic, which really means: don't use this if you use weights.
        /// The sample standard deviation is undefined if WeightSum is 1.0  or less (it will return a meaningless value).
        /// </summary>
        [Pure]
        public double SampleStandardDeviation => Math.Sqrt(SampleVariance);

        /// <summary>
        /// Accumulates all values in the sequence.  (Note that iterating over an IEnumerable is relatively slow compared to other options here).
        /// </summary>
        [Pure]
        public static MeanVarianceAccumulator FromSequence(IEnumerable<double> values)
        {
            var result = new MeanVarianceAccumulator();
            foreach (var value in values)
                result = result.Add(value);
            return result;
        }

        /// <summary>
        /// Accumulates all values in the sequence.
        /// </summary>
        [Pure]
        public static MeanVarianceAccumulator FromSequence(double[] values)
        {
            var result = new MeanVarianceAccumulator();
            foreach (var value in values)
                result = result.Add(value);
            return result;
        }

        /// <summary>
        /// Represents the accumulation of one value.
        /// Equivalent to MeanVarianceAccumulator.Empty.Add(...); 
        /// </summary>
        [Pure]
        public static MeanVarianceAccumulator Init(double firstValue, double firstWeight = 1.0) => new MeanVarianceAccumulator(firstWeight, 0.0, firstValue); //equivalent to adding to an empty distribution.
  
        /// <summary>
        /// The empty accumulator; equivalent to default(MeanVarianceAccumulator)
        /// </summary>
        [Pure]
        public static MeanVarianceAccumulator Empty => default(MeanVarianceAccumulator);

        /// <summary>
        /// Returns a readable string representation including the current mean and standard deviation.
        /// </summary>
        [Pure]
        public override string ToString() => Mean.ToString(CultureInfo.InvariantCulture) + " +/- " + StandardDeviation.ToString(CultureInfo.InvariantCulture);
    }
}
