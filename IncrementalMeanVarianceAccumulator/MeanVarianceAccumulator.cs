using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace IncrementalMeanVarianceAccumulator
{
	public struct MeanVarianceAccumulator
	{
		readonly double weightSum, sX, meanX;
		MeanVarianceAccumulator(double _weightSum, double _sX, double _meanX) { meanX = _meanX; sX = _sX; weightSum = _weightSum; }

		public MeanVarianceAccumulator Add(double val, double weight = 1.0)
		{
			if (weight == 0.0) return this;//ignore zero-weight stuff...
			double newWeightSum = weightSum + weight;
			double mScale = weight / newWeightSum;
			double sScale = weightSum * weight / newWeightSum;
			return new MeanVarianceAccumulator(newWeightSum, sX + (val - meanX) * (val - meanX) * sScale, meanX + (val - meanX) * mScale);
		}

		public MeanVarianceAccumulator Add(MeanVarianceAccumulator other)
		{
			double newWeightSum = weightSum + other.weightSum;
			double mScale = other.weightSum / newWeightSum;
			double sScale = weightSum * other.weightSum / newWeightSum;
			return new MeanVarianceAccumulator(newWeightSum, sX + other.sX + (other.meanX - meanX) * (other.meanX - meanX) * sScale, meanX + (other.meanX - meanX) * mScale);
		}

		public double Mean => meanX; 
		public double Variance => sX / weightSum;
		public double StandardDeviation => Math.Sqrt(Variance);
		public double SampleVariance => sX / (weightSum - 1.0);
		public double SampleStandardDeviation => Math.Sqrt(SampleVariance);
		public double Weight => weightSum;

		public static MeanVarianceAccumulator FromEnumerable(IEnumerable<double> vals) => vals.Aggregate(new MeanVarianceAccumulator(), (mv, v) => mv.Add(v));
        public static MeanVarianceAccumulator Init(double firstValue, double firstWeight = 1.0) => new MeanVarianceAccumulator(firstWeight, 0.0, firstValue); //equivalent to adding to an empty distribution.
        public static MeanVarianceAccumulator Empty => default(MeanVarianceAccumulator);

		public override string ToString() => Mean.ToString(CultureInfo.InvariantCulture) + " +/- " + StandardDeviation.ToString(CultureInfo.InvariantCulture);
	}
}
