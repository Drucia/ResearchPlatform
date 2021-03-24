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
        private readonly Models.Input _input;
        private readonly List<bool> _searchTreeAlgorithms;
        private readonly Dictionary<SearchTreeAlgorithm, Result> _results;

        public Task(ICriteriaAlgorithmBuilder builder, IBranchAndBoundHelper helper, 
            Models.Input input, List<bool> searchTreeAlgorithms)
        {
            _criteriaBuilder = builder;
            _branchAndBoundHelper = helper;
            _input = input;
            _searchTreeAlgorithms = searchTreeAlgorithms;
            _results = new Dictionary<SearchTreeAlgorithm, Result>();
        }

        public void Run()
        {
            _criteriaBuilder.Run();
            var weights = _criteriaBuilder.GetWeights();

            var jobsToChoose = _input.Jobs
                                    .Select(job => {
                                        var newJob = new JobWithChoose(job);
                                        CriteriaCalculator.CalculateUtility(newJob, weights);
                                        return newJob;
                                     })
                                    .ToList();

            jobsToChoose.Sort((left, right) => (int)((left.Utility - right.Utility) * 100));

            var bAb = new BranchAndBound(_input.Base, _input.Nodes, _input.DistanceMatrix, jobsToChoose, _branchAndBoundHelper);

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
