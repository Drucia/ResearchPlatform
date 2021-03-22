using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchPlatform.Models
{
    public class JobWithChoose : Job
    {
        public bool IsChosen { get; set; }

        public JobWithChoose(Job job) : base(job)
        {
            IsChosen = false;
        }
    }
}
