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
        private readonly Node _base;

        public BranchAndBoundHelper(DistancesManager distancesManager, IEnumerable<int> weights, Configuration configuration, Node base_)
        {
            _manager = distancesManager;
            _weights = weights.Select(w => w / 100.0).ToList();
            _configuration = configuration;
            _base = base_;
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

        public double CalculateValueOfGoalFunction(List<JobToProceed> done)
        {
            if (done.Count < 2)
                return 0.0;

            var profitAndTime = CalculateRealProfitAndDrive(done);
            var realProfit = profitAndTime.Item1;
            var realTime = profitAndTime.Item2;

            var utilityAvg = done.Average(job => job.Utility);
            var price = (realProfit / done.Sum(job => job.Price));
            var time = realTime / IBranchAndBoundHelper.MAX_TIME_WITH_WORKING;

            return _weights[0] * utilityAvg + _weights[1] * price - _weights[2] * time;
        }

        private double CalculateValueOfHightLimes(List<JobToProceed> chosen)
        {
            // for done
            //var realProfitSum = 0.0;
            //var realTimeSum = 0.0;
            //var utilitySum = 0.0;

            //var realProfitSum = done.Sum(d => d.Profit);
            //var realTimeSum = done.Sum(d => d.TimeOfExecution);
            //var utilitySum = done.Sum(job => job.Utility);

            //if (done.Count > 1)
            //{
            //    var profitAndTime = CalculateRealProfitAndDrive(done);

            //    realProfitSum = profitAndTime.Item1;
            //    realTimeSum = profitAndTime.Item2;
            //    utilitySum = done.Sum(job => job.Utility);
            //}

            // for chosen
            var chosenProfitSum = CalculateRealProfitAndDrive(chosen);
            var chosenUtilitySum = chosen.Sum(j => j.Utility);

            //var avgProfit = (realProfitSum + chosenProfitSum.Item1) / (done.Sum(d => d.Price) + chosen.Sum(c => c.Price));
            var avgProfit = chosenProfitSum.Item1 / chosen.Sum(c => c.Price);
            var avgUtility = chosenUtilitySum / chosen.Count;

            var res = _weights[0] * avgUtility + _weights[1] * avgProfit;

            return res;
        }

        private Tuple<double, double> CalculateRealProfitAndDrive(List<JobToProceed> done)
        {
            return CalculateRealProfitAndDriveWithStartNode(done, done.First().To);
        }

        private Tuple<double, double> CalculateRealProfitAndDriveWithStartNode(List<JobToProceed> done, Node start)
        {
            Tuple<Tuple<double /* profit */, double /* time */>, Node /* last node */> profit =
                          done.Aggregate(Tuple.Create(Tuple.Create(0.0, 0.0), start), (acc, job) => {

                              var currProfit = acc.Item1.Item1;
                              var currTimeForDrive = acc.Item1.Item2;

                              var distanceToStart = _manager.GetDistanceBetween(acc.Item2, job.From);
                              var distanceFromStartToEnd = _manager.GetDistanceBetween(job.From, job.To);

                              currProfit += job.Price - (distanceToStart.Costs * 0.001 + distanceFromStartToEnd.Costs * 0.001 +
                              ((distanceToStart.DistanceInMeters + distanceFromStartToEnd.DistanceInMeters) / 1000) *
                              (_configuration.AvgFuelConsumption * _configuration.FuelCost + _configuration.CostOfMaintain));

                              currTimeForDrive += distanceToStart.DurationInSeconds / 60;
                              currTimeForDrive += 2 * job.LoadingTime;

                              return Tuple.Create(Tuple.Create(currProfit, currTimeForDrive), job.To);
                          });

            return profit.Item1;
        }

        public double GetMaxPossibleValue(List<JobToProceed> done, List<JobToProceed> restPossibleJobs, double currentValue, double workTime)
        {
            if (restPossibleJobs.Count == 0)
                return currentValue;
            else
            {
                var jobsToDo = new List<JobToProceed>(restPossibleJobs);
                var maxPrice = jobsToDo.Max(j => j.Price);
                jobsToDo.Sort((left, right) => (int) ((
                _weights[1] * (right.Profit / maxPrice) + _weights[0] * right.Utility // right
                    - _weights[1] * (left.Profit / maxPrice) - _weights[0] * left.Utility // left
                    ) * 1000));
                var tmpWorkTime = workTime;
                var chosenJobs = new List<JobToProceed>();
                
                while(jobsToDo.Count > 0 && tmpWorkTime <= IBranchAndBoundHelper.MAX_TIME_WITH_WORKING)
                {
                    var chosenJob = jobsToDo[0];
                    if (chosenJob.Profit > 0)
                    {
                        chosenJobs.Add(chosenJob);
                        tmpWorkTime += chosenJob.TimeOfExecution;
                    }
                    
                    jobsToDo.RemoveAt(0);
                }

                return chosenJobs.Count == 0 ? currentValue : CalculateValueOfHightLimes(done.Concat(chosenJobs).ToList());
            }
        }
    }
}
