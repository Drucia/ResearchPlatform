using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ResearchPlatform.Algorithms
{
    public class AHPBuilder : ICriteriaAlgorithmBuilder
    {
        private static readonly double RANDOM_JUDGEMENT_FOR_4_CRITERIA = 0.9;
        private static readonly double CONSISTENCY_LIMES = 0.1;

        public List<List<double>> _matrix;
        public List<List<double>> _normalizedMatrix;
        public List<double> _tmp;
        private List<double> _weights;
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
            _tmp = Enumerable.Repeat(0.0, _matrix.Count).ToList();
            _weights = Enumerable.Repeat(0.0, _matrix.Count).ToList();
        }

        public AHPBuilder CalculateSumOfComparisons()
        {
            for (int row = 0; row < _matrix.Count; row++)
            {
                for (int col = 0; col < _matrix.Count; col++)
                {
                    _tmp[col] += _matrix[row][col];
                }
            }

            return this;
        }

        public AHPBuilder NormalizeMatrix()
        {
            for (int row = 0; row < _matrix.Count; row++)
            {
                for (int col = 0; col < _matrix.Count; col++)
                {
                    _normalizedMatrix[row][col] = Math.Round(_matrix[row][col] / _tmp[col], 4);
                }
            }

            return this;
        }

        public AHPBuilder CalculateCriteriaWeights()
        {
            for (int row = 0; row < _matrix.Count; row++)
            {
                _weights[row] = Math.Round(_normalizedMatrix[row].Sum() / _normalizedMatrix.Count(), 2);
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
            for (int row = 0; row < _matrix.Count; row++)
            {
                for (int col = 0; col < _matrix.Count; col++)
                {
                    _matrix[row][col] = Math.Round(_matrix[row][col] * _weights[col], 2);
                }
            }
        }

        private void CalculateWeightedSumValue()
        {
            for (int row = 0; row < _matrix.Count; row++)
            {
                _tmp[row] = Math.Round(_matrix[row].Sum(), 2);
            }
        }

        private void CalculateRatioOfWeightedSumAnWeights()
        {
            for (int row = 0; row < _matrix.Count; row++)
            {
                _tmp[row] = Math.Round(_tmp[row] / _weights[row], 2);
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

        public void Run()
        {
            this.CalculateSumOfComparisons()
                .NormalizeMatrix()
                .CalculateCriteriaWeights()
                .CalculateMatrixConsistency();
        }

        public List<double> GetWeights()
        {
            return _weights;
        }
    }
}
