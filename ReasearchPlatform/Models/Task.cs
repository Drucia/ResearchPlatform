using ResearchPlatform.Algorithms;
using ResearchPlatform.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using static ResearchPlatform.Algorithms.BranchAndBound;

namespace ResearchPlatform.Models
{
    public class Task
    {
        private readonly static int NUMBER_OF_REPEAT_ALG = 5;

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
            var criteriaWatch = System.Diagnostics.Stopwatch.StartNew();
            _criteriaBuilder.Run();
            criteriaWatch.Stop();

            var jobsWithUtility = _criteriaBuilder.GetJobsWithCalculatedUtility();

            var bAb = new BranchAndBound(_input.Base, _distanceManager, jobsWithUtility, _branchAndBoundHelper);

            foreach (SearchTreeAlgorithm alg in Enum.GetValues(typeof(SearchTreeAlgorithm)))
            {
                if (_searchTreeAlgorithms[(int)alg])
                {
                    var res = new BestResult();
                    var times = new List<long>();
                    var counter = 0;
                    var turnOffApprox = false;

                    while (counter < NUMBER_OF_REPEAT_ALG)
                    {
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        res = bAb.Run(alg, turnOffApprox);
                        watch.Stop();
                        times.Add(watch.ElapsedMilliseconds);

                        counter++;
                    }

                    turnOffApprox = true;

                    var w = System.Diagnostics.Stopwatch.StartNew();
                    var resWithoutApp = bAb.Run(alg, turnOffApprox);
                    w.Stop();

                    _results.Add(alg, new Result() { 
                        Jobs = res.ChosenJobs, 
                        Breaks = res.Breaks, 
                        Duration = (long) times.Average(),
                        CriteriaDuration = criteriaWatch.ElapsedMilliseconds,
                        VisitedNodes = Math.Round((double) res.VisitedNodes / resWithoutApp.VisitedNodes, 2),
                        AmountOfJobs = _jobsToProceed.Count,
                        Value = res.Value
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
