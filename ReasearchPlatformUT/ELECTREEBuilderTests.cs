using ResearchPlatform.Algorithms;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using Xunit;
using static ResearchPlatform.Algorithms.ELECTREEBuilder;

namespace ReasearchPlatformUT
{
    public class ELECTREEBuilderTests
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

        private static readonly List<JobWithCriteria> jobsWithCriteria = new List<JobWithCriteria>() {
            new JobWithCriteria(){ Job = jobs[0], ComfortOfWork = 3, TimeOfExecution = -230, Profit = 500, PossibilityOfNextJobs = 20, Reliability = 3},
            new JobWithCriteria(){ Job = jobs[1], ComfortOfWork = 2, TimeOfExecution = -430, Profit = 400, PossibilityOfNextJobs = 10, Reliability = 2},
            new JobWithCriteria(){ Job = jobs[2], ComfortOfWork = 1, TimeOfExecution = -530, Profit = 300, PossibilityOfNextJobs = 5, Reliability = 1},
        };

        private static readonly List<Tuple<JobWithCriteria, List<double>>> expectedCorcondanceMatrix = new List<Tuple<JobWithCriteria, List<double>>>() { 
            Tuple.Create(jobsWithCriteria[0], new List<double>(){ 1, 1, 1}), 
            Tuple.Create(jobsWithCriteria[1], new List<double>(){ 0.675, 1, 1}), 
            Tuple.Create(jobsWithCriteria[2], new List<double>(){ 0.48, 0.675, 1}),
        };

        private static readonly List<Tuple<JobWithCriteria, List<double>>> expectedReliabilityMatrix = new List<Tuple<JobWithCriteria, List<double>>>() {
            Tuple.Create(jobsWithCriteria[0], new List<double>(){ 1, 1, 1}),
            Tuple.Create(jobsWithCriteria[1], new List<double>(){ 0, 1, 1}),
            Tuple.Create(jobsWithCriteria[2], new List<double>(){ 0, 0.675, 1}),
        };

        private static readonly List<JobToProceed> jobsWithUtility = new List<JobToProceed>() {
            new JobToProceed(){ ID = 1, ComfortOfWork = 3, TimeOfExecution = 230, Profit = 500, PossibilityOfNextJobs = 20, Reliability = 3, Utility = 1},
            new JobToProceed(){ ID = 2, ComfortOfWork = 2, TimeOfExecution = 430, Profit = 400, PossibilityOfNextJobs = 10, Reliability = 2, Utility = 0.67},
            new JobToProceed(){ ID = 3, ComfortOfWork = 1, TimeOfExecution = 530, Profit = 300, PossibilityOfNextJobs = 5, Reliability = 1, Utility = 0.33},
        };

        [Fact]
        public void CalculateCorcondanceMatrix()
        {
            var electree = new ELECTREEBuilder(weights, jobs);
            electree.CalculateCorcondanceMatrix();

            Assert.Equal(expectedCorcondanceMatrix, electree._corcondanceMatrix);
        }

        [Fact]
        public void CalculateReliabilityMatrix()
        {
            var electree = new ELECTREEBuilder(weights, jobs);
            electree.CalculateCorcondanceMatrix()
                    .CalculateReliabilityMatrix();

            Assert.Equal(expectedReliabilityMatrix, electree._reliabilityMatrix);
        }

        [Fact]
        public void CreateOrderByDestilations()
        {
            var electree = new ELECTREEBuilder(weights, jobs);
            electree.CalculateCorcondanceMatrix()
                    .CalculateReliabilityMatrix()
                    .CreateOrderByDestilations();

            Assert.Equal(jobs, electree._sortedJobs);
        }

        [Fact]
        public void GetJobsWithCalculatedUtility()
        {
            var electree = new ELECTREEBuilder(weights, jobs);
            electree.CalculateCorcondanceMatrix()
                    .CalculateReliabilityMatrix()
                    .CreateOrderByDestilations();

            var roundedRes = electree.GetJobsWithCalculatedUtility();
            roundedRes.ForEach(j => j.Utility = Math.Round(j.Utility, 2));

            Assert.Equal(jobsWithUtility, roundedRes);
        }
    }
}
