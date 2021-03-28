using ResearchPlatform.Algorithms;
using ResearchPlatform.Helpers;
using ResearchPlatform.Models;
using System.Collections.Generic;
using System.Linq;

namespace ResearchPlatform
{
    public static class AlgorithmsManager
    {
        public static bool CheckMatrixConsistency(IEnumerable<IEnumerable<string>> matrix)
        {
            var ahp = new AHPBuilder(matrix);
            ahp.Run();
            return ahp.IsConsistent;
        }

        public static List<Dictionary<SearchTreeAlgorithm, Result>> RunWith(Configuration configuration, Models.Input input)
        {
            var algorithmsMatrix = configuration.AlgorithmsMatrix;
            var distanceManager = new DistancesManager(input.DistanceMatrix);
            var branchAndBoundHelper = new BranchAndBoundHelper(distanceManager);
            var jobsToProceed = input.Jobs.Select(j => new JobToProceed(j)).ToList();

            CriteriaCalculator.CalculateCriteria(
                jobsToProceed, 
                configuration, 
                distanceManager, 
                input.Base,
                input.Clients);

            var algorithmsToRun = new List<Models.Task>
            {
                new Models.Task(
                    new AHPBuilder(configuration.ComparisionMatrix),
                    branchAndBoundHelper,
                    input,
                    new List<bool>(algorithmsMatrix[(int)MultiCriteriaAlgorithm.AHP]),
                    jobsToProceed),

                new Models.Task(
                    new OwnWeightsBuilder(configuration.CriteriaWeights.Select(w => w / 100.0).ToList()),
                    branchAndBoundHelper,
                    input,
                    new List<bool>(algorithmsMatrix[(int)MultiCriteriaAlgorithm.OwnWeights]),
                    jobsToProceed)
            };

            algorithmsToRun.ForEach(alg => alg.Run());

            return algorithmsToRun.Select(alg => alg.GetResults()).ToList();
        }
    }
}
