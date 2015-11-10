using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexanderDevelopment.ConfigDataMover.Lib
{
    public class JobStep
    {
        public string StepName { get; set; }
        public string StepFetch { get; set; }
        public bool UpdateOnly { get; set; }
    }
}
