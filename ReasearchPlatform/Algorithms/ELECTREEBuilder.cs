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
        public List<JobWithCriteria> _decisionMatrix;
        public List<Tuple<JobWithCriteria, List<double>>> _corcondanceMatrix;
        public List<Tuple<JobWithCriteria, List<double>>> _reliabilityMatrix;


        public List<Tuple<JobWithCriteria, List<double>>> _corcondanceIntervalMatrix;
        public List<Tuple<JobWithCriteria, List<double>>> _discordanceIntervalMatrix;
        public List<Tuple<JobWithCriteria, List<double>>> _corcondanceIndexMatrix;
        public List<Tuple<JobWithCriteria, List<double>>> _discordanceIndexMatrix;
        public List<Tuple<JobWithCriteria, List<double>>> _netValues;

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
                TimeOfExecution = j.TimeOfExecution
            }).ToList();
            _corcondanceIntervalMatrix = new List<Tuple<JobWithCriteria, List<double>>>();
            _discordanceIntervalMatrix = new List<Tuple<JobWithCriteria, List<double>>>();
            _corcondanceIndexMatrix = new List<Tuple<JobWithCriteria, List<double>>>();
            _discordanceIndexMatrix = new List<Tuple<JobWithCriteria, List<double>>>();



            _corcondanceMatrix = new List<Tuple<JobWithCriteria, List<double>>>();
            _reliabilityMatrix = new List<Tuple<JobWithCriteria, List<double>>>();
        }

        public List<JobToProceed> GetJobsWithCalculatedUtility()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            this.CalculateCorcondanceMatrix()
                .CalculateReliabilityMatrix();
                //.CreateOrderByDestilations();
            //this.NormalizeDecisionMatrix()
            //    .CalculateConcordanceIntervalMatrix()
            //    .CalculateConcordanceIndexMatrix()
            //    .CalculateDiscordanceIntervalMatrix()
            //    .CalculateDiscordanceIndexMatrix()
            //    .CalculateSuperiorAndInferiorValues();
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

            if ((left + q) < right && (left + p) >= right)
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

            return (right - left + p) / (v - p);
        }

        public ELECTREEBuilder CreateOrderByDestilations(List<Tuple<JobWithCriteria, List<double>>> rel)
        {// not working correctly for x3 - is only 1 not 2
            _reliabilityMatrix = rel;
            var lambda = GetLambdaValue(Enumerable.Range(0, _reliabilityMatrix.Count).ToList(), _reliabilityMatrix.Max(row => row.Item2.Max()));
            var jobsWhichAreBetterThan = new Dictionary<int, List<int>>(); // for each of job set of jobs which is better

            for (var row = 0; row < _reliabilityMatrix.Count; row++)
            {
                jobsWhichAreBetterThan.Add(row, new List<int>());

                for (var compare = 0; compare < _reliabilityMatrix.Count; compare++)
                {
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

                grades.Add(Tuple.Create(jobWhichAreBetterThan.Key, new List<int>() { betterThanCount , worstThanCount, betterThanCount - worstThanCount}));
            }

            return this;
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


        // ------------------ ELECTRE ------------------- \\

        private ELECTREEBuilder CalculateSuperiorAndInferiorValues()
        {
            var superiorsByRow = _corcondanceIntervalMatrix.Select(row => row.Item2.Sum()).ToList();
            var superiorsByCol = new List<double>();
            var inferiorsByRow = _discordanceIntervalMatrix.Select(row => row.Item2.Sum()).ToList();
            var inferiorsByCol = new List<double>();

            for (var col = 0; col < _corcondanceIntervalMatrix.Count; col++)
            {
                superiorsByCol.Add(_corcondanceIntervalMatrix.Sum(row => row.Item2[col]));
                inferiorsByCol.Add(_discordanceIntervalMatrix.Sum(row => row.Item2[col]));
            }

            return this;
        }

        private ELECTREEBuilder CalculateDiscordanceIndexMatrix()
        {
            var matrixSize = _discordanceIntervalMatrix.Count;
            var discordanceIndex = _discordanceIntervalMatrix.Sum(cor => cor.Item2.Sum()) * matrixSize * (matrixSize - 1);

            for (var row = 0; row < matrixSize; row++)
            {
                var left = _corcondanceIntervalMatrix[row];
                var results = new List<double>();

                foreach (var interval in left.Item2)
                {
                    results.Add(interval > discordanceIndex ? 0 : 1);
                }

                _discordanceIndexMatrix.Add(Tuple.Create(left.Item1, results));
            }

            return this;
        }

        private ELECTREEBuilder CalculateDiscordanceIntervalMatrix()
        {
            for (var row = 0; row < _decisionMatrix.Count; row++)
            {
                var left = _decisionMatrix[row];
                var results = new List<double>();

                for (var compare = 0; compare < _decisionMatrix.Count; compare++)
                {
                    var discordanceInterval = 0.0;

                    if (row == compare)
                    {
                        results.Add(discordanceInterval);
                        continue;
                    }

                    var right = _decisionMatrix[compare];
                    var allAbsValues = new List<double>();
                    var discordacesValues = new List<double>();

                    allAbsValues.Add(Math.Abs(left.Profit - right.Profit));
                    allAbsValues.Add(Math.Abs(left.PossibilityOfNextJobs - right.PossibilityOfNextJobs));
                    allAbsValues.Add(Math.Abs(left.Reliability - right.Reliability));
                    allAbsValues.Add(Math.Abs(left.TimeOfExecution - right.TimeOfExecution));
                    allAbsValues.Add(Math.Abs(left.ComfortOfWork - right.ComfortOfWork));

                    if (left.Profit < right.Profit)
                        discordacesValues.Add(Math.Abs(left.Profit - right.Profit));

                    if (left.PossibilityOfNextJobs < right.PossibilityOfNextJobs)
                        discordacesValues.Add(Math.Abs(left.PossibilityOfNextJobs - right.PossibilityOfNextJobs));

                    if (left.Reliability < right.Reliability)
                        discordacesValues.Add(Math.Abs(left.Reliability - right.Reliability));

                    if (left.TimeOfExecution < right.TimeOfExecution)
                        discordacesValues.Add(Math.Abs(left.TimeOfExecution - right.TimeOfExecution));

                    if (left.ComfortOfWork < right.ComfortOfWork)
                        discordacesValues.Add(Math.Abs(left.ComfortOfWork - right.ComfortOfWork));

                    var maxDiscordance = discordacesValues.Max();
                    var maxAbs = allAbsValues.Max();
                    discordanceInterval = maxDiscordance / maxAbs;

                    results.Add(discordanceInterval);
                }

                _discordanceIntervalMatrix.Add(Tuple.Create(left, results));
            }

            return this;
        }

        private ELECTREEBuilder CalculateConcordanceIndexMatrix()
        {
            var matrixSize = _corcondanceIntervalMatrix.Count;
            var corcondanceIndex = _corcondanceIntervalMatrix.Sum(cor => cor.Item2.Sum()) * matrixSize * (matrixSize - 1);

            for (var row = 0; row < matrixSize; row++)
            {
                var left = _corcondanceIntervalMatrix[row];
                var results = new List<double>();

                foreach (var interval in left.Item2)
                {
                    results.Add(interval >= corcondanceIndex ? 1 : 0);
                }

                _corcondanceIndexMatrix.Add(Tuple.Create(left.Item1, results));
            }

            return this;
        }

        private ELECTREEBuilder CalculateConcordanceIntervalMatrix()
        {
            for (var row = 0; row < _decisionMatrix.Count; row++)
            {
                var left = _decisionMatrix[row];
                var results = new List<double>();

                for (var compare = 0; compare < _decisionMatrix.Count; compare++)
                {
                    var result = 0.0;

                    if (row == compare)
                    {
                        results.Add(result);
                        continue;
                    }

                    var right = _decisionMatrix[compare];

                    if (left.Profit >= right.Profit)
                        result += _weights[(int) Criteria.Profit];

                    if (left.PossibilityOfNextJobs >= right.PossibilityOfNextJobs)
                        result += _weights[(int)Criteria.CompletedJobs];

                    if (left.Reliability >= right.Reliability)
                        result += _weights[(int)Criteria.CustomerReliability];

                    if (left.TimeOfExecution >= right.TimeOfExecution)
                        result += _weights[(int)Criteria.DrivingTime];

                    if (left.ComfortOfWork >= right.ComfortOfWork)
                        result += _weights[(int)Criteria.ComfortOfWork];

                    results.Add(result);
                }

                _corcondanceIntervalMatrix.Add(Tuple.Create(left, results));
            }

            return this;
        }
        //public ELECTREEBuilder NormalizeDecisionMatrix()
        //{
        //    _decisionMatrix.ForEach(job => {
        //        job.Profit = Math.Pow(job.Profit, 2);
        //        job.PossibilityOfNextJobs = Math.Pow(job.PossibilityOfNextJobs, 2);
        //        job.ComfortOfWork = Math.Pow(job.ComfortOfWork, 2);
        //        job.Reliability = Math.Pow(job.Reliability, 2);
        //        job.TimeOfExecution = Math.Pow(job.TimeOfExecution, 2);
        //    });

        //    var sqrtOfProfit = Math.Sqrt(_decisionMatrix.Sum(job => job.Profit));
        //    var sqrtOfNextJobs = Math.Sqrt(_decisionMatrix.Sum(job => job.PossibilityOfNextJobs));
        //    var sqrtOfComfort = Math.Sqrt(_decisionMatrix.Sum(job => job.ComfortOfWork));
        //    var sqrtOfReliability = Math.Sqrt(_decisionMatrix.Sum(job => job.Reliability));
        //    var sqrtOfTime = Math.Sqrt(_decisionMatrix.Sum(job => job.TimeOfExecution));

        //    _decisionMatrix.ForEach(job => {
        //        job.Profit = job.Profit / sqrtOfProfit * _weights[(int) Criteria.Profit];
        //        job.PossibilityOfNextJobs = job.PossibilityOfNextJobs / sqrtOfNextJobs * _weights[(int)Criteria.CompletedJobs];
        //        job.ComfortOfWork = job.ComfortOfWork / sqrtOfComfort * _weights[(int)Criteria.ComfortOfWork];
        //        job.Reliability = job.Reliability / sqrtOfReliability * _weights[(int)Criteria.CustomerReliability];
        //        job.TimeOfExecution = job.TimeOfExecution / sqrtOfTime * _weights[(int)Criteria.DrivingTime];
        //    });

        //    return this;
        //}

    }
}
