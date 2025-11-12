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
    public class GetAllConditionsQueryHandler : IRequestHandler<GetAllConditionsQuery, List<RuleCondition>>
    {
        private readonly MentalHealthDbContext _context;

        public GetAllConditionsQueryHandler(MentalHealthDbContext context)
        {
            _context = context;
        }

        public async Task<List<RuleCondition>> Handle(GetAllConditionsQuery request, CancellationToken cancellationToken)
        {
            return await _context.RuleConditions.ToListAsync(cancellationToken);
        }
    }
}
