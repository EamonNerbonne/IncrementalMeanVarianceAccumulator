﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using IncrementalMeanVarianceAccumulator;
using Xunit;

namespace IncrementalMeanVarianceAccumulatorTest
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class BasicTests
    {
        [Fact]
        public void EmptyAccumulatorHasNanVariance()
        {
            var acc = MeanVarianceAccumulator.Empty;
        }
    }
}
