using ResearchPlatform.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ReasearchPlatformUT
{
    public class AHPBuilderTests
    {
        private readonly List<List<string>> comparisionMatrix = new List<List<string>>(){
            new List<string>(){ "1", "5", "4", "7"},
            new List<string>(){ "0.2", "1", "0.5", "3"},
            new List<string>(){ "0.25", "2", "1", "3"},
            new List<string>(){ "0.14", "0.33", "0.33", "1"},
        };

        private readonly List<double> expectedSumOfComparisions = new List<double>() { 
            1.59, 8.33, 5.83, 14
        };

        private readonly List<List<double>> expectedNormalizedMatrix = new List<List<double>>() {
            new List<double>(){0.63, 0.60, 0.69, 0.50},
            new List<double>(){0.13, 0.12, 0.09, 0.21},
            new List<double>(){0.16, 0.24, 0.17, 0.21},
            new List<double>(){0.09, 0.04, 0.06, 0.07},
        };

        private readonly List<double> expectedWeights = new List<double>() {
            0.60, 0.14, 0.20, 0.06
        };

        [Fact]
        public void CalculateSumOfComparisons()
        {
            var ahpBuilder = new AHPBuilder(comparisionMatrix, null);
            ahpBuilder.CalculateSumOfComparisons();

            var res = ahpBuilder._tmp.Select(t => Math.Round(t, 2));

            Assert.Equal(expectedSumOfComparisions, res);
        }

        [Fact]
        public void NormalizeMatrix()
        {
            var ahpBuilder = new AHPBuilder(comparisionMatrix, null);
            ahpBuilder.CalculateSumOfComparisons()
                      .NormalizeMatrix();

            var res = ahpBuilder._normalizedMatrix
                .Select(row => row.Select(col => Math.Round(col, 2)).ToList()).ToList();

            Assert.Equal(expectedNormalizedMatrix, res);
        }

        [Fact]
        public void CalculateCriteriaWeights()
        {
            var ahpBuilder = new AHPBuilder(comparisionMatrix, null);
            ahpBuilder.CalculateSumOfComparisons()
                                .NormalizeMatrix()
                                .CalculateCriteriaWeights();

            var res = ahpBuilder.GetWeights().Select(r => Math.Round(r, 2)).ToList();

            Assert.Equal(expectedWeights, res);
        }
    }
}
