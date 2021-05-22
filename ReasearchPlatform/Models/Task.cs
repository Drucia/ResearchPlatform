using ResearchPlatform.Algorithms;
using ResearchPlatform.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (_searchTreeAlgorithms.Where(toRun => toRun).Count() == 0)
                return;

            var criteriaWatch = Stopwatch.StartNew();
            _criteriaBuilder.Run();
            criteriaWatch.Stop();

            var jobsWithUtility = _criteriaBuilder.GetJobsWithCalculatedUtility();
            _branchAndBoundHelper.SetMaxForUtilityAndProfit(jobsWithUtility);
            var bAndb = new BranchAndBound(_input.Base, _distanceManager, jobsWithUtility, _branchAndBoundHelper);

            foreach (SearchTreeAlgorithm alg in Enum.GetValues(typeof(SearchTreeAlgorithm)))
            {
                if (_searchTreeAlgorithms[(int)alg])
                {
                    var res = new BestResult();
                    var times = new List<long>();
                    var nodes = new List<int>();
                    var counter = 0;
                    var turnOffApprox = false;

                    while (counter < NUMBER_OF_REPEAT_ALG)
                    {
                        var watch = Stopwatch.StartNew();
                        res = bAndb.Run(alg, turnOffApprox);
                        watch.Stop();
                        times.Add(watch.ElapsedMilliseconds);
                        nodes.Add(res.VisitedNodes);

                        counter++;
                    }

                    _results.Add(alg, new Result() { 
                        Jobs = res.ChosenJobs, 
                        Breaks = res.Breaks, 
                        Duration = (long) times.Average(),
                        CriteriaDuration = criteriaWatch.ElapsedMilliseconds,
                        VisitedNodes = (int) nodes.Average(),
                        AmountOfJobs = _jobsToProceed.Count,
                        Value = res.Value,
                        DrivenTime = res.DrivenTime,
                        Factors = res.Factors,
                        AllJobs = jobsWithUtility
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
