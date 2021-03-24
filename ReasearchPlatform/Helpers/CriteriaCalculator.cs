using ResearchPlatform.Models;
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
                var distanceFromBase = distancesManager.GetDistanceBetween(baze, job.To);
                var distanceToBase = distancesManager.GetDistanceBetween(job.From, baze);

                job.Profit = job.Price - (distanceToDoJob.Costs * 0.01 + distanceFromBase.Costs * 0.01 + distanceToBase.Costs * 0.01 +
                    ((distanceToDoJob.DistanceInMeters + distanceFromBase.DistanceInMeters + distanceToBase.DistanceInMeters) / 1000) *
                    (configuration.AvgFuelConsumption * configuration.FuelCost + configuration.CostOfMaintain));

                job.ComfortOfWork = configuration.TypeOfLoadingMultipler * job.TypeOfLoading + configuration.RiskMultipler * job.SeizureRisk;
                job.TimeOfExecution = 2 * job.LoadingTime + (distanceToDoJob.DurationInSeconds + distanceFromBase.DurationInSeconds
                    + distanceToBase.DurationInSeconds) / 60;

                job.Reliability = job.ClientOpinion;
                job.PossibilityOfNextJobs = clients.Find(c => c.ClientID == job.ClientId).AmountOfDoneJobs;
            });
        }
    }
}