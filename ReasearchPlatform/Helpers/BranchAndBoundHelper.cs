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
        private double _maxPossibleProfit;
        private double _maxPossibleUtility;

        public BranchAndBoundHelper(DistancesManager distancesManager, IEnumerable<int> weights,
            Configuration configuration)
        {
            _manager = distancesManager;
            _weights = weights.Select(w => w / 100.0).ToList();
            _configuration = configuration;
        }

        public void SetMaxForUtilityAndProfit(List<JobToProceed> allJobs)
        {
            _maxPossibleProfit = GetMaxPossibleProfit(allJobs);
            _maxPossibleUtility = GetMaxPossibleUtility(allJobs);
        }

        private double GetMaxPossibleProfit(List<JobToProceed> allJobs)
        {
            var tmpWorkTime = 0.0;
            var jobsToDo = new List<JobToProceed>(allJobs);
            jobsToDo.Sort((left, right) => (int)((right.Profit / right.TimeOfExecution) - (left.Profit / left.TimeOfExecution)));
            var maxPossProfit = 0.0;

            while (jobsToDo.Count > 0 && tmpWorkTime <= IBranchAndBoundHelper.MAX_TIME_WITH_WORKING)
            {
                var chosenJob = jobsToDo[0];
                var percentage = (IBranchAndBoundHelper.MAX_TIME_WITH_WORKING - tmpWorkTime) / chosenJob.TimeOfExecution;

                if (percentage >= 1)
                    maxPossProfit += chosenJob.Profit;
                else
                    maxPossProfit += chosenJob.Profit * percentage;

                tmpWorkTime += chosenJob.TimeOfExecution;

                jobsToDo.RemoveAt(0);
            }

            return Math.Round(maxPossProfit, 0);
        }

        private double GetMaxPossibleUtility(List<JobToProceed> allJobs)
        {
            var tmpWorkTime = 0.0;
            var jobsToDo = new List<JobToProceed>(allJobs);
            jobsToDo.Sort((left, right) => (int)(((right.Utility / right.TimeOfExecution) - (left.Utility / left.TimeOfExecution)) * 1000));
            var maxPossUtility = 0.0;

            while (jobsToDo.Count > 0 && tmpWorkTime <= IBranchAndBoundHelper.MAX_TIME_WITH_WORKING)
            {
                var chosenJob = jobsToDo[0];
                var percentage = (IBranchAndBoundHelper.MAX_TIME_WITH_WORKING - tmpWorkTime) / chosenJob.TimeOfExecution;

                if (percentage >= 1)
                    maxPossUtility += chosenJob.Utility;
                else
                    maxPossUtility += chosenJob.Utility * percentage;

                tmpWorkTime += chosenJob.TimeOfExecution;

                jobsToDo.RemoveAt(0);
            }

            return Math.Round(maxPossUtility, 4);
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
            var timeToStart = (int)_manager.GetDistanceBetween(currNode, currentJob.From).DurationInSeconds / 60;
            var timeFromStartToEnd = (int)_manager.GetDistanceBetween(currentJob.From, currentJob.To).DurationInSeconds / 60;

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

        public double CalculateValueOfGoalFunction(List<JobToProceed> done, double workTime)
        {
            if (done.Count < 2)
                return 0.0;

            var profitAndTime = CalculateRealProfit(done);
            var realProfit = profitAndTime;

            var utilityAvg = done.Sum(job => job.Utility) / _maxPossibleUtility;
            var price = realProfit / _maxPossibleProfit;
            var time = workTime / IBranchAndBoundHelper.MAX_TIME_WITH_WORKING;

            return _weights[0] * utilityAvg + _weights[1] * price - _weights[2] * time;
        }

        public List<double> CalculateFactorsOfGoalFunction(List<JobToProceed> done, double workTime)
        {
            if (done.Count < 2)
                return new List<double>() { 0, 0, 0 };

            var profitAndTime = CalculateRealProfit(done);
            var realProfit = profitAndTime;

            var utilityAvg = done.Sum(job => job.Utility) / _maxPossibleUtility;
            var price = realProfit / _maxPossibleProfit;
            var time = workTime / IBranchAndBoundHelper.MAX_TIME_WITH_WORKING;

            return new List<double>() { Math.Round(utilityAvg, 2), Math.Round(price, 2), Math.Round(time, 2) };
        }

        private double CalculateValueOfHightLimes(List<JobToProceed> done, List<JobToProceed> chosen, double workTime, double percForLast)
        {
            var tmp = new List<JobToProceed>();
            var allRes = new List<double>();
            var profitDone = 0.0;

            if (done.Count > 0)
            {
                profitDone = CalculateRealProfit(done);
            }

            for (var idx = 0; idx < chosen.Count; idx++)
            {
                tmp.Add(chosen[idx]);
                var avgProfit = 0.0;
                var avgUtility = 0.0;
                var time = 0.0;

                if (idx == chosen.Count - 1)
                {
                    avgProfit = (tmp.SkipLast(1).Sum(c => c.Profit) + chosen[idx].Profit * percForLast + profitDone) / _maxPossibleProfit;
                    avgUtility = (done.Concat(tmp.SkipLast(1)).Sum(j => j.Utility) + chosen[idx].Utility * percForLast) / _maxPossibleUtility;
                    time = (tmp.SkipLast(1).Sum(c => c.TimeOfExecution) + chosen[idx].TimeOfExecution * percForLast + workTime) / IBranchAndBoundHelper.MAX_TIME_WITH_WORKING;
                }
                else
                {
                    avgProfit = (tmp.Sum(c => c.Profit) + profitDone) / _maxPossibleProfit;
                    avgUtility = done.Concat(tmp).Sum(j => j.Utility) / _maxPossibleUtility;
                    time = (tmp.Sum(c => c.TimeOfExecution) + workTime) / IBranchAndBoundHelper.MAX_TIME_WITH_WORKING;
                }

                var res = _weights[0] * avgUtility + _weights[1] * avgProfit - _weights[2] * time;
                allRes.Add(res);
            }

            return allRes.Max();
        }

        private double CalculateRealProfit(List<JobToProceed> done)
        {
            return CalculateRealProfitWithStartNode(done, done.First().To);
        }

        private double CalculateRealProfitWithStartNode(List<JobToProceed> done, Node start)
        {
            Tuple<double /* profit */, Node /* last node */> profit =
                          done.Aggregate(Tuple.Create(0.0, start), (acc, job) =>
                          {

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

        public double GetMaxPossibleValue(List<JobToProceed> done, List<JobToProceed> restPossibleJobs, double currentValue, double workTime)
        {
            if (restPossibleJobs.Count == 0)
                return currentValue;
            else
            {
                var jobsToDo = new List<JobToProceed>(restPossibleJobs);
                jobsToDo.Sort((left, right) => (int)((
                _weights[1] * (right.Profit / _maxPossibleProfit) + _weights[0] * (right.Utility / _maxPossibleUtility) - _weights[2] *
                    (right.TimeOfExecution / IBranchAndBoundHelper.MAX_TIME_WITH_WORKING) // right
                - _weights[1] * (left.Profit / _maxPossibleProfit) - _weights[0] * (left.Utility / _maxPossibleUtility) + _weights[2] *
                    (left.TimeOfExecution / IBranchAndBoundHelper.MAX_TIME_WITH_WORKING) // left
                    ) * 1000));
                var tmpWorkTime = workTime;
                var chosenJobs = new List<JobToProceed>();
                var percentageForLast = 1.0;

                while (jobsToDo.Count > 0 && tmpWorkTime <= IBranchAndBoundHelper.MAX_TIME_WITH_WORKING)
                {
                    var chosenJob = jobsToDo[0];
                    chosenJobs.Add(chosenJob);
                    percentageForLast = (IBranchAndBoundHelper.MAX_TIME_WITH_WORKING - tmpWorkTime) / chosenJob.TimeOfExecution;
                    tmpWorkTime += chosenJob.TimeOfExecution;

                    jobsToDo.RemoveAt(0);
                }

                if (chosenJobs.Count > 0)
                {
                    var prox = CalculateValueOfHightLimes(done, chosenJobs, workTime, percentageForLast);
                }

                return chosenJobs.Count == 0 ? currentValue : CalculateValueOfHightLimes(done, chosenJobs, workTime, percentageForLast);
            }
        }
    }
}
