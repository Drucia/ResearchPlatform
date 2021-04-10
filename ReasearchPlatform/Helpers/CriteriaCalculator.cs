using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResearchPlatform
{
    public static class CriteriaCalculator
    {
        public static void CalculateCriteria(List<JobToProceed> jobs, 
            Configuration configuration, DistancesManager distancesManager, 
            Node baze, List<Client> clients)
        {
            jobs.ForEach(job =>
            {
                var distanceToDoJob = distancesManager.GetDistanceBetween(job.From, job.To);
                //var distanceFromBase = distancesManager.GetDistanceBetween(baze, job.To);
                //var distanceToBase = distancesManager.GetDistanceBetween(job.From, baze);

                if (distanceToDoJob != null)// && distanceFromBase != null && distanceToBase != null)
                {
                    //job.Profit = job.Price - (distanceToDoJob.Costs * 0.001 + distanceFromBase.Costs * 0.001 + distanceToBase.Costs * 0.001 +
                    //    ((distanceToDoJob.DistanceInMeters + distanceFromBase.DistanceInMeters + distanceToBase.DistanceInMeters) / 1000) *
                    //    (configuration.AvgFuelConsumption * configuration.FuelCost + configuration.CostOfMaintain));

                    job.Profit = job.Price - (distanceToDoJob.Costs * 0.001 +
                        ((distanceToDoJob.DistanceInMeters) / 1000) * (configuration.AvgFuelConsumption * configuration.FuelCost + configuration.CostOfMaintain));

                    job.ComfortOfWork = configuration.TypeOfLoadingMultipler * job.TypeOfLoading + configuration.RiskMultipler * job.SeizureRisk;
                    //job.TimeOfExecution = 2 * job.LoadingTime + (distanceToDoJob.DurationInSeconds + distanceFromBase.DurationInSeconds
                    //    + distanceToBase.DurationInSeconds) / 60;

                    job.TimeOfExecution = 2 * job.LoadingTime + distanceToDoJob.DurationInSeconds / 60;

                    job.Reliability = job.ClientOpinion;
                    var clientIdx = clients.FindIndex(c => c.ClientID == job.ClientId);
                    job.PossibilityOfNextJobs = clientIdx == -1 ? 0 : clients[clientIdx].AmountOfDoneJobs;
                } else
                {
                    job.ID = -1;
                }
            });
        }

        public static void CalculateUtility(List<JobToProceed> jobsToProceed, List<double> weights)
        {
            var minProfit = jobsToProceed.Min(job => job.Profit);
            var maxProfit = jobsToProceed.Max(job => job.Profit) + Math.Abs(minProfit < 0 ? minProfit : 0);

            var minTimeOfExec = jobsToProceed.Min(job => job.TimeOfExecution);
            var maxClientOpinion = jobsToProceed.Max(job => job.ClientOpinion);
            var maxPossOfNextJobs = jobsToProceed.Max(job => job.PossibilityOfNextJobs);
            var maxComfortOfWork = jobsToProceed.Max(job => job.ComfortOfWork);

            jobsToProceed.ForEach(job => {
                job.Utility =
                    weights[(int)Criteria.Profit] * ((job.Profit + Math.Abs(minProfit < 0 ? minProfit : 0)) / maxProfit) +
                    weights[(int)Criteria.DrivingTime] * (minTimeOfExec / job.TimeOfExecution) +
                    weights[(int)Criteria.CustomerReliability] * (job.ClientOpinion / maxClientOpinion) +
                    weights[(int)Criteria.CompletedJobs] * (job.PossibilityOfNextJobs / maxPossOfNextJobs) +
                    weights[(int)Criteria.ComfortOfWork] * (job.ComfortOfWork / maxComfortOfWork);
            });
        }
    }
}