using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RLBW_ERP.Data;
using RLBW_ERP.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RLBW_ERP.Models.SalesModels.Opportunity;

namespace RLBW_ERP.Controllers.SalesControllers
{
    public class SalesIndicatorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SalesIndicatorsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> SalesFunnel()
        {
            var allOpportunities = _context.Opportunities;
            var opportunitiesCount = allOpportunities
                .Where(c => c.Status == OpportunityStatus.Opened)
                .Count();

            IQueryable<SalesFunnelViewModel> salesFunnel =
                from opportunities in _context.Opportunities
                    .Where(c => c.Status == OpportunityStatus.Opened)
                group opportunities by opportunities.Id into PipelineStage
                orderby PipelineStage.Key
                select new SalesFunnelViewModel()
                {
                    Id = (int)PipelineStage.Key,
                    OpportunitiesCount = PipelineStage.Count() == 0 ? 100 : PipelineStage.Count(),
                    OpportunitiesPercentage = Math.Round((double)PipelineStage.Count() / opportunitiesCount * 100, 1, MidpointRounding.AwayFromZero)
                };
            return View(await salesFunnel.AsNoTracking().ToListAsync());
        }
        public async Task<IActionResult> SalesStatus()
        {
            var allOpportunities = _context.Opportunities;
            var opportunitiesCount = allOpportunities.Count();
            var names = new List<OpportunityStatus>();

            IQueryable<SalesFunnelViewModel> salesStatus =
                from opportunities in _context.Opportunities.Include(o=>o.Status)
                group opportunities by opportunities.Status into SalesStatus
                orderby SalesStatus.Key
                select new SalesFunnelViewModel()
                {
                    OpportunityStatus = SalesStatus.Key,
                    OpportunitiesCount = SalesStatus.Count() == 0 ? 100 : SalesStatus.Count(),
                    OpportunitiesPercentage = Math.Round((double)SalesStatus.Count() / opportunitiesCount * 100, 1, MidpointRounding.AwayFromZero)
                };
            return View(await salesStatus.AsNoTracking().ToListAsync());

        }
    }
}