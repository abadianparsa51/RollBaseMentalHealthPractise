using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Model.Layer.Entity;
using MediatR;

namespace Application.Layer.Querrys
{
    public class GetAllRulesQuery : IRequest<List<DiagnosticRule>>
    {
    }
}
