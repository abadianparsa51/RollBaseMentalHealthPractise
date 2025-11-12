using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Layer.Dto
{
    public class DiagnosisResultDto
    {
        public bool Major_Depression { get; set; }
        public bool Hypomania { get; set; }
        public bool Mania { get; set; }
        public bool Panic_Disorder { get; set; }
        public bool Agoraphobia { get; set; }
        public bool Social_Phobia { get; set; }
        public bool Ocd { get; set; }
        public bool Ptsd { get; set; }
    }
}
