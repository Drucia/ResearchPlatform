using ResearchPlatform.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResearchPlatform.Models
{
    public class Distance
    {
        public Node From { get; set; }
        public Node To { get; set; }
        public double DistanceInMeters { get; set; }
        public double DurationInSeconds { get; set; }
        public double Costs { get; set; }

        public static Distance CreateFromDTO(DistanceDTO distance, Node from, Node to)
        {
            return new Distance()
            {
                From = from,
                To = to,
                DistanceInMeters = CalculateAvg(distance.distances),
                DurationInSeconds = CalculateAvg(distance.times),
                Costs = CalculateAvg(distance.weights)
            };
        }

        private static double CalculateAvg(List<List<int>> distances)
        {
            var summedRows = distances
                    .Select(row => row.Sum())
                    .ToList();
            return summedRows.Average();
        }

        private static double CalculateAvg(List<List<double>> distances)
        {
            var summedRows = distances
                    .Select(row => row.Sum())
                    .ToList();
            return summedRows.Average();
        }
    }
}