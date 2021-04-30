using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResearchPlatform.Algorithms
{
    public class OwnWeightsBuilder : ICriteriaAlgorithmBuilder
    {
        private List<double> _weights;
        private List<JobToProceed> _jobs;

        public OwnWeightsBuilder(List<double> weights, List<JobToProceed> jobs)
        {
            _weights = weights;
            _jobs = jobs;
        }

        public List<JobToProceed> GetJobsWithCalculatedUtility()
        {
            var minProfit = _jobs.Min(job => job.Profit);
            var maxProfit = _jobs.Max(job => job.Profit) + Math.Abs(minProfit < 0 ? minProfit : 0);

            var minTimeOfExec = _jobs.Min(job => job.TimeOfExecution);
            var maxClientOpinion = _jobs.Max(job => job.ClientOpinion);
            var maxPossOfNextJobs = _jobs.Max(job => job.PossibilityOfNextJobs);
            var maxComfortOfWork = _jobs.Max(job => job.ComfortOfWork);

            _jobs.ForEach(job => {
                job.Utility =
                    (maxProfit == 0 ? 0 : (_weights[(int)Criteria.Profit] * ((job.Profit + Math.Abs(minProfit < 0 ? minProfit : 0)) / maxProfit))) +
                    (job.TimeOfExecution == 0 ? 0 : (_weights[(int)Criteria.DrivingTime] * (minTimeOfExec / job.TimeOfExecution))) +
                    (maxClientOpinion == 0 ? 0 : (_weights[(int)Criteria.CustomerReliability] * (job.ClientOpinion / maxClientOpinion))) +
                    (maxPossOfNextJobs == 0 ? 0 : (_weights[(int)Criteria.CompletedJobs] * (job.PossibilityOfNextJobs / maxPossOfNextJobs))) +
                    (maxComfortOfWork == 0 ? 0 : (_weights[(int)Criteria.ComfortOfWork] * (job.ComfortOfWork / maxComfortOfWork)));
            });

            return _jobs;
        }

        public void Run()
        {
        }
    }
}
