using ResearchPlatform.Algorithms;
using ResearchPlatform.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResearchPlatform.Models
{
    public class Task
    {
        private readonly ICriteriaAlgorithmBuilder _criteriaBuilder;
        private readonly IBranchAndBoundHelper _branchAndBoundHelper;
        private readonly Input _input;
        private readonly List<bool> _searchTreeAlgorithms;
        private readonly List<JobToProceed> _jobsToProceed;
        private readonly Dictionary<SearchTreeAlgorithm, Result> _results;

        public Task(ICriteriaAlgorithmBuilder builder, IBranchAndBoundHelper helper, 
            Models.Input input, List<bool> searchTreeAlgorithms,
            List<JobToProceed> jobsToProceed)
        {
            _criteriaBuilder = builder;
            _branchAndBoundHelper = helper;
            _input = input;
            _searchTreeAlgorithms = searchTreeAlgorithms;
            _jobsToProceed = jobsToProceed;
            _results = new Dictionary<SearchTreeAlgorithm, Result>();
        }

        public void Run()
        {
            _criteriaBuilder.Run();
            var weights = _criteriaBuilder.GetWeights();

            CriteriaCalculator.CalculateUtility(_jobsToProceed, weights);

            var bAb = new BranchAndBound(_input.Base, _input.DistanceMatrix, _jobsToProceed, _branchAndBoundHelper);

            foreach (SearchTreeAlgorithm alg in Enum.GetValues(typeof(SearchTreeAlgorithm)))
            {
                if (_searchTreeAlgorithms[(int)alg])
                    _results.Add(alg, new Result() { Jobs = bAb.Run(alg) });
            }
        }

        public Dictionary<SearchTreeAlgorithm, Result> GetResults()
        {
            return _results;
        }
    }
}
