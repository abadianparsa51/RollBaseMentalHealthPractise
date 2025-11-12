using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Layer.Querrys;
using Core.Layer.Data;
using Core.Model.Layer.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Core.Layer.Handlers
{
    public class GetAllRulesQueryHandler : IRequestHandler<GetAllRulesQuery, List<DiagnosticRule>>
    {
        private readonly MentalHealthDbContext _context;

        public GetAllRulesQueryHandler(MentalHealthDbContext context)
        {
            _context = context;
        }

        public async Task<List<DiagnosticRule>> Handle(GetAllRulesQuery request, CancellationToken cancellationToken)
        {
            return await _context.DiagnosticRules.ToListAsync(cancellationToken);
        }
    }
}
