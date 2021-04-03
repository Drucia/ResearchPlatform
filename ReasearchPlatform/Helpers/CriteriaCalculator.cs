using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResearchPlatform
{
    public static class CriteriaCalculator
    {
        private static readonly int SHIFT_FOR_UTILITY = 100000;
        public static void CalculateCriteria(List<JobToProceed> jobs, 
            Configuration configuration, DistancesManager distancesManager, 
            Node baze, List<Client> clients)
        {
            jobs.ForEach(job =>
            {
                var distanceToDoJob = distancesManager.GetDistanceBetween(job.From, job.To);
                var distanceFromBase = distancesManager.GetDistanceBetween(baze, job.To);
                var distanceToBase = distancesManager.GetDistanceBetween(job.From, baze);

                if (distanceToDoJob != null && distanceFromBase != null && distanceToBase != null)
                {
                    job.Profit = job.Price - (distanceToDoJob.Costs * 0.001 + distanceFromBase.Costs * 0.001 + distanceToBase.Costs * 0.001 +
                        ((distanceToDoJob.DistanceInMeters + distanceFromBase.DistanceInMeters + distanceToBase.DistanceInMeters) / 1000) *
                        (configuration.AvgFuelConsumption * configuration.FuelCost + configuration.CostOfMaintain));

                    job.ComfortOfWork = configuration.TypeOfLoadingMultipler * job.TypeOfLoading + configuration.RiskMultipler * job.SeizureRisk;
                    job.TimeOfExecution = 2 * job.LoadingTime + (distanceToDoJob.DurationInSeconds + distanceFromBase.DurationInSeconds
                        + distanceToBase.DurationInSeconds) / 60;

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
            // todo
            jobsToProceed.ForEach(job => {
                job.Utility =
                    weights[(int)Criteria.Profit] * job.Profit +
                    weights[(int)Criteria.DrivingTime] * job.TimeOfExecution +
                    weights[(int)Criteria.CustomerReliability] * job.ClientOpinion +
                    weights[(int)Criteria.CompletedJobs] * job.PossibilityOfNextJobs +
                    weights[(int)Criteria.ComfortOfWork] * job.ComfortOfWork + SHIFT_FOR_UTILITY;
            });
        }
    }
}