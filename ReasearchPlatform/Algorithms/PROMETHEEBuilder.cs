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
        public List<JobWithCriteria> _decisionMatrix;
        public Dictionary<JobToProceed, List<JobWithCriteria>> _preferencesMatrix;
        public List<Tuple<JobToProceed, List<double>>> _aggregatePreferences;
        public Dictionary<JobToProceed, Tuple<double, double>> _outrinkingFlows;

        public class JobWithCriteria
        {
            public JobToProceed Job { get; set; }
            public double Profit { get; set; } // beneficial
            public double ComfortOfWork { get; set; } // beneficial
            public double TimeOfExecution { get; set; } // non beneficial
            public double Reliability { get; set; } // beneficial
            public double PossibilityOfNextJobs { get; set; } // beneficial

            public override bool Equals(object obj)
            {
                return obj is JobWithCriteria criteria &&
                       EqualityComparer<JobToProceed>.Default.Equals(Job, criteria.Job) &&
                       Profit == criteria.Profit &&
                       ComfortOfWork == criteria.ComfortOfWork &&
                       TimeOfExecution == criteria.TimeOfExecution &&
                       Reliability == criteria.Reliability &&
                       PossibilityOfNextJobs == criteria.PossibilityOfNextJobs;
            }
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
            _preferencesMatrix = new Dictionary<JobToProceed, List<JobWithCriteria>>();
            _aggregatePreferences = new List<Tuple<JobToProceed, List<double>>>();
            _outrinkingFlows = new Dictionary<JobToProceed, Tuple<double, double>>();
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

        public PROMETHEEBuilder CalculatePreferences()
        {
            for (var row = 0; row < _decisionMatrix.Count; row++)
            {
                var left = _decisionMatrix[row];
                _preferencesMatrix.Add(left.Job, new List<JobWithCriteria>());

                for (var compare = 0; compare < _decisionMatrix.Count; compare++)
                {
                    var right = _decisionMatrix[compare];
                    _preferencesMatrix.GetValueOrDefault(left.Job).Add(new JobWithCriteria()
                    {
                        Job = right.Job,
                        Profit = (left.Profit - right.Profit) <= 0 ? 0 : (left.Profit - right.Profit) * _weights[(int)Criteria.Profit],
                        PossibilityOfNextJobs = (left.PossibilityOfNextJobs - right.PossibilityOfNextJobs) <= 0 ? 0 :
                            (left.PossibilityOfNextJobs - right.PossibilityOfNextJobs) * _weights[(int)Criteria.CompletedJobs],
                        ComfortOfWork = (left.ComfortOfWork - right.ComfortOfWork) <= 0 ? 0 : (left.ComfortOfWork - right.ComfortOfWork) 
                            * _weights[(int)Criteria.ComfortOfWork],
                        Reliability = (left.Reliability - right.Reliability) <= 0 ? 0 : (left.Reliability - right.Reliability) 
                            * _weights[(int)Criteria.CustomerReliability],
                        TimeOfExecution = (left.TimeOfExecution - right.TimeOfExecution) <= 0 ? 0 : (left.TimeOfExecution - right.TimeOfExecution) 
                            * _weights[(int)Criteria.DrivingTime]
                    });
                }
            }

            return this;
        }

        public PROMETHEEBuilder CalculateAggregatePreferences()
        {
            foreach (var job in _preferencesMatrix)
            {
                var agg = new List<double>();
                foreach (var preference in job.Value)
                {
                    agg.Add(
                        (preference.Profit + preference.PossibilityOfNextJobs + preference.ComfortOfWork 
                        + preference.Reliability + preference.TimeOfExecution) / _weights.Sum());
                }
                _aggregatePreferences.Add(Tuple.Create(job.Key, agg));
            }

            return this;
        }

        public PROMETHEEBuilder DetermineOutrankingFlows()
        {
            for (var row = 0; row < _aggregatePreferences.Count; row++)
            {
                var preference = _aggregatePreferences[row];
                _outrinkingFlows.Add(preference.Item1, 
                    Tuple.Create(CalculateLeavingFlow(preference.Item2), CalculateEnteringFlow(row)));
            }
            return this;
        }

        private double CalculateLeavingFlow(List<double> preferences)
        {
            return preferences.Sum() / (preferences.Count - 1);
        }

        private double CalculateEnteringFlow(int column)
        {
            var preferencesByColumn = _aggregatePreferences.Select(p => p.Item2[column]).ToList();
            return preferencesByColumn.Sum() / (preferencesByColumn.Count - 1);
        }

        public void Run()
        {
            this.NormalizeDecisionMatrix()
                .CalculatePreferences()
                .CalculateAggregatePreferences()
                .DetermineOutrankingFlows()
                .CalculateNetOutrankingValue();
        }

        public PROMETHEEBuilder CalculateNetOutrankingValue()
        {
            _jobs = _outrinkingFlows.Select(outranking => {
                var job = outranking.Key;
                var netValue = outranking.Value.Item1 - outranking.Value.Item2;
                job.Utility = netValue;
                return job;
            }).ToList();

            return this;
        }

        public List<JobToProceed> GetJobsWithCalculatedUtility()
        {
            var minUtility = _jobs.Min(j => j.Utility);
            var maxUtility = _jobs.Max(j => j.Utility) + Math.Abs(minUtility < 0 ? minUtility : 0);

            _jobs.ForEach(job =>
            {
                job.Utility = (job.Utility + Math.Abs(minUtility < 0 ? minUtility : 0)) / maxUtility;
            });

            return _jobs;
        }
    }
}
