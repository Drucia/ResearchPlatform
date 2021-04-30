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
            public List<Break> Breaks { get; set; }
            public int VisitedNodes { get; set; }
            public int DrivenTime { get; set; }
        }

        // input
        private readonly Node _base;
        private readonly IDistancesManager _distancesManager;
        private readonly IBranchAndBoundHelper _helper;
        private int _visitedNodes;
        private BestResult _best;

        // processing
        private List<JobToProceed> _jobsToProceed;

        public BranchAndBound(Node startNode, IDistancesManager distances,
            List<JobToProceed> jobs, IBranchAndBoundHelper helper)
        {
            _helper = helper;
            _base = startNode;
            _distancesManager = distances;
            _visitedNodes = 0;
            _jobsToProceed = new List<JobToProceed>(jobs);
        }

        public BestResult Run(SearchTreeAlgorithm searchTreeAlgorithm, bool turnOffApprox)
        {
            _visitedNodes = 0;
            _best = new BestResult();

            switch (searchTreeAlgorithm)
            {
                case SearchTreeAlgorithm.DFS: return RunWithDFS(turnOffApprox);
                case SearchTreeAlgorithm.Heuristic: return RunWithHeuristic(turnOffApprox);
                case SearchTreeAlgorithm.Random: return RunWithRandom(turnOffApprox);
            }

            return _best;
        }

        private BestResult RunWithHeuristic(bool turnOffApprox)
        {
            // sort jobs according to heuristic
            _jobsToProceed.Sort((left, right) => (int)((right.Profit / right.TimeOfExecution - left.Profit / left.TimeOfExecution) * 100));

            return PrepareAndRunRec(_jobsToProceed, turnOffApprox);
        }

        private BestResult RunWithRandom(bool turnOffApprox)
        {
            // sort jobs according to random
            var tmp = new List<JobToProceed>(_jobsToProceed);
            var random = new Random();
            var sorted = new List<JobToProceed>();

            while(tmp.Count > 0)
            {
                var randomIdx = random.Next(0, tmp.Count);
                sorted.Add(_jobsToProceed[randomIdx]);
                tmp.RemoveAt(randomIdx);
            }

            return PrepareAndRunRec(sorted, turnOffApprox);
        }

        private BestResult RunWithDFS(bool turnOffApprox)
        {
            // sort jobs
            _jobsToProceed.Sort((left, right) => (int)((right.Utility - left.Utility) * 100));

            return PrepareAndRunRec(_jobsToProceed, turnOffApprox);
        }

        private BestResult PrepareAndRunRec(List<JobToProceed> sorted, bool turnOffApprox)
        {
            var currentNode = _base;
            _best = new BestResult() { Value = 0.0, ChosenJobs = new List<JobToProceed>() };
            var dummyJob = new JobToProceed()
            {
                From = _base,
                To = _base,
                Pickup = Tuple.Create(0, IBranchAndBoundHelper.MAX_TIME_WITH_WORKING),
                Delivery = Tuple.Create(0, IBranchAndBoundHelper.MAX_TIME_WITH_WORKING)
            };

            DFSRec(currentNode, dummyJob, new List<JobToProceed>(), new List<Break>(),
                sorted, 0, 0, 0, turnOffApprox);

            _best.VisitedNodes = _visitedNodes;

            return _best;
        }

        private void DFSRec(Node currNode, JobToProceed currentJob, List<JobToProceed> done, List<Break> breaks,
            List<JobToProceed> all, int workTime, int drivenTime, int wholeDrivenTime, bool turnOffApprox)
        {
            _visitedNodes++;

            var currentValue = _helper.CalculateValueOfGoalFunction(done);
            var maxPrice = all.Count > 0 ? all.Max(j => j.Price) : 1;
            var prox = _helper.GetMaxPossibleValue(done, GetRestJobsToDo(done, all, workTime), currentValue, workTime, maxPrice);

            if (turnOffApprox || _best.Value <= prox)
            {
                if (_helper.AreAllConstraintsSatisfied(currNode, currentJob, done, workTime, drivenTime, wholeDrivenTime))
                {
                    done.Add(currentJob);
                    currNode = ExecuteJob(done, out int wT, out int dT, out int wholeDT, breaks);

                    var allPossible = GetPossibleJobsToDo(all, done, currNode, wT);
                    foreach (var job in allPossible)
                    {
                        DFSRec(currNode, job, new List<JobToProceed>(done), new List<Break>(breaks), all, wT, dT, wholeDT, turnOffApprox);
                    }

                    currentValue = _helper.CalculateValueOfGoalFunction(done);

                    // leaf
                    if (allPossible.Count == 0 && _best.Value <= currentValue)
                        ChangeBestResult(currentValue, new List<JobToProceed>(done), breaks, wholeDT);
                }
                else
                {
                    // cut tree
                    if (_best.Value <= currentValue)
                        ChangeBestResult(currentValue, new List<JobToProceed>(done), breaks, wholeDrivenTime);
                }
            }
            else
            {
                // cut tree
                if (_best.Value <= currentValue)
                    ChangeBestResult(currentValue, new List<JobToProceed>(done), breaks, wholeDrivenTime);
            }
        }

        private List<JobToProceed> GetRestJobsToDo(List<JobToProceed> done, List<JobToProceed> all, double currWorkTime)
        {
            return all.Where(j => !done.Contains(j) && currWorkTime <= j.Pickup.Item2).ToList();
        }

        private void ChangeBestResult(double currentValue, List<JobToProceed> done, List<Break> breaks, int wholeDT)
        {
            _best.Value = currentValue;
            _best.ChosenJobs = done;
            _best.Breaks = breaks;
            _best.DrivenTime = wholeDT;
        }

        private Node ExecuteJob(List<JobToProceed> done, out int wT, out int dT, out int wholeDT, List<Break> breaks)
        {
            breaks.Clear();
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
                    if (breakTime > 0)
                    {
                        breaks.Add(new Break() { Place = lastNode, DrivenTime = curDrivenTime });
                        curDrivenTime = 0;
                    }

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
                    if (breakTime > 0)
                    {
                        breaks.Add(new Break() { Place = job.From, DrivenTime = curDrivenTime });
                        curDrivenTime = 0;
                    }

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
