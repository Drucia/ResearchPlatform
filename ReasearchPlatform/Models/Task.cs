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
        private readonly IDistancesManager _distanceManager;

        public Task(ICriteriaAlgorithmBuilder builder, IBranchAndBoundHelper helper, 
            Models.Input input, List<bool> searchTreeAlgorithms,
            List<JobToProceed> jobsToProceed, IDistancesManager distanceManager)
        {
            _criteriaBuilder = builder;
            _branchAndBoundHelper = helper;
            _input = input;
            _searchTreeAlgorithms = searchTreeAlgorithms;
            _jobsToProceed = jobsToProceed;
            _results = new Dictionary<SearchTreeAlgorithm, Result>();
            _distanceManager = distanceManager;
        }

        public void Run()
        {
            _criteriaBuilder.Run();
            var weights = _criteriaBuilder.GetWeights();

            CriteriaCalculator.CalculateUtility(_jobsToProceed, weights);

            var bAb = new BranchAndBound(_input.Base, _distanceManager, _jobsToProceed, _branchAndBoundHelper);

            foreach (SearchTreeAlgorithm alg in Enum.GetValues(typeof(SearchTreeAlgorithm)))
            {
                if (_searchTreeAlgorithms[(int)alg])
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var res = bAb.Run(alg);
                    watch.Stop();
                    _results.Add(alg, new Result() { 
                        Jobs = res.ChosenJobs, 
                        Breaks = res.Breaks, 
                        Duration = watch.ElapsedMilliseconds,
                        VisitedNodes = res.VisitedNodes
                    });
                }
            }
        }

        public Dictionary<SearchTreeAlgorithm, Result> GetResults()
        {
            return _results;
        }
    }
}
