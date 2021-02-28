using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

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

        public static Configuration CurrentConfiguration { get; set; } = new Configuration();
    }
}
