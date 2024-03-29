﻿using ResearchPlatform.Input;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResearchPlatform.Helpers
{
    public class InputGenerator
    {
        private static readonly int AMOUNT_OF_JOBS_TO_GENERATE = 100;
        private static readonly double MIN_PRISE_FOR_KM_FOR_JOB = 5.15;
        private static readonly double MAX_PRISE_FOR_KM_FOR_JOB = 9.75;
        private static readonly int MIN_LOADING_TIME = 30;
        private static readonly int MAX_LOADING_TIME = 91;
        private static readonly int MAX_WORKING_TIME = 780;
        private static readonly int MARGIN_FOR_LOADING_TIME = 45;
        private static readonly int AMOUNT_OF_CLIENTS = 150;
        private static readonly int MAX_NUMBER_OF_REPEAT_CALLS = 10;
        private static readonly int MAX_LOGS_COUNT = 10;

        private static InputGenerator _instance;
        private static Random _random;
        public Models.Input Input { get; private set; }
        public static InputGenerator GetInstance()
        {
            if (_instance == null)
            {
                _instance = new InputGenerator();
                _random = new Random();
            }

            return _instance;
        }

        private async Task<InputGenerator> GenerateInputDataAsync(string postcode)
        {
            var centralNode = await Fetcher.FetchCityNodeFromPostcodeAsync(postcode);
            if (centralNode == null)
                return null;

            var nodesAround = await Fetcher.FetchCitiesNodesAroundNodeAsync(centralNode);
            if (nodesAround == null)
                return null;

            var logs = new List<string>();

            var distances = await GetAllDistancesForAsync(centralNode, nodesAround, logs);

            Input = new Models.Input() {
                Base = centralNode,
                Nodes = nodesAround,
                DistanceMatrix = distances,
                Jobs = new List<Job>(),
                Logs = logs,
                Clients = new List<Client>()
            };

            return this;
        }

        public InputGenerator GenerateJobs(Models.Input input, int numberOfJobs)
        {
            Input = input;
            var maxPossibleDistance = Input.DistanceMatrix.Select(d => d.DistanceInMeters).Max();
            var currentJobs = input.Jobs.Count == 0 ? 1 : input.Jobs.Count + 1;
            for (int i = currentJobs; i < currentJobs + numberOfJobs; i++)
            {
                var from = GetRandomNode();
                var to = GetRandomNode(from);

                var distance = Input.DistanceMatrix.Find(distance => (distance.From.Equals(from) && distance.To.Equals(to)) 
                    || (distance.To.Equals(from) && distance.From.Equals(to)));

                if (distance == null)
                {
                    Input.Logs.Add($"Error with distance from {from.ID} to {to.ID} - NULL in generating jobs");
                    continue;
                }

                var loadingTime = _random.Next(MIN_LOADING_TIME, MAX_LOADING_TIME);
                var pickupStart = _random.Next(0, MAX_WORKING_TIME);
                var pickupEnd = pickupStart + loadingTime + MARGIN_FOR_LOADING_TIME;
                var deliveryStart = (int) (pickupStart + distance.DurationInSeconds / 60);

                var _randomRisk = _random.NextDouble();
                var _randomTypeOfLoading = _random.NextDouble();

                input.Jobs.Add(new Job()
                {
                    ID = i,
                    From = from,
                    To = to,
                    Price = ((distance.DistanceInMeters + maxPossibleDistance) / 1000) * (MIN_PRISE_FOR_KM_FOR_JOB + _random.NextDouble() * MAX_PRISE_FOR_KM_FOR_JOB),
                    LoadingTime = loadingTime,
                    Pickup = Tuple.Create(pickupStart, pickupEnd),
                    Delivery = Tuple.Create(deliveryStart, (int)(pickupEnd + distance.DurationInSeconds / 60)),
                    ClientId = _random.Next(1, AMOUNT_OF_CLIENTS),
                    TypeOfLoading = _randomTypeOfLoading < 0.25 ? 1 : _randomTypeOfLoading < 0.52 ? 2 : 3,
                    SeizureRisk = _randomRisk < 0.05 ? 1 : _randomRisk < 0.32 ? 2 : 3 // a'la Gausse
                });
            }

            return this;
        }

        public async System.Threading.Tasks.Task GenerateAsync(string postcode, int amountOfJobsToGenerate)
        {
            await GenerateInputDataAsync(postcode);
            GenerateJobs(Input, amountOfJobsToGenerate);
            GenerateClientsWithOpinions();
            var missingDistances = await GetAllDistancesForAsync(Input.Base, Input.Nodes, Input.DistanceMatrix, Input.Logs);
            Input.DistanceMatrix = Input.DistanceMatrix.Concat(missingDistances).ToList();
        }

        private Node GetRandomNode(Node node = null)
        {
            var randomIdx = _random.Next(0, Input.Nodes.Count + 1);

            Node randomNode = GetNodeByIdx(randomIdx);

            if (node == null || (node != null && !node.Equals(randomNode)))
                return randomNode;
            else
            {
                while (node.Equals(randomNode))
                {
                    randomIdx = _random.Next(0, Input.Nodes.Count + 1);
                    randomNode = GetNodeByIdx(randomIdx);
                }

                return randomNode;
            }
        }

        private Node GetNodeByIdx(int idx)
        {
            if (idx == Input.Nodes.Count)
                return Input.Base;
            else
                return Input.Nodes[idx];
        }

        public async Task<List<Distance>> GetAllDistancesForAsync(Node centralNode, List<Node> nodesAround, List<string> logs)
        {
            List<Distance> distances = new List<Distance>();

            for (int i=0; i < nodesAround.Count; i++)
            {
                for (int j=i+1; j < nodesAround.Count; j++)
                {
                    var distance = await Fetcher.FetchDistanceBetweenNodesAsync(nodesAround[i], nodesAround[j]);
                    var counter = 0;
                    while (distance == null && counter < MAX_NUMBER_OF_REPEAT_CALLS)
                    {
                        distance = await Fetcher.FetchDistanceBetweenNodesAsync(nodesAround[i], nodesAround[j]);
                        counter++;
                    }

                    if (distance == null || distance.DistanceInMeters == 0)
                    {
                        logs.Add($"Error with distance from {nodesAround[i].ID} to {nodesAround[j].ID} - {(distance == null ? "NULL" : "0")}");
                    }
                    else
                    {
                        distances.Add(distance);
                    }

                    if (logs.Count > MAX_LOGS_COUNT)
                        break;
                }
            }

            for (int i=0; i < nodesAround.Count; i++)
            {
                var distance = await Fetcher.FetchDistanceBetweenNodesAsync(centralNode, nodesAround[i]);
                var counter = 0;
                while (distance == null && counter < MAX_NUMBER_OF_REPEAT_CALLS)
                {
                    distance = await Fetcher.FetchDistanceBetweenNodesAsync(centralNode, nodesAround[i]);
                    counter++;
                }

                if (distance == null || distance.DistanceInMeters == 0)
                {
                    logs.Add($"Error with distance from {centralNode.ID} to {nodesAround[i].ID} - {(distance == null ? "NULL" : "0")}");
                }
                else
                {
                    distances.Add(distance);
                }

                if (logs.Count > MAX_LOGS_COUNT)
                    break;
            }

            return distances;
        }

        public async Task<List<Distance>> GetAllDistancesForAsync(Node centralNode, List<Node> nodesAround, List<Distance> fetchedDistances, List<string> logs)
        {
            List<Distance> distances = new List<Distance>();
            var distancesToCalc = GetAllDistancesToFetch(centralNode, nodesAround, fetchedDistances);

            foreach (var distance in distancesToCalc)
            {
                var calculatedDistance = await Fetcher.FetchDistanceBetweenNodesAsync(distance.From, distance.To);
                var counter = 0;
                while (calculatedDistance == null && counter < MAX_NUMBER_OF_REPEAT_CALLS)
                {
                    Thread.Sleep(5000);
                    calculatedDistance = await Fetcher.FetchDistanceBetweenNodesAsync(distance.From, distance.To);
                    counter++;
                }

                if (calculatedDistance == null || calculatedDistance.DistanceInMeters == 0)
                {
                    logs.Add($"Error with distance from {distance.From.ID} to {distance.To.ID} - {(distance == null ? "NULL" : "0")}");
                } else
                {
                    distances.Add(calculatedDistance);
                }

                if (logs.Count > MAX_LOGS_COUNT)
                    break;
            }

             return distances;
        }

        private List<Distance> GetAllDistancesToFetch(Node centralNode, List<Node> nodesAround, List<Distance> distances)
        {
            var allDistances = new List<Distance>();
            for (int i = 0; i < nodesAround.Count; i++)
            {
                for (int j = i + 1; j < nodesAround.Count; j++)
                {
                    allDistances.Add(new Distance() { From = nodesAround[i], To = nodesAround[j] });
                }
            }

            nodesAround.ForEach(node => allDistances.Add(new Distance() { From = centralNode, To = node }));
            return allDistances.Where(dist => !distances.Contains(dist)).ToList();
        }

        public void GenerateClientsWithOpinions()
        {
            var _random = new Random();

            var allClientsId = Input.Jobs
                                        .Select(job => job.ClientId)
                                        .Distinct()
                                        .ToList();
            var amountOfAllOpinions = allClientsId.Count * 1.5;

            var opinions = new Dictionary<int /* client id */, double /* CSAT */>();
            var sumOfAllOpinions = 0;

            for (int i = 0; i < amountOfAllOpinions; i++)
            {
                var clientIdx = _random.Next(0, allClientsId.Count);
                var actualSumOfOpinions = 0.0;
                opinions.TryGetValue(allClientsId[clientIdx], out actualSumOfOpinions);
                var opinionToAdd = _random.Next(1, 6);
                if (actualSumOfOpinions == 0.0)
                    opinions.Add(allClientsId[clientIdx], actualSumOfOpinions + opinionToAdd);
                else
                    opinions[allClientsId[clientIdx]] = actualSumOfOpinions + opinionToAdd;
                sumOfAllOpinions += opinionToAdd;
            }

            var keys = new List<int>(opinions.Keys);

            foreach (var key in keys)
            {
                opinions[key] = (opinions[key] / sumOfAllOpinions) * 100;
            }

            foreach (var entry in opinions)
            {
                var jobsWithThisClient = Input.Jobs.Where(job => job.ClientId == entry.Key).ToList();
                jobsWithThisClient.ForEach(job => job.ClientOpinion = entry.Value);
            }

            // end of generating opinions

            // only 10% are own clients
            var ownClientsAmount = allClientsId.Count * 0.1;

            while (ownClientsAmount > 0)
            {
                var randomClientId = allClientsId[_random.Next(0, allClientsId.Count)];
                if (Input.Clients.Where(c => c.ClientID == randomClientId).Count() == 0)
                {
                    Input.Clients.Add(new Client() { ClientID = randomClientId, AmountOfDoneJobs = _random.Next(1, 36) });
                    ownClientsAmount--;
                }
            }

            // end of generating own clients
        }
    }
}
