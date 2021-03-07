using ResearchPlatform.Algorithms;
using System.Collections.Generic;

namespace ResearchPlatform
{
    public class AlgorithmsManager
    {
        private static AlgorithmsManager _instance;

        private AHPBuilder _AHPBuilder;

        private AlgorithmsManager()
        {
        }

        public static AlgorithmsManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new AlgorithmsManager();
            }

            return _instance;
        }

        public bool CheckMatrixConsistency(IEnumerable<IEnumerable<string>> matrix)
        {
            _AHPBuilder = new AHPBuilder(matrix);
            RunAHP();

            return _AHPBuilder.IsConsistent;
        }

        private void RunAHP()
        {
            _AHPBuilder
                .CalculateSumOfComparisons()
                .NormalizeMatrix()
                .CalculateCriteriaWeights()
                .CalculateMatrixConsistency();
        }
    }
}
