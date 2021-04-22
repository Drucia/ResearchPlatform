using ResearchPlatform.Algorithms;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static ResearchPlatform.Algorithms.ELECTREEBuilder;

namespace ReasearchPlatformUT
{
    public class ELECTREEBuilderTests
    {
        [Fact]
        public void NormalizeDecisionMatrix()
        {
            var reliabilityMatrix = new List<Tuple<JobWithCriteria, List<double>>>() {
                Tuple.Create(new JobWithCriteria(){ Job = new JobToProceed(){ ID = 1} }, new List<double>(){ 1, 0.8, 0.55, 0.85, 0.865, 0.7}),
                Tuple.Create(new JobWithCriteria(){ Job = new JobToProceed(){ ID = 2} }, new List<double>(){ 1, 1, 0.55, 0.85, 0.865, 0.7}),
                Tuple.Create(new JobWithCriteria(){ Job = new JobToProceed(){ ID = 3} }, new List<double>(){ 0.7, 0.7, 1, 0.85, 0.781, 0.96}),
                Tuple.Create(new JobWithCriteria(){ Job = new JobToProceed(){ ID = 4} }, new List<double>(){ 0.55, 0.55, 0.25, 1, 0.8, 0.4}),
                Tuple.Create(new JobWithCriteria(){ Job = new JobToProceed(){ ID = 5} }, new List<double>(){ 0.634, 0.55, 0.45, 0.76, 1, 0.45}),
                Tuple.Create(new JobWithCriteria(){ Job = new JobToProceed(){ ID = 6} }, new List<double>(){ 0.59, 0.55, 0.85, 0.7, 0.865, 1}),
            };

            var electree = new ELECTREEBuilder(new List<double>(), new List<JobToProceed>());
            electree.CreateOrderByDestilations(reliabilityMatrix);
        }
    }
}
