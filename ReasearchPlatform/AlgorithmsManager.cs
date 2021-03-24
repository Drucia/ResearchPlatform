using ResearchPlatform.Algorithms;
using ResearchPlatform.Helpers;
using ResearchPlatform.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var branchAndBoundHelper = new BranchAndBoundHelper();
            var algorithmsToRun = new List<Models.Task>
            {
                new Models.Task(
                new AHPBuilder(configuration.ComparisionMatrix),
                branchAndBoundHelper,
                input,
                new List<bool>(algorithmsMatrix[(int)MultiCriteriaAlgorithm.AHP])),

                new Models.Task(
                new OwnWeightsBuilder(configuration.CriteriaWeights.Select(w => w / 100.0).ToList()),
                branchAndBoundHelper,
                input,
                new List<bool>(algorithmsMatrix[(int)MultiCriteriaAlgorithm.OwnWeights]))
            };

            algorithmsToRun.ForEach(alg => alg.Run());

            return algorithmsToRun.Select(alg => alg.GetResults()).ToList();
        }
    }
}
