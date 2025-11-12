using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Layer.Dto
{
    public class DiagnosisRequestDto
    {
        public Dictionary<string, List<string>> Answers { get; set; } = new();
    }
}
