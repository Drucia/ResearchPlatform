using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResearchPlatform.Helpers
{
    public class BranchAndBoundHelper : IBranchAndBoundHelper
    {
        private readonly DistancesManager _manager;
        private readonly List<double> _weights;
        private readonly Configuration _configuration;

        public BranchAndBoundHelper(DistancesManager distancesManager, IEnumerable<int> weights, Configuration configuration)
        {
            _manager = distancesManager;
            _weights = weights.Select(w => w / 100.0).ToList();
            _configuration = configuration;
        }

        public bool AreAllConstraintsSatisfied(Node currNode, JobToProceed currentJob, List<JobToProceed> done, 
            int workTime, int drivenTime, int wholeDrivenTime)
        {
            return 
                wholeDrivenTime <= IBranchAndBoundHelper.MAX_TIME_WITH_WHOLE_DRIVING &&
                drivenTime <= IBranchAndBoundHelper.MAX_TIME_WITH_DRIVING &&
                workTime <= IBranchAndBoundHelper.MAX_TIME_WITH_WORKING &&
                CheckIfExecuteJobIsPossible(currNode, currentJob, workTime, drivenTime, wholeDrivenTime);
        }

        private bool CheckIfExecuteJobIsPossible(Node currNode, JobToProceed currentJob, 
            int workTime, int drivenTime, int wholeDrivenTime)
        {
            // simulate execute of job
            var timeToStart = (int) _manager.GetDistanceBetween(currNode, currentJob.From).DurationInSeconds / 60;
            var timeFromStartToEnd = (int) _manager.GetDistanceBetween(currentJob.From, currentJob.To).DurationInSeconds / 60;

            var breakTime = drivenTime + timeToStart >= IBranchAndBoundHelper.MAX_TIME_WITH_DRIVING
                ? IBranchAndBoundHelper.BREAK_TIME : 0;

            // if break then zero driven time
            drivenTime = breakTime > 0 ? 0 : drivenTime;

            // add break time if necessary in commuting node
            // add travel time from last node to start node
            drivenTime += timeToStart;
            wholeDrivenTime += timeToStart;
            workTime = Math.Max(workTime + breakTime + timeToStart, currentJob.Pickup.Item1);

            if (workTime > currentJob.Pickup.Item2)
                return false;

            // add loading time
            workTime += currentJob.LoadingTime;

            breakTime = drivenTime + timeFromStartToEnd >= IBranchAndBoundHelper.MAX_TIME_WITH_DRIVING
                ? IBranchAndBoundHelper.BREAK_TIME : 0;

            // if break then zero driven time
            drivenTime = breakTime > 0 ? 0 : drivenTime;

            // add break time if necessary in start node
            // add travel time from start to end node
            drivenTime += timeFromStartToEnd;
            wholeDrivenTime += timeFromStartToEnd;
            workTime = Math.Max(workTime + breakTime + timeFromStartToEnd, currentJob.Delivery.Item1);

            if (workTime > currentJob.Delivery.Item2)
                return false;

            // add loading time
            workTime += currentJob.LoadingTime;

            if (workTime > IBranchAndBoundHelper.MAX_TIME_WITH_WORKING || wholeDrivenTime > IBranchAndBoundHelper.MAX_TIME_WITH_WHOLE_DRIVING)
                return false;

            return true;
        }

        public double CalculateValueOfGoalFunction(Node baze, List<JobToProceed> done, int workTime)
        {
            if (done.Count < 2)
                return double.NegativeInfinity;

            var utilityAvg = done.Average(job => job.Utility);
            var price = CalculateRealProfit(baze, done) / done.Sum(job => job.Price);
            var time = (double) workTime / IBranchAndBoundHelper.MAX_TIME_WITH_WORKING;
            var re = _weights[0] * utilityAvg + _weights[1] * price - _weights[2] * time;

            return _weights[0] * utilityAvg + _weights[1] * price - _weights[2] * time;
        }

        private double CalculateRealProfit(Node baze, List<JobToProceed> done)
        {
            Tuple<double /* profit */, Node /* last node */> profit =
                          done.Aggregate(Tuple.Create(0.0, done.First().To), (acc, job) => {

                              var currProfit = acc.Item1;
                              var distanceToStart = _manager.GetDistanceBetween(acc.Item2, job.From);
                              var distanceFromStartToEnd = _manager.GetDistanceBetween(job.From, job.To);

                              currProfit += job.Price - (distanceToStart.Costs * 0.001 + distanceFromStartToEnd.Costs * 0.001 + 
                              ((distanceToStart.DistanceInMeters + distanceFromStartToEnd.DistanceInMeters) / 1000) *
                              (_configuration.AvgFuelConsumption * _configuration.FuelCost + _configuration.CostOfMaintain));

                              return Tuple.Create(currProfit, job.To);
                          });

            return profit.Item1;
        }
    }
}
