using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CRM_Sample.Common;
using CRM_Sample.Data;
using CRM_Sample.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using static CRM_Sample.Models.SalesModels.OpportunityAction;

namespace CRM_Sample.Controllers.SalesControllers
{
    public class PlannedActionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PlannedActionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            var plannedActions = await (from opportunityActions in _context.OpportunityActions.Where(o => o.Status == ActionStatus.Planned).Include(o => o.Person).ThenInclude(p => p.CompanyEmployees).OrderBy(o => o.Date)

                                        select new PlannedActionsViewModel()
                                        {
                                            ActionId = opportunityActions.Id,
                                            Opportunity = opportunityActions.Opportunity,
                                            Company = opportunityActions.Opportunity.Company,
                                            Date = opportunityActions.Date,
                                            ActionTypeName = opportunityActions.ActionType.Name,
                                            Person = opportunityActions.Person,
                                            ErpUser = opportunityActions.ErpUser,
                                            Description = opportunityActions.Description,
                                            PipelineStage = opportunityActions.Pipeline.Stage,
                                            PlannedStatus = opportunityActions.Date.Date < DateTime.Today ? "Atrasada" : opportunityActions.Date.Date == DateTime.Today ? "Hoje" : opportunityActions.Date.Date == DateTime.Today.AddDays(1) ? "Amanhã" : "Em dia"
                                        }).ToListAsync();

            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();
            ViewData["ErpUser"] = new SelectList(_context.ErpUsers.Where(e => e.Active), "Id", "Name");
            return View(plannedActions);
        }
        public async Task<IActionResult> FilteredIndex(
            int? erpUserId,
            string status,
            string DateStart,
            string DateEnd,
            string searchString,
            string currentFilter)
        {
            var plannedActions = await (from opportunityActions in _context.OpportunityActions
                                            .Where(o => o.Status == ActionStatus.Planned)
                                            .Include(o => o.Person).ThenInclude(p => p.CompanyEmployees)
                                            .Include(x => x.Opportunity)
                                            .OrderBy(o => o.Date)
                                        select new PlannedActionsViewModel()
                                        {
                                            ActionId = opportunityActions.Id,
                                            Opportunity = opportunityActions.Opportunity,
                                            Company = opportunityActions.Opportunity.Company,
                                            Date = opportunityActions.Date,
                                            ActionTypeName = opportunityActions.ActionType.Name,
                                            Person = opportunityActions.Person,
                                            ErpUser = opportunityActions.ErpUser,
                                            Description = opportunityActions.Description,
                                            PipelineStage = opportunityActions.Pipeline.Stage,
                                            PlannedStatus = opportunityActions.Date.Date < DateTime.Today ? "Atrasada" : opportunityActions.Date.Date == DateTime.Today ? "Hoje" : opportunityActions.Date.Date == DateTime.Today.AddDays(1) ? "Amanhã" : "Em dia"
                                        }).ToListAsync();

            if (erpUserId != null && erpUserId != 0)
            {
                plannedActions = plannedActions.Where(p => p.ErpUser.Id == erpUserId).ToList();
            }
            if (status != null && status != "Todas")
            {
                plannedActions = plannedActions.Where(p => p.PlannedStatus == status).ToList();
            }

            if (string.IsNullOrEmpty(searchString))
            {
                searchString = currentFilter;
            }
            else
            {
                plannedActions = plannedActions.Where(x =>
                    x.Company.FriendlyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (x.Person != null && (
                        (x.Person.FirstName != null && x.Person.FirstName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                        (x.Person.LastName != null && x.Person.LastName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                        (x.Person.MiddleName != null && x.Person.MiddleName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                        (x.Person.FullName != null && x.Person.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    )) ||
                    (x.Opportunity.Title != null && x.Opportunity.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            if (DateStart != null)
            {
                DateTime dateStart = Convert.ToDateTime(DateStart);
                plannedActions = plannedActions.Where(p => p.Date.Date >= dateStart).ToList();
            }
            if (DateEnd != null)
            {
                DateTime dateEnd = Convert.ToDateTime(DateEnd);
                plannedActions = plannedActions.Where(p => p.Date.Date <= dateEnd).ToList();
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["ErpUser"] = new SelectList(_context.ErpUsers.Where(e => e.Active), "Id", "Name", erpUserId);
            return PartialView("_PlannedActionsPartialView", plannedActions);
        }
    }
}