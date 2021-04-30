﻿using ResearchPlatform.Helpers;
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
            public double Prox { get; set; }
        }

        // input
        private readonly Node _base;
        private readonly IDistancesManager _distancesManager;
        private readonly IBranchAndBoundHelper _helper;
        private int _visitedNodes;
        private BestResult _best;

        // processing
        private readonly List<JobToProceed> _jobsToProceed;

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
            switch (searchTreeAlgorithm)
            {
                case SearchTreeAlgorithm.DFS: return RunWithDFS(turnOffApprox);
                case SearchTreeAlgorithm.BFS:
                    break;
                case SearchTreeAlgorithm.Heuristic:
                    break;
                case SearchTreeAlgorithm.Random:
                    break;
            }

            return new BestResult();
        }

        private BestResult RunWithDFS(bool turnOffApprox)
        {
            // sort jobs
            _jobsToProceed.Sort((left, right) => (int)((right.Utility - left.Utility) * 100));

            var currentNode = _base;
            _best = new BestResult(){ Value = 0.0, ChosenJobs = new List<JobToProceed>()};
            var dummyJob = new JobToProceed() { 
                From = _base, 
                To = _base,
                Pickup = Tuple.Create(0, IBranchAndBoundHelper.MAX_TIME_WITH_WORKING),
                Delivery = Tuple.Create(0, IBranchAndBoundHelper.MAX_TIME_WITH_WORKING)
            };

            DFSRec(currentNode, dummyJob, new List<JobToProceed>(), new List<Break>(),
                _jobsToProceed, 0, 0, 0, turnOffApprox);

            _best.VisitedNodes = _visitedNodes;

            int wT, dT, wholeDT;
            var breaks = new List<Break>();
            ExecuteJob(_best.ChosenJobs, out wT, out dT, out wholeDT, breaks);

            return _best;
        }

        private void DFSRec(Node currNode, JobToProceed currentJob, List<JobToProceed> done, List<Break> breaks,
            List<JobToProceed> all, int workTime, int drivenTime, int wholeDrivenTime, bool turnOffApprox)
        {
            _visitedNodes++;

            var currentValue = _helper.CalculateValueOfGoalFunction(done);
            var prox = _helper.GetMaxPossibleValue(done, GetRestJobsToDo(done, all, workTime), currentValue, workTime);

            if (_best.Value <= prox || turnOffApprox)
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
                        ChangeBestResult(currentValue, new List<JobToProceed>(done), breaks, prox);
                }
                else
                {
                    // cut tree
                    if (_best.Value <= currentValue)
                        ChangeBestResult(currentValue, new List<JobToProceed>(done), breaks, prox);
                }
            }
            else
            {
                // cut tree
                if (_best.Value <= currentValue)
                    ChangeBestResult(currentValue, new List<JobToProceed>(done), breaks, prox);
            }
        }

        private List<JobToProceed> GetRestJobsToDo(List<JobToProceed> done, List<JobToProceed> all, double currWorkTime)
        {
            return all.Where(j => !done.Contains(j) && currWorkTime <= j.Pickup.Item2).ToList();
        }

        private void ChangeBestResult(double currentValue, List<JobToProceed> done, List<Break> breaks, double prox)
        {
            _best.Value = currentValue;
            _best.ChosenJobs = done;
            _best.Breaks = breaks;
            _best.Prox = prox;
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
