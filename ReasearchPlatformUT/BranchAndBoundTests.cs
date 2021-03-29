using Moq;
using ResearchPlatform.Algorithms;
using ResearchPlatform.Helpers;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace ReasearchPlatformUT
{
    public class BranchAndBoundTests
    {
        private readonly Node _base;
        private readonly Distance _baseDistance;
        private readonly JobToProceed _fakeJob;

        public BranchAndBoundTests()
        {
            _base = new Node()
            {
                Name = "KG"
            };

            _baseDistance = new Distance()
            {
                From = _base,
                To = _base
            };

            _fakeJob = new JobToProceed()
            {
                IsChosen = true,
                From = _base,
                To = _base,
                Delivery = Tuple.Create(0, 780),
                Pickup = Tuple.Create(0, 780),
            };
        }

        [Fact]
        public void RunDFSWithEmptyListOfJobs()
        {
            var distanceManagerMock = new Mock<IDistancesManager>();
            var bAndBHelperMock = new Mock<IBranchAndBoundHelper>();
            var jobsToProceed = new List<JobToProceed>();

            var bAndB = new BranchAndBound(_base, distanceManagerMock.Object, 
                jobsToProceed, bAndBHelperMock.Object);

            bAndBHelperMock.Setup(helper => helper.AreAllConstraintsSatisfied(_base, It.IsNotNull<JobToProceed>(),
                It.Is<List<JobToProceed>>(list => list.Count == 0), 0, 0, 0)).Returns(true);

            distanceManagerMock.Setup(manager => manager.GetDistanceBetween(_base, _base)).Returns(_baseDistance);

            var res = bAndB.Run(SearchTreeAlgorithm.DFS);
            
            Assert.NotEmpty(res);
            Assert.Equal(res, new List<JobToProceed>{_fakeJob});
            Assert.True(distanceManagerMock.Invocations.Count == 2, $"Invocation count has: {distanceManagerMock.Invocations.Count}");
            Assert.True(bAndBHelperMock.Invocations.Count == 1, $"Invocation count has: {bAndBHelperMock.Invocations.Count}");
        }
    }
}
