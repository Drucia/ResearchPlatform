using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResearchPlatform.Algorithms
{
    public class PROMETHEEBuilder : ICriteriaAlgorithmBuilder
    {
        private List<double> _weights;
        private List<JobToProceed> _jobs;
        private List<JobWithCriteria> _decisionMatrix;

        private struct JobWithCriteria
        {
            public JobToProceed Job { get; set; }
            public double Profit { get; set; } // beneficial
            public double ComfortOfWork { get; set; } // beneficial
            public double TimeOfExecution { get; set; } // non beneficial
            public double Reliability { get; set; } // beneficial
            public int PossibilityOfNextJobs { get; set; } // beneficial
        }

        public PROMETHEEBuilder(List<double> weights, List<JobToProceed> jobs)
        {
            _weights = weights;
            _jobs = jobs;
            _decisionMatrix = jobs.Select(j => new JobWithCriteria(){ Job = j, Profit = j.Profit, 
                PossibilityOfNextJobs = j.PossibilityOfNextJobs,
                ComfortOfWork = j.ComfortOfWork,
                Reliability = j.Reliability,
                TimeOfExecution = j.TimeOfExecution
            }).ToList();
        }

        public List<JobToProceed> GetJobsWithCalculatedUtility()
        {
            throw new NotImplementedException();
        }

        public PROMETHEEBuilder NormalizeDecisionMatrix()
        {
            var minProfit = _decisionMatrix.Min(j => j.Profit);
            var maxProfit = _decisionMatrix.Max(j => j.Profit);

            var minPossibility = _decisionMatrix.Min(j => j.PossibilityOfNextJobs);
            var maxPossibility = _decisionMatrix.Max(j => j.PossibilityOfNextJobs);

            var minComfort = _decisionMatrix.Min(j => j.ComfortOfWork);
            var maxComfort = _decisionMatrix.Max(j => j.ComfortOfWork);

            var minReliability = _decisionMatrix.Min(j => j.Reliability);
            var maxReliability = _decisionMatrix.Max(j => j.Reliability);

            var minTime = _decisionMatrix.Min(j => j.TimeOfExecution);
            var maxTime = _decisionMatrix.Max(j => j.TimeOfExecution);

            _decisionMatrix.ForEach(job => {
                job.Profit = (job.Profit - minProfit) / (maxProfit - minProfit);
                job.PossibilityOfNextJobs = (job.PossibilityOfNextJobs - minPossibility) / (maxPossibility - minPossibility);
                job.ComfortOfWork = (job.ComfortOfWork - minComfort) / (maxComfort - minComfort);
                job.Reliability = (job.Reliability - minReliability) / (maxReliability - minReliability);
                job.TimeOfExecution = (maxTime - job.TimeOfExecution) / (maxTime - minTime);
            });

            return this;
        }

        public PROMETHEEBuilder CalculateEvaluativeDifferences()
        {
            // TODO
            return this;
        }

        public List<double> GetWeights()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            this.NormalizeDecisionMatrix()
                .CalculateEvaluativeDifferences();
        }
    }
}
