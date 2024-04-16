using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CRM_Sample.Data;
using CRM_Sample.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using static CRM_Sample.Models.SalesModels.OpportunityAction;

namespace CRM_Sample.Controllers.SalesControllers
{
    public class DoneActionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DoneActionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string erpUserName)
        {
            var userId = _context.ErpUsers.Where(e => e.Name.Contains(erpUserName)).First().Id;

            var doneActions = await (from opportunityActions in _context.OpportunityActions
                                     where opportunityActions.Status == ActionStatus.Done && opportunityActions.ErpUserId == userId
                                     orderby opportunityActions.Date descending
                                     select new DoneActionsViewModel()
                                     {
                                         ActionId = opportunityActions.Id,
                                         OpportunityId = opportunityActions.OpportunityId,
                                         Company = opportunityActions.Opportunity.Company.FriendlyName,
                                         Date = opportunityActions.Date,
                                         ActionTypeName = opportunityActions.ActionType.Name,
                                         Person = opportunityActions.Person.FullName,
                                         ErpUser = opportunityActions.ErpUser,
                                         Description = opportunityActions.Description,
                                         PipelineStage = opportunityActions.Pipeline.Stage,
                                         ActionStatus = opportunityActions.Date.Date == DateTime.Today ? "Hoje" :
                                            opportunityActions.Date.Date == DateTime.Today.AddDays(-1) ? "Ontem" :
                                            opportunityActions.Date.Date > DateTime.Today.AddDays(-7) ? "Última semana" :
                                            "Mais antiga"
                                     }).Take(50).ToListAsync();

            ViewData["ErpUser"] = new SelectList(_context.ErpUsers, "Id", "Name",userId);
            ViewData["SelectedUser"] = userId;
            return View(doneActions);
        }
        public async Task<IActionResult> FilteredIndex(int? erpUserId, string status, string DateStart, string DateEnd, string strQuantity)
        {
            var quantity = 50;
            var doneActions = await (from opportunityActions in _context.OpportunityActions
                                     where opportunityActions.Status == ActionStatus.Done
                                     orderby opportunityActions.Date descending
                                     select new DoneActionsViewModel()
                                     {
                                         ActionId = opportunityActions.Id,
                                         OpportunityId = opportunityActions.OpportunityId,
                                         Company = opportunityActions.Opportunity.Company.FriendlyName,
                                         Date = opportunityActions.Date,
                                         ActionTypeName = opportunityActions.ActionType.Name,
                                         Person = opportunityActions.Person.FullName,
                                         ErpUser = opportunityActions.ErpUser,
                                         Description = opportunityActions.Description,
                                         PipelineStage = opportunityActions.Pipeline.Stage,
                                         ActionStatus = opportunityActions.Date.Date == DateTime.Today ? "Hoje" :
                                            opportunityActions.Date.Date == DateTime.Today.AddDays(-1) ? "Ontem" :
                                            opportunityActions.Date.Date > DateTime.Today.AddDays(-7) ? "Última semana" :
                                            "Mais antiga"
                                     }).ToListAsync();
            if (erpUserId != null && erpUserId != 0)
            {
                doneActions = doneActions.Where(p => p.ErpUser.Id == erpUserId).ToList();
            }
            if (status != null && status != "null" && status != "Todas")
            {
                doneActions = doneActions.Where(p => p.ActionStatus == status).ToList();
            }
            if (DateStart != null)
            {
                DateTime dateStart = Convert.ToDateTime(DateStart);
                doneActions = doneActions.Where(p => p.Date.Date >= dateStart).ToList();
            }
            if (DateEnd != null)
            {
                DateTime dateEnd = Convert.ToDateTime(DateEnd);
                doneActions = doneActions.Where(p => p.Date.Date <= dateEnd).ToList();
            }
            if (strQuantity != "Todas")
            {
                quantity = strQuantity != null && strQuantity!="null" && strQuantity!="undefined" ? Convert.ToInt32(strQuantity) : quantity;
                var actions = doneActions.Take(quantity);
                return PartialView("_DoneActionsPartialView", actions);
            }
            ViewData["ErpUser"] = new SelectList(_context.ErpUsers, "Id", "Name", erpUserId);
            return PartialView("_DoneActionsPartialView", doneActions);
        }
    }
}