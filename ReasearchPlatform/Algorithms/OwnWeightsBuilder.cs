using System.Collections.Generic;

namespace ResearchPlatform.Algorithms
{
    public class OwnWeightsBuilder : ICriteriaAlgorithmBuilder
    {
        private List<double> _weights;

        public OwnWeightsBuilder(List<double> weights)
        {
            _weights = weights;
        }

        public List<double> GetWeights()
        {
            return _weights;
        }

        public void Run()
        {
        }
    }
}
