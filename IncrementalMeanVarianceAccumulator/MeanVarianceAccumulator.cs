using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace IncrementalMeanVarianceAccumulator
{
	public struct MeanVarianceAccumulator
	{
		readonly double weightSum, sX, meanX;
		MeanVarianceAccumulator(double _weightSum, double _sX, double _meanX) { meanX = _meanX; sX = _sX; weightSum = _weightSum; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MeanVarianceAccumulator Add(double val, double weight = 1.0)
		{
			if (weight == 0.0) return this;//ignore zero-weight stuff...
			double newWeightSum = weightSum + weight;
			double mScale = weight / newWeightSum;
			double sScale = weightSum * weight / newWeightSum;
			return new MeanVarianceAccumulator(newWeightSum, sX + (val - meanX) * (val - meanX) * sScale, meanX + (val - meanX) * mScale);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public double WeightSum => weightSum;
        public double Mean => meanX;
        public double Variance => sX / weightSum;
		public double StandardDeviation => Math.Sqrt(Variance);
        /// <summary>
        /// The sample variance is "sum of values divided by one less than the count"
        /// This is only meaningful if weights are strictly stochastic, which really means: don't use this if you use weights.
        /// The sample variance is undefined if WeightSum is 1.0  or less (it will return a meaningless value).
        /// </summary>
		public double SampleVariance => sX / (weightSum - 1.0);
        /// <summary>
        /// The sample standard deviation is the square root of the sample variance.
        /// This is only meaningful if weights are strictly stochastic, which really means: don't use this if you use weights.
        /// The sample standard deviation is undefined if WeightSum is 1.0  or less (it will return a meaningless value).
        /// </summary>
		public double SampleStandardDeviation => Math.Sqrt(SampleVariance);

		public static MeanVarianceAccumulator FromEnumerable(IEnumerable<double> vals) => vals.Aggregate(new MeanVarianceAccumulator(), (mv, v) => mv.Add(v));
        public static MeanVarianceAccumulator Init(double firstValue, double firstWeight = 1.0) => new MeanVarianceAccumulator(firstWeight, 0.0, firstValue); //equivalent to adding to an empty distribution.
        public static MeanVarianceAccumulator Empty => default(MeanVarianceAccumulator);

		public override string ToString() => Mean.ToString(CultureInfo.InvariantCulture) + " +/- " + StandardDeviation.ToString(CultureInfo.InvariantCulture);
	}
}
