using ResearchPlatform.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ResearchPlatform.Models
{
    public class Configuration
    {
        public ObservableCollection<ObservableCollection<string>> ComparisionMatrix { get; set; } = new ObservableCollection<ObservableCollection<string>> {
            new ObservableCollection<string>{ "1", "2", "1/4", "3", "1/2"},
            new ObservableCollection<string>{ "1/2", "1", "1/5", "1/3", "1/2"},
            new ObservableCollection<string>{ "4", "5", "1", "9", "2"},
            new ObservableCollection<string>{ "1/3", "3", "1/9", "1", "4" },
            new ObservableCollection<string>{ "2", "2", "1/2", "1/4", "1" }
        };

        public ObservableCollection<int> CriteriaWeights { get; set; } = new ObservableCollection<int> {
            5, // Criteria.ComfortOfWork 
            10, // Criteria.DrivingTime
            60, // Criteria.Profit
            15, // Criteria.CompletedJobs
            10 // Criteria.CustomerReliability
        };
        public static List<string> PossibleComparisionValues { get;}  = new List<string>{
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "1/2", "1/3", "1/4", "1/5", "1/6", "1/7", "1/8", "1/9"
        };

        public static List<string> PossibleTarpaulinTypes { get; } = new List<string>{
            TarpaulinTypes.TILT, TarpaulinTypes.CURTAIN, TarpaulinTypes.WAGON, TarpaulinTypes.CHEST, TarpaulinTypes.TROLLEY, TarpaulinTypes.COOLING_BODIES
        };

        public int TrackHeight { get; set; } = 200;
        public int TrackWidth { get; set; } = 800;
        public int TrackDepth { get; set; } = 200;
        public int MaxLoadCapacity { get; set; } = 2000;


        public static Configuration Create => new Configuration();

        public void CopyTo(Configuration originalConfiguration)
        {
            originalConfiguration.ComparisionMatrix = ComparisionMatrix;
            originalConfiguration.CriteriaWeights = CriteriaWeights;
        }

        public void fillMatrix()
        {
            var converter = new MatrixItemConverter();

            for (int row=0; row < ComparisionMatrix.Count; row++)
            {
                var rowCol = ComparisionMatrix[row];
                for (int col=0; col < rowCol.Count; col++)
                {
                    if (row == col)
                        break;

                    ComparisionMatrix[row][col] = converter.Convert(ComparisionMatrix[col][row], null, null, null) as string;
                }

            }
        }
    }
}
