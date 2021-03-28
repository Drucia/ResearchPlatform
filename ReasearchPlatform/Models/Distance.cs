using ResearchPlatform.Models.DTO;
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

        private static int CalculateAvg(List<List<int>> distances)
        {
            var summedRows = distances
                    .Select(row => row.Sum())
                    .ToList();
            return (int) summedRows.Average();
        }

        private static int CalculateAvg(List<List<double>> distances)
        {
            var summedRows = distances
                    .Select(row => row.Sum())
                    .ToList();
            return (int) summedRows.Average();
        }
    }

    public class DistancesManager
    {
        private List<Distance> _distances;

        public DistancesManager(List<Distance> distances)
        {
            _distances = distances;
        }

        public Distance GetDistanceBetween(Node from, Node to)
        {
            if (from.Equals(to))
                return new Distance() { DistanceInMeters = 0, DurationInSeconds = 0, Costs = 0 };

            return _distances.Find(distance => distance.From.Equals(from) && distance.To.Equals(to) ||
                distance.From.Equals(to) && distance.To.Equals(from));
        }
    }
}