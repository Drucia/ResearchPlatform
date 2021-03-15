using ResearchPlatform.Algorithms;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<List<Job>> RunWith(Configuration configuration, Models.Input input)
        {
            return new List<Job>();
        }
    }
}
