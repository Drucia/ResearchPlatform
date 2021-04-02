using ResearchPlatform.Helpers;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResearchPlatform.Algorithms
{
    public class BranchAndBound
    {
        public struct BestResult
        {
            public double Value { get; set; }
            public List<JobToProceed> ChosenJobs { get; set; }
        }

        // input
        private readonly Node _base;
        private readonly IDistancesManager _distancesManager;
        private readonly IBranchAndBoundHelper _helper;
        private readonly List<JobToProceed> _jobs;
        private BestResult _best;

        // processing
        private readonly List<JobToProceed> _jobsToProceed;

        public BranchAndBound(Node startNode, IDistancesManager distances,
            List<JobToProceed> jobs, IBranchAndBoundHelper helper)
        {
            _helper = helper;
            _base = startNode;
            _distancesManager = distances;
            _jobs = jobs;
            _jobsToProceed = new List<JobToProceed>(jobs);
        }

        public List<JobToProceed> Run(SearchTreeAlgorithm searchTreeAlgorithm)
        {
            switch(searchTreeAlgorithm)
            {
                case SearchTreeAlgorithm.DFS: return RunWithDFS();
            }

            return new List<JobToProceed>();
        }

        private List<JobToProceed> RunWithDFS()
        {
            // sort jobs
            _jobsToProceed.Sort((left, right) => (int)((left.Utility - right.Utility) * 100));

            var currentNode = _base;
            _best = new BestResult(){ Value = double.NegativeInfinity, ChosenJobs = new List<JobToProceed>()};
            var dummyJob = new JobToProceed() { 
                From = _base, 
                To = _base,
                Pickup = Tuple.Create(0, IBranchAndBoundHelper.MAX_TIME_WITH_WORKING),
                Delivery = Tuple.Create(0, IBranchAndBoundHelper.MAX_TIME_WITH_WORKING)
            };

            var results = new List<List<JobToProceed>>();

            DFSRec(currentNode, dummyJob, new List<JobToProceed>(), 
                _jobsToProceed, 0, 0, 0, results);

            return _best.ChosenJobs;
        }

        private void DFSRec(Node currNode, JobToProceed currentJob, List<JobToProceed> done, 
            List<JobToProceed> all, int workTime, int drivenTime, int wholeDrivenTime, List<List<JobToProceed>> allCheckedJobsPath)
        {
            var currentValue = _helper.CalculateValueOfGoalFunction(_base, done, workTime);

            if (_helper.AreAllConstraintsSatisfied(currNode, currentJob, done, workTime, drivenTime, wholeDrivenTime))
            {
                done.Add(currentJob);
                currNode = ExecuteJob(done, out int wT, out int dT, out int wholeDT);

                var allPossible = GetPossibleJobsToDo(all, done, currNode, wT);
                foreach (var job in allPossible)
                {
                    DFSRec(currNode, job, done, all, wT, dT, wholeDT, allCheckedJobsPath);
                }

                currentValue = _helper.CalculateValueOfGoalFunction(_base, done, workTime);

                // leaf
                if (allPossible.Count == 0 && _best.Value <= currentValue)
                {
                    ChangeBestResult(currentValue, new List<JobToProceed>(done));
                    allCheckedJobsPath.Add(new List<JobToProceed>(done));
                }
            }
            else
            {
                // cut tree
                if (_best.Value <= currentValue)                
                    ChangeBestResult(currentValue, new List<JobToProceed>(done));

                allCheckedJobsPath.Add(new List<JobToProceed>(done));
            }

            if (done.Count > 1)
                done.RemoveAt(done.Count - 1);
        }

        private void ChangeBestResult(double currentValue, List<JobToProceed> done)
        {
            _best.Value = currentValue;
            _best.ChosenJobs = done;
        }

        private Node ExecuteJob(List<JobToProceed> done, out int wT, out int dT, out int wholeDT)
        {
            Tuple<List<int>, Node /* last node */> times =
                done.Aggregate(Tuple.Create(new List<int>(Enumerable.Repeat(0, 3)), done.First().To), (acc, job) => {

                    var curWorkTime = acc.Item1[0];
                    var curDrivenTime = acc.Item1[1];
                    var wholeDrivenTime = acc.Item1[2];

                    var lastNode = acc.Item2;

                    var timeToStart = GetTimeToGo(lastNode, job.From);
                    var timeFromStartToEnd = GetTimeToGo(job.From, job.To);

                    var breakTime = curDrivenTime + timeToStart >= IBranchAndBoundHelper.MAX_TIME_WITH_DRIVING 
                        ? IBranchAndBoundHelper.BREAK_TIME : 0;

                    // if break then zero driven time
                    curDrivenTime = breakTime > 0 ? 0 : curDrivenTime;

                    // add break time if necessary in commuting node
                    // add travel time from last node to start node
                    curDrivenTime += timeToStart;
                    wholeDrivenTime += timeToStart;
                    curWorkTime = Math.Max(curWorkTime + breakTime + timeToStart, job.Pickup.Item1);

                    // add loading time
                    curWorkTime += job.LoadingTime;

                    breakTime = curDrivenTime + timeFromStartToEnd >= IBranchAndBoundHelper.MAX_TIME_WITH_DRIVING 
                        ? IBranchAndBoundHelper.BREAK_TIME : 0;

                    // if break then zero driven time
                    curDrivenTime = breakTime > 0 ? 0 : curDrivenTime;

                    // add break time if necessary in start node
                    // add travel time from start to end node
                    curDrivenTime += timeFromStartToEnd;
                    wholeDrivenTime += timeFromStartToEnd;
                    curWorkTime = Math.Max(curWorkTime + breakTime + timeFromStartToEnd, job.Delivery.Item1);

                    // add loading time
                    curWorkTime += job.LoadingTime;
                    acc.Item1[0] = curWorkTime;
                    acc.Item1[1] = curDrivenTime;
                    acc.Item1[2] = wholeDrivenTime;

                    return Tuple.Create(acc.Item1, job.To);
                });

            wT = times.Item1[0];
            dT = times.Item1[1];
            wholeDT = times.Item1[2];

            return times.Item2;
        }

        private List<JobToProceed> GetPossibleJobsToDo(List<JobToProceed> jobs, List<JobToProceed> done, Node currentNode, int workTime)
        {
            return jobs
                .Where(j => !done.Contains(j) && IsPossibleToGoToJob(j, currentNode, workTime)).ToList();
        }

        private bool IsPossibleToGoToJob(JobToProceed job, Node currentNode, int workTime)
        {
            return job.Pickup.Item2 > (workTime + GetTimeToGo(currentNode, job.From)); 
        }

        private int GetTimeToGo(Node currentNode, Node nextNode)
        {
            return (int) _distancesManager.GetDistanceBetween(currentNode, nextNode).DurationInSeconds / 60;
        }
    }
}
