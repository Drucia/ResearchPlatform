using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ResearchPlatform.Algorithms
{
    public class AHPBuilder
    {
        private static readonly double RANDOM_JUDGEMENT_FOR_4_CRITERIA = 0.9;
        private static readonly double CONSISTENCY_LIMES = 0.1;

        private List<List<double>> _matrix;
        private List<List<double>> _normalizedMatrix;
        private List<double> _tmp;

        public List<double> Weights { get; private set;  }
        public bool IsConsistent { get; private set;  }

        public AHPBuilder(IEnumerable<IEnumerable<string>> matrix)
        {
            var dataTable = new DataTable();
            _matrix = matrix
                .Select(row => row.Select(elem => Convert.ToDouble(dataTable.Compute(elem, "")))
                .ToList())
                .ToList();
            _normalizedMatrix = matrix
                .Select(row => row.Select(elem => Convert.ToDouble(dataTable.Compute(elem, "")))
                .ToList())
                .ToList();
            _tmp = Enumerable.Repeat(0.0, _matrix.Count()).ToList();
            Weights = Enumerable.Repeat(0.0, _matrix.Count()).ToList();
        }

        public AHPBuilder CalculateSumOfComparisons()
        {
            for (int row = 0; row < _matrix.Count(); row++)
            {
                for (int col = 0; col < _matrix.Count(); col++)
                {
                    _tmp[col] += _matrix[row][col];
                }
            }

            return this;
        }

        public AHPBuilder NormalizeMatrix()
        {
            for (int row = 0; row < _matrix.Count(); row++)
            {
                for (int col = 0; col < _matrix.Count(); col++)
                {
                    _normalizedMatrix[row][col] = _matrix[row][col] / _tmp[col];
                }
            }

            return this;
        }

        public AHPBuilder CalculateCriteriaWeights()
        {
            for (int row = 0; row < _matrix.Count(); row++)
            {
                Weights[row] = _matrix[row].Sum() / _matrix.Count();
            }

            return this;
        }

        public AHPBuilder CalculateMatrixConsistency()
        {

            MultiplyMatrixByWeights();
            CalculateWeightedSumValue();
            CalculateRatioOfWeightedSumAnWeights();
            CalculateConsistencyRatio();

            return this;
        }

        private void MultiplyMatrixByWeights()
        {
            for (int row = 0; row < _matrix.Count(); row++)
            {
                for (int col = 0; col < _matrix.Count(); col++)
                {
                    _matrix[row][col] = _matrix[row][col] * Weights[col];
                }
            }
        }

        private void CalculateWeightedSumValue()
        {
            for (int row = 0; row < _matrix.Count(); row++)
            {
                _tmp[row] = _matrix[row].Sum();
            }
        }

        private void CalculateRatioOfWeightedSumAnWeights()
        {
            for (int row = 0; row < _matrix.Count(); row++)
            {
                _tmp[row] = _tmp[row] / Weights[row];
            }
        }

        private void CalculateConsistencyRatio()
        {
            var n = _tmp.Count();
            var lambdaMax = _tmp.Sum() / n;
            var consistencyIdx = (lambdaMax - n) / (n - 1);
            var consistencyRatio = consistencyIdx / RANDOM_JUDGEMENT_FOR_4_CRITERIA;
            IsConsistent = consistencyRatio < CONSISTENCY_LIMES;
        }
    }
}
