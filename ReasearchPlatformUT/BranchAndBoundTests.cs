using Moq;
using ResearchPlatform.Algorithms;
using ResearchPlatform.Helpers;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReasearchPlatformUT
{
    public class BranchAndBoundTests
    {
        private Node _base;

        // declaration of nodes
        private Node _Walbrzych;
        private Node _Swiebo;
        private Node _Swidnica;
        private Node _Marciszow;

        private Dictionary<string, Distance> _distances;
        private readonly JobToProceed _fakeJob;

        public BranchAndBoundTests()
        {
            PrepareNodes();
            PrepareDistances();

            _fakeJob = new JobToProceed()
            {
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

            distanceManagerMock.Setup(manager => manager.GetDistanceBetween(_base, _base))
                .Returns(GetDistance(_base, _base));

            var res = bAndB.Run(SearchTreeAlgorithm.DFS);
            
            Assert.NotEmpty(res);
            Assert.Equal(new List<JobToProceed> { _fakeJob }, res);
            Assert.Equal(2, distanceManagerMock.Invocations.Count);
            Assert.Equal(3, bAndBHelperMock.Invocations.Count);
        }

        [Fact]
        public void RunDFSWithOneJobWhichShouldBeChosen()
        {
            var distanceManagerMock = new Mock<IDistancesManager>();
            var bAndBHelperMock = new Mock<IBranchAndBoundHelper>();
            var jobsToProceed = new List<JobToProceed>() { 
                new JobToProceed(){ From = _Walbrzych, To = _Swiebo, Pickup = Tuple.Create(60, 100), Delivery = Tuple.Create(90, 120), LoadingTime = 20, Utility = 30, Price = 2000},
            };

            var bAndB = new BranchAndBound(_base, distanceManagerMock.Object,
                jobsToProceed, bAndBHelperMock.Object);

            bAndBHelperMock.Setup(helper => helper.AreAllConstraintsSatisfied(_base, It.IsNotNull<JobToProceed>(),
                It.Is<List<JobToProceed>>(list => list.Count == 0), 0, 0, 0)).Returns(true);

            bAndBHelperMock.Setup(helper => helper.AreAllConstraintsSatisfied(_base, jobsToProceed[0],
                It.Is<List<JobToProceed>>(list => list.Count != 0), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            SetupDistanceManagerMock(distanceManagerMock, new List<Tuple<Node, Node>>() {
                Tuple.Create(_base, _base),
                Tuple.Create(_base, _Walbrzych),
            }); ;

            distanceManagerMock.Setup(manager => manager.GetDistanceBetween(_Walbrzych, _Swiebo)).Returns(new Distance()
            {
                From = _Walbrzych,
                To = _Swiebo,
                Costs = 60,
                DistanceInMeters = 9900,
                DurationInSeconds = 16 * 60
            });

            var res = bAndB.Run(SearchTreeAlgorithm.DFS);

            Assert.NotEmpty(res);
            Assert.Equal(new List<JobToProceed> { _fakeJob, jobsToProceed[0]}, res);
        }

        [Fact]
        public void RunDFSAllJobsShouldBeChosen()
        {
            var distanceManagerMock = new Mock<IDistancesManager>();
            var bAndBHelperMock = new Mock<IBranchAndBoundHelper>();
            var jobsToProceed = new List<JobToProceed>() {
                new JobToProceed(){ 
                    From = _Walbrzych, To = _Swiebo, 
                    Pickup = Tuple.Create(60, 100), 
                    Delivery = Tuple.Create(90, 120), 
                    LoadingTime = 20, Utility = 30, Price = 2000},
                new JobToProceed(){ 
                    From = _Swidnica, To = _Marciszow, 
                    Pickup = Tuple.Create(150, 210), 
                    Delivery = Tuple.Create(210, 270), 
                    LoadingTime = 50, Utility = 20, Price = 4000},
            };

            var bAndB = new BranchAndBound(_base, distanceManagerMock.Object,
                jobsToProceed, bAndBHelperMock.Object);

            bAndBHelperMock.Setup(helper => helper.AreAllConstraintsSatisfied(It.IsAny<Node>(), It.IsNotNull<JobToProceed>(),
                It.IsAny<List<JobToProceed>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(true);

            bAndBHelperMock.Setup(helper => helper.CalculateValueOfGoalFunction(It.IsAny<Node>(), 
                jobsToProceed.Take(1).ToList(), It.IsAny<int>()))
                    .Returns(2000.0);

            bAndBHelperMock.Setup(helper => helper.CalculateValueOfGoalFunction(It.IsAny<Node>(), new List<JobToProceed>() { _fakeJob, jobsToProceed[0], jobsToProceed[1] }, It.IsAny<int>()))
                .Returns(6000.0);

            bAndBHelperMock.Setup(helper => helper.CalculateValueOfGoalFunction(It.IsAny<Node>(), new List<JobToProceed>() { _fakeJob, jobsToProceed[1] }, It.IsAny<int>()))
                .Returns(4000.0);

            bAndBHelperMock.Setup(helper => helper.CalculateValueOfGoalFunction(It.IsAny<Node>(), new List<JobToProceed>() { _fakeJob, jobsToProceed[1], jobsToProceed[0] }, It.IsAny<int>()))
                .Returns(5000.0);

            SetupDistanceManagerMock(distanceManagerMock, new List<Tuple<Node, Node>>() {
                Tuple.Create(_base, _base),
                Tuple.Create(_base, _Walbrzych),
                Tuple.Create(_Walbrzych, _Swiebo),
                Tuple.Create(_Swiebo, _Swidnica),
                Tuple.Create(_base, _Swidnica),
                Tuple.Create(_Swidnica, _Marciszow),
                Tuple.Create(_Marciszow, _Walbrzych),
            });

            var res = bAndB.Run(SearchTreeAlgorithm.DFS);

            Assert.NotEmpty(res);
            Assert.Equal(3, res.Count);
            Assert.Equal(new List<JobToProceed> { _fakeJob, jobsToProceed[0], jobsToProceed[1]}, res);
        }

        private void SetupDistanceManagerMock(Mock<IDistancesManager> distanceManagerMock, List<Tuple<Node, Node>> pairs)
        {
            foreach (var pair in pairs)
            {
                distanceManagerMock.Setup(manager => manager.GetDistanceBetween(pair.Item1, pair.Item2))
                    .Returns(GetDistance(pair.Item1, pair.Item2));
            }
        }

        private Distance GetDistance(Node from, Node to)
        {
            return _distances.ContainsKey($"{from.ID}_{to.ID}") ? _distances.GetValueOrDefault($"{from.ID}_{to.ID}") 
                : _distances.GetValueOrDefault($"{to.ID}_{from.ID}");
        }

        private void PrepareDistances()
        {
            _distances = new Dictionary<string, Distance>() {
                 { $"{_base.ID}_{_base.ID}", new Distance()
                    {
                        From = _base,
                        To = _base,
                    }
                },
                { $"{_base.ID}_{_Walbrzych.ID}", new Distance()
                    {
                        From = _base,
                        To = _Walbrzych,
                        Costs = 100,
                        DistanceInMeters = 27000,
                        DurationInSeconds = 36 * 60
                    } 
                },
                { $"{_Walbrzych.ID}_{_Swiebo.ID}", new Distance()
                    {
                        From = _Walbrzych,
                        To = _Swiebo,
                        Costs = 60,
                        DistanceInMeters = 9900,
                        DurationInSeconds = 16 * 60
                    }
                },
                { $"{_Swiebo.ID}_{_Swidnica.ID}", new Distance()
                    {
                        From = _Swiebo,
                        To = _Swidnica,
                        Costs = 90,
                        DistanceInMeters = 13000,
                        DurationInSeconds = 17 * 60
                    }
                },
                { $"{_Swidnica.ID}_{_Marciszow.ID}", new Distance()
                    {
                        From = _Swidnica,
                        To = _Marciszow,
                        Costs = 150,
                        DistanceInMeters = 43000,
                        DurationInSeconds = 43 * 60
                    }
                },
                { $"{_base.ID}_{_Swidnica.ID}", new Distance()
                    {
                        From = _base,
                        To = _Swidnica,
                        Costs = 200,
                        DistanceInMeters = 41000,
                        DurationInSeconds = 50 * 60
                    }
                },
                { $"{_Marciszow.ID}_{_Walbrzych.ID}", new Distance()
                    {
                        From = _Marciszow,
                        To = _Walbrzych,
                        Costs = 150,
                        DistanceInMeters = 29000,
                        DurationInSeconds = 38 * 60
                    }
                },
            };
        }

        private void PrepareNodes()
        {
            _base = new Node()
            {
                Name = "Kamienna Góra"
            };

            _Walbrzych = new Node()
            {
                ID = 1,
                Name = "Wa³brzych"
            };

            _Swiebo = new Node()
            {
                ID = 2,
                Name = "Œwiebodzice"
            };

            _Swidnica = new Node()
            {
                ID = 3,
                Name = "Œwidnica"
            };

            _Marciszow = new Node()
            {
                ID = 6,
                Name = "Marciszów"
            };
        }
    }
}
