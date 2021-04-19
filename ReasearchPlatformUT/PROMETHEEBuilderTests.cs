using ResearchPlatform.Algorithms;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static ResearchPlatform.Algorithms.PROMETHEEBuilder;

namespace ReasearchPlatformUT
{
    public class PROMETHEEBuilderTests
    {
        private static readonly List<double> weights = new List<double>() {
            0.05, // Criteria.ComfortOfWork 
            0.10, // Criteria.DrivingTime
            0.60, // Criteria.Profit
            0.15, // Criteria.CompletedJobs
            0.10 // Criteria.CustomerReliability
        };

        private static readonly List<JobToProceed> jobs = new List<JobToProceed>() {
            new JobToProceed(){ ID = 1, ComfortOfWork = 3, TimeOfExecution = 230, Profit = 500, PossibilityOfNextJobs = 20, Reliability = 3},
            new JobToProceed(){ ID = 2, ComfortOfWork = 2, TimeOfExecution = 430, Profit = 400, PossibilityOfNextJobs = 10, Reliability = 2},
            new JobToProceed(){ ID = 3, ComfortOfWork = 1, TimeOfExecution = 530, Profit = 300, PossibilityOfNextJobs = 5, Reliability = 1},
        };

        private static readonly List<JobWithCriteria> expectedDecisionMatrix = new List<JobWithCriteria>(){
            new JobWithCriteria() { Job = jobs[0], ComfortOfWork = 1, TimeOfExecution = 1, Profit = 1, PossibilityOfNextJobs = 1, Reliability = 1},
            new JobWithCriteria() { Job = jobs[1], ComfortOfWork = 0.5, TimeOfExecution = 0.33, Profit = 0.5, PossibilityOfNextJobs = 0.33, Reliability = 0.5},
            new JobWithCriteria() { Job = jobs[2], ComfortOfWork = 0, TimeOfExecution = 0, Profit = 0, PossibilityOfNextJobs = 0, Reliability = 0},
        };

        private static readonly Dictionary<JobToProceed, List<JobWithCriteria>> expectedPreferencesMatrix = new Dictionary<JobToProceed, List<JobWithCriteria>>() {
            { 
                jobs[0], new List<JobWithCriteria>() {
                    new JobWithCriteria() { Job = jobs[0], ComfortOfWork = 0, TimeOfExecution = 0, Profit = 0, PossibilityOfNextJobs = 0, Reliability = 0},
                    new JobWithCriteria() { Job = jobs[1], ComfortOfWork = 0.025, TimeOfExecution = 0.067, Profit = 0.3, PossibilityOfNextJobs = 0.1, Reliability = 0.05},
                    new JobWithCriteria() { Job = jobs[2], ComfortOfWork = 0.05, TimeOfExecution = 0.1, Profit = 0.6, PossibilityOfNextJobs = 0.15, Reliability = 0.1}
                } 
            },
            {
                jobs[1], new List<JobWithCriteria>() {
                    new JobWithCriteria() { Job = jobs[0], ComfortOfWork = 0, TimeOfExecution = 0, Profit = 0, PossibilityOfNextJobs = 0, Reliability = 0},
                    new JobWithCriteria() { Job = jobs[1], ComfortOfWork = 0, TimeOfExecution = 0, Profit = 0, PossibilityOfNextJobs = 0, Reliability = 0},
                    new JobWithCriteria() { Job = jobs[2], ComfortOfWork = 0.025, TimeOfExecution = 0.033, Profit = 0.3, PossibilityOfNextJobs = 0.05, Reliability = 0.05}
                }
            },
            {
                jobs[2], new List<JobWithCriteria>() {
                    new JobWithCriteria() { Job = jobs[0], ComfortOfWork = 0, TimeOfExecution = 0, Profit = 0, PossibilityOfNextJobs = 0, Reliability = 0},
                    new JobWithCriteria() { Job = jobs[1], ComfortOfWork = 0, TimeOfExecution = 0, Profit = 0, PossibilityOfNextJobs = 0, Reliability = 0},
                    new JobWithCriteria() { Job = jobs[2], ComfortOfWork = 0, TimeOfExecution = 0, Profit = 0, PossibilityOfNextJobs = 0, Reliability = 0}
                }
            }
        };

        private static readonly List<List<double>> expectedAvgPreferencesMatrix = new List<List<double>>() {
            new List<double>(){0, 0.542, 1},
            new List<double>(){0, 0, 0.458},
            new List<double>(){0, 0, 0},
        };

        private static readonly List<double> expectedLeavingFlows = new List<double>() {
            0.771, 0.229, 0
        };

        private static readonly List<double> expectedEnteringFlows = new List<double>(){
            0, 0.271, 0.729
        };

        private static readonly List<double> expectedNetOutrankingValues = new List<double>(){
            0.771, -0.042, -0.729
        };

        // + 0.729  podzielic na 1.5

        private static readonly List<JobToProceed> expectedJobsWithUtility = new List<JobToProceed>() {
            new JobToProceed(){ ID = 1, ComfortOfWork = 3, TimeOfExecution = 230, Profit = 500, PossibilityOfNextJobs = 20, Reliability = 3, Utility = 1},
            new JobToProceed(){ ID = 2, ComfortOfWork = 2, TimeOfExecution = 430, Profit = 400, PossibilityOfNextJobs = 10, Reliability = 2, Utility = 0.458},
            new JobToProceed(){ ID = 3, ComfortOfWork = 1, TimeOfExecution = 530, Profit = 300, PossibilityOfNextJobs = 5, Reliability = 1, Utility = 0},
        };

        [Fact]
        public void NormalizeDecisionMatrix()
        {
            var promethee = new PROMETHEEBuilder(weights, jobs);
            promethee.NormalizeDecisionMatrix();

            var res = promethee._decisionMatrix;
            res.ForEach(d => {
                d.ComfortOfWork = Math.Round(d.ComfortOfWork, 2);
                d.TimeOfExecution = Math.Round(d.TimeOfExecution, 2);
                d.Profit = Math.Round(d.Profit, 2);
                d.PossibilityOfNextJobs = Math.Round(d.PossibilityOfNextJobs, 2);
                d.Reliability = Math.Round(d.Reliability, 2);
            });

            Assert.Equal(expectedDecisionMatrix, res);
        }

        [Fact]
        public void CalculatePreferences()
        {
            var promethee = new PROMETHEEBuilder(weights, jobs);
            promethee.NormalizeDecisionMatrix()
                     .CalculatePreferences();

            var res = promethee._preferencesMatrix;

            foreach(var r in res)
            {
                r.Value.ForEach(d => {
                    d.ComfortOfWork = Math.Round(d.ComfortOfWork, 3);
                    d.TimeOfExecution = Math.Round(d.TimeOfExecution, 3);
                    d.Profit = Math.Round(d.Profit, 3);
                    d.PossibilityOfNextJobs = Math.Round(d.PossibilityOfNextJobs, 3);
                    d.Reliability = Math.Round(d.Reliability, 3);
                });
            }

            Assert.Equal(expectedPreferencesMatrix, res);
        }

        [Fact]
        public void CalculateAggregatePreferences()
        {
            var promethee = new PROMETHEEBuilder(weights, jobs);
            promethee.NormalizeDecisionMatrix()
                     .CalculatePreferences()
                     .CalculateAggregatePreferences();

            var res = promethee._aggregatePreferences
                        .Select(r => r.Item2.Select(a => Math.Round(a, 3))).ToList();

            Assert.Equal(expectedAvgPreferencesMatrix, res);
        }

        [Fact]
        public void DetermineOutrankingFlows()
        {
            var promethee = new PROMETHEEBuilder(weights, jobs);
            promethee.NormalizeDecisionMatrix()
                     .CalculatePreferences()
                     .CalculateAggregatePreferences()
                     .DetermineOutrankingFlows();

            var leavingFlows = promethee._outrinkingFlows
                        .Select(flow => Math.Round(flow.Value.Item1, 3)).ToList();

            var enteringFlows = promethee._outrinkingFlows
                        .Select(flow => Math.Round(flow.Value.Item2, 3)).ToList();

            Assert.Equal(expectedLeavingFlows, leavingFlows);
            Assert.Equal(expectedEnteringFlows, enteringFlows);
        }

        [Fact]
        public void CalculateNetOutrankingValue()
        {
            var promethee = new PROMETHEEBuilder(weights, jobs);
            promethee.NormalizeDecisionMatrix()
                     .CalculatePreferences()
                     .CalculateAggregatePreferences()
                     .DetermineOutrankingFlows()
                     .CalculateNetOutrankingValue();

            var res = jobs.Select(j => Math.Round(j.Utility, 3)).ToList();

            Assert.Equal(expectedNetOutrankingValues, res);
        }

        [Fact]
        public void GetJobsWithCalculatedUtility()
        {
            var promethee = new PROMETHEEBuilder(weights, jobs);
            promethee.Run();
            var results = promethee.GetJobsWithCalculatedUtility();
            results.ForEach(r => r.Utility = Math.Round(r.Utility, 3));

            Assert.Equal(expectedJobsWithUtility, results);
        }
    }
}
