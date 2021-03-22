using ResearchPlatform.Models;
using System.Collections.Generic;

namespace ResearchPlatform.Helpers
{
    public interface IBranchAndBoundHelper
    {
        public bool AreAllConstrintsSatisfied(List<byte> x, List<byte> d, List<byte> z);
        public double CalculateValueOfGoalFunction(List<byte> x, List<byte> d, List<byte> z);
    }
}