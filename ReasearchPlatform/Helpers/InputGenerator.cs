﻿using ResearchPlatform.Input;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchPlatform.Helpers
{
    public class InputGenerator
    {
        private static readonly int AMOUNT_OF_JOBS_TO_GENERATE = 100;
        private static readonly double MIN_PRISE_FOR_KM_FOR_JOB = 3.15;
        private static readonly double MAX_PRISE_FOR_KM_FOR_JOB = 6.75;
        private static readonly int MIN_LOADING_TIME = 30;
        private static readonly int MAX_LOADING_TIME = 91;
        private static readonly int MAX_WORKING_TIME = 780;
        private static readonly int MARGIN_FOR_LOADING_TIME = 45;
        private static readonly int AMOUNT_OF_CLIENTS = 150;

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

            var distances = await GetAllDistancesForAsync(centralNode, nodesAround);

            Input = new Models.Input() {
                Base = centralNode,
                Nodes = nodesAround,
                DistanceMatrix = distances,
                Jobs = new List<Job>(),
                Logs = new List<string>()
            };

            return this;
        }

        private InputGenerator GenerateJobs()
        {
            for (int i = 1; i <= AMOUNT_OF_JOBS_TO_GENERATE; i++)
            {
                var from = GetRandomNode();
                var to = GetRandomNode(from);

                var distance = Input.DistanceMatrix.Find(distance => (distance.From.Equals(from) && distance.To.Equals(to)) 
                    || (distance.To.Equals(from) && distance.From.Equals(to)));

                if (distance == null)
                {
                    Input.Logs.Add($"Error with distance from {from.ID} to {to.ID}");
                    continue;
                }

                var loadingTime = _random.Next(MIN_LOADING_TIME, MAX_LOADING_TIME);
                var pickupStart = _random.Next(0, MAX_WORKING_TIME);
                var pickupEnd = pickupStart + loadingTime + MARGIN_FOR_LOADING_TIME;
                var deliveryStart = (int) (pickupStart + distance.DurationInSeconds / 60);

                Input.Jobs.Add(new Job()
                {
                    ID = i,
                    From = from,
                    To = to,
                    Price = (distance.DistanceInMeters / 1000) * (MIN_PRISE_FOR_KM_FOR_JOB + _random.NextDouble() * MAX_PRISE_FOR_KM_FOR_JOB),
                    LoadingTime = loadingTime,
                    Pickup = Tuple.Create(pickupStart, pickupEnd),
                    Delivery = Tuple.Create(deliveryStart, (int)(pickupEnd + distance.DurationInSeconds / 60)),
                    ClientId = _random.Next(1, AMOUNT_OF_CLIENTS)
                });
            }

            return this;
        }

        public async Task GenerateAsync(string postcode)
        {
            await GenerateInputDataAsync(postcode);
            GenerateJobs();
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

        private async Task<List<Distance>> GetAllDistancesForAsync(Node centralNode, List<Node> nodesAround)
        {
            List<Distance> distances = new List<Distance>();

            for (int i=0; i < nodesAround.Count; i++)
            {
                for (int j=i+1; j < nodesAround.Count; j++)
                {
                    var distance = await Fetcher.FetchDistanceBetweenNodesAsync(nodesAround[i], nodesAround[j]);
                    while (distance == null)
                        distance = await Fetcher.FetchDistanceBetweenNodesAsync(nodesAround[i], nodesAround[j]);
                    distances.Add(distance);
                }
            }

            nodesAround.ForEach(async node => { 
                var distance = await Fetcher.FetchDistanceBetweenNodesAsync(centralNode, node);
                while (distance == null)
                    distance = await Fetcher.FetchDistanceBetweenNodesAsync(centralNode, node);
                distances.Add(distance);
            });

            return distances;
        }
    }
}
