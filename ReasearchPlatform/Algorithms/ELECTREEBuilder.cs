using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResearchPlatform.Algorithms
{
    public class ELECTREEBuilder : ICriteriaAlgorithmBuilder
    {
        private List<double> _weights;
        private List<double> _q = new List<double>() { 0, 30, 150, 2, 0 };
        private List<double> _p = new List<double>() { 2, 60, 400, 4, 2 };
        private List<double> _v = new List<double>() { 3, 180, 700, 6, 3};

        private List<JobToProceed> _jobs;
        public List<JobToProceed> _sortedJobs;
        public List<JobWithCriteria> _decisionMatrix;
        public List<Tuple<JobWithCriteria, List<double>>> _corcondanceMatrix;
        public List<Tuple<JobWithCriteria, List<double>>> _reliabilityMatrix;


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

        public ELECTREEBuilder(List<double> weights, List<JobToProceed> jobsToProceed)
        {
            _weights = weights;
            _jobs = jobsToProceed;
            _decisionMatrix = _jobs.Select(j => new JobWithCriteria()
            {
                Job = j,
                Profit = j.Profit,
                PossibilityOfNextJobs = j.PossibilityOfNextJobs,
                ComfortOfWork = j.ComfortOfWork,
                Reliability = j.Reliability,
                TimeOfExecution = -j.TimeOfExecution
            }).ToList();

            _corcondanceMatrix = new List<Tuple<JobWithCriteria, List<double>>>();
            _reliabilityMatrix = new List<Tuple<JobWithCriteria, List<double>>>();
            _sortedJobs = new List<JobToProceed>();
        }

        public List<JobToProceed> GetJobsWithCalculatedUtility()
        {
            for (var idx = 0; idx < _sortedJobs.Count; idx++)
            {
                _sortedJobs[idx].Utility = _sortedJobs.Count - idx;
            }

            var maxUtility = _sortedJobs.Max(j => j.Utility);
            var jobsWithUtility = new List<JobToProceed>();

            _sortedJobs.ForEach(job =>
            {
                var withUtility = new JobToProceed(job)
                {
                    Utility = job.Utility / maxUtility
                };
                jobsWithUtility.Add(withUtility);
            });

            return jobsWithUtility;
        }

        public void Run()
        {
            this.CalculateCorcondanceMatrix()
                .CalculateReliabilityMatrix()
                .CreateOrderByDestilations();
        }

        // ------------------ ELECTRE III ------------------- \\

        public ELECTREEBuilder CalculateCorcondanceMatrix()
        {
            for (var row = 0; row < _decisionMatrix.Count; row++)
            {
                var left = _decisionMatrix[row];
                var concordances = new List<double>();

                for (var compare = 0; compare < _decisionMatrix.Count; compare++)
                {
                    var right = _decisionMatrix[compare];
                    var profit = CalculateCorcondanceRatio(left.Profit, right.Profit, _q[(int) Criteria.Profit], 
                        _p[(int)Criteria.Profit], _v[(int)Criteria.Profit]);
                    var possibility = CalculateCorcondanceRatio(left.PossibilityOfNextJobs, right.PossibilityOfNextJobs, _q[(int) Criteria.CompletedJobs], 
                        _p[(int)Criteria.CompletedJobs], _v[(int)Criteria.CompletedJobs]);
                    var comfort = CalculateCorcondanceRatio(left.ComfortOfWork, right.ComfortOfWork, _q[(int) Criteria.ComfortOfWork], 
                        _p[(int)Criteria.ComfortOfWork], _v[(int)Criteria.ComfortOfWork]);
                    var reliability = CalculateCorcondanceRatio(left.Reliability, right.Reliability, _q[(int) Criteria.CustomerReliability], 
                        _p[(int)Criteria.CustomerReliability], _v[(int)Criteria.CustomerReliability]);
                    var time = CalculateCorcondanceRatio(left.TimeOfExecution, right.TimeOfExecution, _q[(int) Criteria.DrivingTime], 
                        _p[(int)Criteria.DrivingTime], _v[(int)Criteria.DrivingTime]);

                    var ratio = _weights[(int)Criteria.Profit] * profit +
                        _weights[(int)Criteria.CompletedJobs] * possibility +
                        _weights[(int)Criteria.ComfortOfWork] * comfort +
                        _weights[(int)Criteria.CustomerReliability] * reliability +
                        _weights[(int)Criteria.DrivingTime] * time;

                    concordances.Add(ratio);
                }

                _corcondanceMatrix.Add(Tuple.Create(left, concordances));
            }

            return this;
        }

        private double CalculateCorcondanceRatio(double left, double right, double q, double p, double v)
        {
            if ((left + q) >= right)
                return 1;

            if ((left + p) >= right)
                return (left + p - right) / (p - q);

            return 0;
        }

        public ELECTREEBuilder CalculateReliabilityMatrix()
        {
            for (var row = 0; row < _decisionMatrix.Count; row++)
            {
                var left = _decisionMatrix[row];
                var reliabilityRatios = new List<double>();

                for (var compare = 0; compare < _decisionMatrix.Count; compare++)
                {
                    var reliabilitiesForAllCriteria = new List<double>();

                    var right = _decisionMatrix[compare];
                    var profit = CalculateReliabilityRatio(left.Profit, right.Profit, _q[(int)Criteria.Profit],
                        _p[(int)Criteria.Profit], _v[(int)Criteria.Profit]);
                    var possibility = CalculateReliabilityRatio(left.PossibilityOfNextJobs, right.PossibilityOfNextJobs, _q[(int)Criteria.CompletedJobs],
                        _p[(int)Criteria.CompletedJobs], _v[(int)Criteria.CompletedJobs]);
                    var comfort = CalculateReliabilityRatio(left.ComfortOfWork, right.ComfortOfWork, _q[(int)Criteria.ComfortOfWork],
                        _p[(int)Criteria.ComfortOfWork], _v[(int)Criteria.ComfortOfWork]);
                    var reliability = CalculateReliabilityRatio(left.Reliability, right.Reliability, _q[(int)Criteria.CustomerReliability],
                        _p[(int)Criteria.CustomerReliability], _v[(int)Criteria.CustomerReliability]);
                    var time = CalculateReliabilityRatio(left.TimeOfExecution, right.TimeOfExecution, _q[(int)Criteria.DrivingTime],
                        _p[(int)Criteria.DrivingTime], _v[(int)Criteria.DrivingTime]);

                    var concordanceRatio = _corcondanceMatrix[row].Item2[compare];

                    if (profit > concordanceRatio)
                        reliabilitiesForAllCriteria.Add(profit);
                    if (possibility > concordanceRatio)
                        reliabilitiesForAllCriteria.Add(possibility);
                    if (comfort > concordanceRatio)
                        reliabilitiesForAllCriteria.Add(comfort);
                    if (reliability > concordanceRatio)
                        reliabilitiesForAllCriteria.Add(reliability);
                    if (time > concordanceRatio)
                        reliabilitiesForAllCriteria.Add(time);

                    var reliabilityRatio = concordanceRatio;

                    foreach (var rel in reliabilitiesForAllCriteria)
                    {
                        reliabilityRatio *= (1 - rel) / (1 - concordanceRatio);
                    }

                    reliabilityRatios.Add(reliabilityRatio);
                }

                _reliabilityMatrix.Add(Tuple.Create(left, reliabilityRatios));
            }

            return this;
        }

        private double CalculateReliabilityRatio(double left, double right, double q, double p, double v)
        {
            if (right > left + v)
                return 1;

            if (right <= left + p)
                return 0;

            return (right - left - p) / (v - p);
        }

        public ELECTREEBuilder CreateOrderByDestilations()
        {
            var jobsToCompare = Enumerable.Range(0, _reliabilityMatrix.Count).ToList();
            var initialLambda = GetInitialLambdaValue(jobsToCompare);

            DestilationRec(jobsToCompare, initialLambda);

            return this;
        }

        private void DestilationRec(List<int> jobsToCompare, double initialLambda)
        {
            var lambda = GetLambdaValue(jobsToCompare, initialLambda);
            var jobsWhichAreBetterThan = new Dictionary<int, List<int>>(); // for each of job set of jobs which is better

            for (var row = 0; row < _reliabilityMatrix.Count; row++)
            {
                // check only compared jobs
                if (!jobsToCompare.Contains(row))
                    continue;

                jobsWhichAreBetterThan.Add(row, new List<int>());

                for (var compare = 0; compare < _reliabilityMatrix.Count; compare++)
                {
                    // check only compared jobs
                    if (!jobsToCompare.Contains(compare))
                        continue;
                    // check if row > compare
                    var reliabilityRatio = _reliabilityMatrix[row].Item2[compare];
                    var discriminationLevel = CalculateDiscrimationLevel(reliabilityRatio);
                    var reliabilityRatioViceVersa = _reliabilityMatrix[compare].Item2[row];

                    if (((reliabilityRatio - discriminationLevel) > reliabilityRatioViceVersa) && reliabilityRatio > lambda)
                        jobsWhichAreBetterThan.GetValueOrDefault(row).Add(compare);
                }
            }

            var grades = new List<Tuple<int, List<int>>>();

            foreach (var jobWhichAreBetterThan in jobsWhichAreBetterThan)
            {
                var betterThanCount = jobWhichAreBetterThan.Value.Count();
                var worstThanCount = jobsWhichAreBetterThan.Where(j => j.Key != jobWhichAreBetterThan.Key
                    && j.Value.Contains(jobWhichAreBetterThan.Key)).Count();

                grades.Add(Tuple.Create(jobWhichAreBetterThan.Key, 
                    new List<int>() { betterThanCount, worstThanCount, betterThanCount - worstThanCount }));
            }

            var maxGrade = grades.Max(g => g.Item2[2]);
            var bests = grades.Where(g => g.Item2[2] == maxGrade).Select(g => g.Item1).ToList();
            
            if (bests.Count == 1 || lambda == 0)
            {
                bests.ForEach(best => _sortedJobs.Add(_reliabilityMatrix[best].Item1.Job));
            }
            else
            {
                DestilationRec(bests, lambda);
            }

            jobsToCompare = jobsToCompare.Where(j => !bests.Contains(j)).ToList();

            if (jobsToCompare.Count < 2)
            {
                jobsToCompare.ForEach(compare => _sortedJobs.Add(_reliabilityMatrix[compare].Item1.Job));
                return;
            }
            else
            {
                DestilationRec(jobsToCompare, GetInitialLambdaValue(jobsToCompare));
            }
        }

        private double CalculateDiscrimationLevel(double lambda)
        {
            return -0.15 * lambda + 0.3;
        }

        private double GetLambdaValue(List<int> rows, double lambda)
        {
            var onlyToCompareReliabilities = new List<double>();

            foreach (var row in rows)
            {
                var rowReliability = _reliabilityMatrix[row].Item2;

                for (var col = 0; col < rowReliability.Count; col++)
                {
                    if (rows.Contains(col))
                        onlyToCompareReliabilities.Add(rowReliability[col]);
                }
            }

            var difference = lambda - CalculateDiscrimationLevel(lambda);

            if (onlyToCompareReliabilities.Min() > difference)
                return 0;

            return onlyToCompareReliabilities.Where(val => val < difference).Max();
        }

        private double GetInitialLambdaValue(List<int> rows)
        {
            var onlyToCompareReliabilities = new List<double>();

            foreach (var row in rows)
            {
                var rowReliability = _reliabilityMatrix[row].Item2;

                for (var col = 0; col < rowReliability.Count; col++)
                {
                    if (rows.Contains(col))
                        onlyToCompareReliabilities.Add(rowReliability[col]);
                }
            }

            return onlyToCompareReliabilities.Max();
        }
    }
}
