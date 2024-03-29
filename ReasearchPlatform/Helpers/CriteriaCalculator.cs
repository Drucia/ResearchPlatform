﻿using ResearchPlatform.Models;
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

                if (distanceToDoJob != null)
                {
                    job.Profit = job.Price - (distanceToDoJob.Costs * 0.001 +
                        ((distanceToDoJob.DistanceInMeters) / 1000) * (configuration.AvgFuelConsumption * configuration.FuelCost + configuration.CostOfMaintain));

                    job.ComfortOfWork = configuration.TypeOfLoadingMultipler * job.TypeOfLoading + configuration.RiskMultipler * job.SeizureRisk;

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
    }
}