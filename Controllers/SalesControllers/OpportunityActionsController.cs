using CRM_Sample.Common;
using CRM_Sample.Data;
using CRM_Sample.Models.CustomerModels;
using CRM_Sample.Models.SalesModels;
using CRM_Sample.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CRM_Sample.Controllers.SalesControllers
{
    public class OpportunityActionsController : Controller
    {

        private const string BIND_STRING = "Id,Date,ActionTypeId,PersonId,ErpUserId,Description,PipelineId,Status,OpportunityId";

        private readonly ApplicationDbContext _context;
        private readonly DateTimeFunctions _now;

        public OpportunityActionsController(ApplicationDbContext context, DateTimeFunctions now)
        {
            _context = context;
            _now = now;
        }

        // GET: OpportunityActions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.OpportunityActions
                .Include(o => o.Opportunity)
                .Include(o => o.ActionType)
                .Include(o => o.ErpUser)
                .Include(o => o.Person)
                .Include(o => o.Pipeline);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: OpportunityActions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunityActions = await _context.OpportunityActions
                .Include(o => o.Opportunity)
                .Include(o => o.ActionType)
                .Include(o => o.ErpUser)
                .Include(o => o.Person)
                .Include(o => o.Pipeline)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (opportunityActions == null)
            {
                return NotFound();
            }

            return View(opportunityActions);
        }

        // GET: OpportunityActions/Create
        public async Task<IActionResult> Create(int opportunityId)
        {
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);

            OpportunityAction opportunityActions = new()
            {
                OpportunityId = opportunityId,
                Date = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
            };

            //ViewData["Id"] = opportunity.CompanyId;
            //ViewData["Now"] = _now.GetNow();
            CreateViewDataSet(opportunity.CompanyId, opportunityId);
            return PartialView(opportunityActions);
        }

        // POST: OpportunityActions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] OpportunityAction opportunityAction, RecurrenceType recurrenceType = RecurrenceType.None, int quantity = 1)
        {
            var newModels = new List<OpportunityAction>();
            TrimStrings.TrimStringsFunction(opportunityAction);

            if (opportunityAction.PersonId == 0)
            {
                opportunityAction.PersonId = null;
            }

            var opportunity = await _context.Opportunities
                .Where(o => o.Id == opportunityAction.OpportunityId)
                .Include(o => o.OpportunityActions)
                .FirstOrDefaultAsync();
            var pipeline = 0;

            if (opportunity.OpportunityActions.Count != 0)
            {
                var lastAction = opportunity.OpportunityActions.Max(o => o.Date);
                pipeline = DateTime.Compare(lastAction, opportunityAction.Date);
            }
            else
            {
                pipeline = -1;
            }
            if (pipeline <= 0)
            {
                opportunity.PipelineId = opportunityAction.PipelineId;
                _context.Update(opportunity);
            }
            if (ModelState.IsValid)
            {
                if (recurrenceType != RecurrenceType.None)
                {
                    var repetitionDates = CalculateRepetitionDates(recurrenceType, quantity, opportunityAction.Date.Date);
                    var Descriptions = GenerateDescriptions(opportunityAction.Description, quantity);
                    opportunityAction.Description = opportunityAction.Description.Replace("{##}", "01");
                    for (int i = 0; i < repetitionDates.Count; i++)
                    {
                        var newModel = new OpportunityAction()
                        {
                            Date = repetitionDates[i],
                            Description = Descriptions[i],
                            ActionTypeId = opportunityAction.ActionTypeId,
                            ErpUserId = opportunityAction.ErpUserId,
                            OpportunityId = opportunityAction.OpportunityId,
                            PersonId = opportunityAction.PersonId,
                            PipelineId = opportunityAction.PipelineId,
                            Status = OpportunityAction.ActionStatus.Planned,
                        };
                        newModels.Add(newModel);
                    }
                    _context.AddRange(newModels);
                }
                _context.Add(opportunityAction);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }

            CreateViewDataSet(opportunity.CompanyId, opportunityAction.OpportunityId);
            return PartialView(opportunityAction);
        }

        private List<string> GenerateDescriptions(string Description, int quantity)
        {
            var Descriptions = new List<string>();

            for (int i = 2; i <= quantity; i++)
            {
                string replacedDescription = Description.Replace("{##}", i.ToString("D2"));
                Descriptions.Add(replacedDescription);
            }
            return Descriptions;
        }

        // GET: OpportunityActions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunityAction = await _context.OpportunityActions.FindAsync(id);
            if (opportunityAction == null)
            {
                return NotFound();
            }
            var opportunity = await _context.Opportunities
                .Include(o => o.Company)
                .FirstOrDefaultAsync(o => o.Id == opportunityAction.OpportunityId);

            var opportunities = await (from o in _context.Opportunities
                           .Where(x => x.CompanyId == opportunity.CompanyId)
                           .Include(x => x.Product).ThenInclude(x => x.Category)
                                       select new
                                       {
                                           o.Id,
                                           o.Title,
                                           Category = o.Product.Category.Name,
                                           Product = o.Product.Name,
                                           DisplayField = string.Format("{0} ({1})", o.Id, o.Title ?? o.Product.Category.Name + o.Product.Name)
                                       }).ToListAsync();

            CreateViewDataSet(opportunity.CompanyId, opportunityAction.OpportunityId, opportunityAction);
            return PartialView(opportunityAction);
        }


        // POST: OpportunityActions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] OpportunityAction opportunityAction)
        {

            if (id != opportunityAction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    TrimStrings.TrimStringsFunction(opportunityAction);
                    opportunityAction.PersonId = opportunityAction.PersonId == 0 ? null : opportunityAction.PersonId;
                    _context.Update(opportunityAction);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Details", "Opportunities", new { id = opportunityAction.OpportunityId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OpportunityActionsExists(opportunityAction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            var opportunity = await _context.Opportunities
                        .Where(o => o.Id.Equals(opportunityAction.OpportunityId))
                        .Include(o => o.OpportunityActions)
                        .Include(x => x.Proposals)
                        .FirstOrDefaultAsync();

            CreateViewDataSet(opportunity.CompanyId, opportunityAction.OpportunityId, opportunityAction);
            return View(opportunityAction);
        }


        private void UpdateOpportunity(OpportunityAction opportunityActions, Opportunity opportunity)
        {
            opportunity = UpdateOpportunityPipeline(opportunityActions, opportunity);
            opportunity = UpdateOpportunityDate(opportunityActions, opportunity);
            _context.Update(opportunity);
        }

        private Opportunity UpdateOpportunityDate(OpportunityAction opportunityActions, Opportunity opportunity)
        {
            var doneActions = opportunity.OpportunityActions.Where(x => x.Status == OpportunityAction.ActionStatus.Done).Count();
            var firstDoneAction = opportunity.Date;
            if (doneActions > 0)
            {
                firstDoneAction = opportunity.OpportunityActions.Where(x => x.Status == OpportunityAction.ActionStatus.Done).Min(o => o.Date).Date;
            }
            firstDoneAction = firstDoneAction > opportunityActions.Date.Date ? opportunityActions.Date.Date : firstDoneAction;
            DateTime dateToSet = opportunity.Date > firstDoneAction ? firstDoneAction : opportunity.Date;

            if (opportunity.Proposals.Count > 0)
            {
                var firstProposal = opportunity.Proposals.Min(x => x.Date).Date;
                dateToSet = dateToSet > firstProposal ? firstProposal : dateToSet;
            }
            opportunity.Date = dateToSet;
            return opportunity;
        }

        private static Opportunity UpdateOpportunityPipeline(OpportunityAction opportunityActions, Opportunity opportunity)
        {
            var doneActions = opportunity.OpportunityActions.Where(x => x.Status == OpportunityAction.ActionStatus.Done).Count();
            var lastDoneAction = opportunity.Date;
            int lastActionId = 0;
            if (doneActions > 0)
            {
                lastDoneAction = opportunity.OpportunityActions.Where(x => x.Status == OpportunityAction.ActionStatus.Done).Max(o => o.Date);
                lastActionId = opportunity.OpportunityActions.FirstOrDefault(o => o.Date.Equals(lastDoneAction)).Id;
            }

            if (lastActionId.Equals(opportunityActions.Id) && lastActionId > 0)
            {
                opportunity.Id = opportunityActions.Id;
            }
            return opportunity;
        }

        public async Task<IActionResult> Delay(int actionId, string days)
        {
            int daysToDelay = Convert.ToInt32(days);
            var opportunityAction = await _context.OpportunityActions.FindAsync(actionId);
            var act = new DelayActionViewModel()
            {
                ActionId = actionId,
                Days = daysToDelay,
                OldDate = opportunityAction.Date,
                NewDate = new DateTimeFunctions().GetNow().AddDays(daysToDelay)
            };

            return PartialView(act);
        }
        [HttpPost]
        public async Task<IActionResult> Delay(DelayActionViewModel model)
        {
            var opportunityAction = await _context.OpportunityActions.FindAsync(model.ActionId);
            TimeSpan ts = new(9, 0, 0);
            opportunityAction.Date = model.NewDate;
            opportunityAction.Date += ts;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(opportunityAction);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Opportunities", new { id = opportunityAction.OpportunityId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OpportunityActionsExists(opportunityAction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(opportunityAction);
        }


        //GET
        public async Task<IActionResult> Copy(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunityAction = await _context.OpportunityActions.FindAsync(id);
            if (opportunityAction == null)
            {
                return NotFound();
            }
            opportunityAction.Description = "";
            var opportunity = await _context.Opportunities.Include(o => o.Company).FirstOrDefaultAsync(o => o.Id == opportunityAction.OpportunityId);

            CreateViewDataSet(opportunity.CompanyId, opportunityAction.OpportunityId, opportunityAction);
            return PartialView(opportunityAction);
        }

        // POST: OpportunityActions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Copy([Bind(BIND_STRING)] OpportunityAction opportunityAction)
        {
            var newAction = new OpportunityAction()
            {
                Date = opportunityAction.Date,
                ActionType = opportunityAction.ActionType,
                ActionTypeId = opportunityAction.ActionTypeId,
                Description = opportunityAction.Description,
                ErpUserId = opportunityAction.ErpUserId,
                OpportunityId = opportunityAction.OpportunityId,
                PersonId = opportunityAction.PersonId == 0 ? null : opportunityAction.PersonId,
                PipelineId = opportunityAction.PipelineId,
                Status = opportunityAction.Status
            };
            TrimStrings.TrimStringsFunction(newAction);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(newAction);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Opportunities", new { id = newAction.OpportunityId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OpportunityActionsExists(newAction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            var opportunity = await _context.Opportunities
                .Where(o => o.Id.Equals(opportunityAction.OpportunityId))
                .Include(o => o.OpportunityActions)
                .Include(x => x.Proposals)
                .FirstOrDefaultAsync();
            CreateViewDataSet(opportunity.CompanyId, opportunityAction.OpportunityId, opportunityAction);
            return View(opportunityAction);
        }

        //GET
        public async Task<IActionResult> WaitForReply(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunityAction = await _context.OpportunityActions.FindAsync(id);
            if (opportunityAction == null)
            {
                return NotFound();
            }

            opportunityAction.Description = "Aguardar retorno";
            TimeSpan ts = new(9, 0, 0);
            opportunityAction.Date = _now.GetNow().AddDays(7);
            opportunityAction.Status = OpportunityAction.ActionStatus.Planned;

            var opportunity = await _context.Opportunities.Include(o => o.Company).FirstOrDefaultAsync(o => o.Id == opportunityAction.OpportunityId);

            CreateViewDataSet(opportunity.CompanyId, opportunityAction.OpportunityId, opportunityAction);
            return PartialView(opportunityAction);
        }

        // POST: OpportunityActions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> WaitForReply([Bind(BIND_STRING)] OpportunityAction opportunityAction)

        {
            TrimStrings.TrimStringsFunction(opportunityAction);
            if (opportunityAction.PersonId == 0)
            {
                opportunityAction.PersonId = null;
            }
            var opportunity = await _context.Opportunities
                .Where(o => o.Id.Equals(opportunityAction.OpportunityId))
                .Include(o => o.OpportunityActions)
                .Include(x => x.Proposals)
                .FirstOrDefaultAsync();
            if (opportunity.OpportunityActions.Count != 0)
            {
                UpdateOpportunity(opportunityAction, opportunity);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(opportunityAction);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Opportunities", new { id = opportunityAction.OpportunityId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OpportunityActionsExists(opportunityAction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            CreateViewDataSet(opportunity.CompanyId, opportunityAction.OpportunityId, opportunityAction);
            return View(opportunityAction);
        }

        // GET: OpportunityActions/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunityActions = await _context.OpportunityActions
                .Include(o => o.ActionType)
                .Include(o => o.ErpUser)
                .Include(o => o.Person)
                .Include(o => o.Pipeline)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (opportunityActions == null)
            {
                return NotFound();
            }

            return View(opportunityActions);
        }

        // POST: OpportunityActions/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var opportunityActions = await _context.OpportunityActions.FindAsync(id);
            _context.OpportunityActions.Remove(opportunityActions);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OpportunityActionsExists(int id)
        {
            return _context.OpportunityActions.Any(e => e.Id == id);
        }

        // GET: OpportunityActions/Duplicate
        public async Task<IActionResult> Duplicate(int opportunityId, int actionId)
        {
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            var opportunityAction = await _context.OpportunityActions.FindAsync(actionId);

            if (opportunity.OpportunityGroup == null)
            {
                ViewData["DoNotDuplicate"] = true;
                return PartialView();
            }
            else
            {
                ViewData["DoNotDuplicate"] = false;
            }

            Opportunity opportunitiesRelated = new()
            {
                OpportunitiesRelated = await _context.Opportunities
                .Where(o => o.OpportunityGroup == opportunity.OpportunityGroup && o.Id != opportunityId)
                .Include(o => o.Product).ThenInclude(p => p.Category)
                .Include(o => o.OpportunityActions)
                .ToListAsync()
            };
            foreach (var item in opportunitiesRelated.OpportunitiesRelated)
            {
                var opportunityActions = _context.OpportunityActions.Where(
                    o => o.Date == opportunityAction.Date &&
                    o.Description == opportunityAction.Description &&
                    o.OpportunityId == item.Id);

                item.Checked = await _context.OpportunityActions.AnyAsync(
                    o => o.Date == opportunityAction.Date &&
                    o.Description == opportunityAction.Description &&
                    o.OpportunityId == item.Id);

                System.Diagnostics.Debug.WriteLine(item.Id);
                System.Diagnostics.Debug.WriteLine(item.Id);
            }


            ViewData["Id"] = actionId;
            CreateViewDataSet(opportunity.CompanyId, opportunityAction.OpportunityId, opportunityAction);
            return PartialView(opportunitiesRelated);
        }

        // POST: OpportunityActions/Duplicate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(Opportunity opportunities, int actionId)
        {
            OpportunityAction opportunityAction = new();
            opportunityAction = await _context.OpportunityActions.FindAsync(actionId);
            int counter = 0;
            if (ModelState.IsValid)
            {
                var pipeline = 0;
                foreach (var item in opportunities.OpportunitiesRelated.Where(o => o.Checked == true))
                {
                    OpportunityAction action = new()
                    {
                        Date = opportunityAction.Date,
                        Description = opportunityAction.Description,
                        ActionTypeId = opportunityAction.ActionTypeId,
                        ErpUserId = opportunityAction.ErpUserId,
                        OpportunityId = item.Id,
                        PersonId = opportunityAction.PersonId,
                        Id = opportunityAction.Id,
                        Status = opportunityAction.Status,
                    };
                    var opportunity = await _context.Opportunities.Include(o => o.OpportunityActions).FirstOrDefaultAsync(o => o.Id == item.Id);
                    if (opportunity.OpportunityActions.Count != 0)
                    {
                        var lastAction = opportunity.OpportunityActions.Max(o => o.Date);
                        pipeline = DateTime.Compare(lastAction, opportunityAction.Date);
                    }
                    else
                    {
                        pipeline = -1;
                    }

                    _context.Add(action);
                    counter++;
                    if (pipeline <= 0)
                    {
                        opportunity.Id = opportunityAction.Id;
                        _context.Update(opportunity);
                    }
                }
                TempData["msg"] = $"<script>alert('Ação copiada para {counter} oportunidades.');</script>";

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Opportunities", new { id = opportunityAction.OpportunityId });
            }
            CreateViewDataSet(opportunities.CompanyId, opportunities.Id, opportunityAction);
            return View(opportunityAction);
        }
        private void CreateViewDataSet(int companyId, int? opportunityId, OpportunityAction opportunityAction = null)
        {
            ViewData["ActionTypeId"] = new SelectList(_context.ActionTypes, "Id", "Name", opportunityAction?.ActionTypeId);
            ViewData["ErpUserId"] = new SelectList(_context.ErpUsers.Where(e => e.Active == true).OrderBy(e => e.Name), "Id", "Name", opportunityAction?.ErpUserId);
            ViewData["PersonId"] = new SelectList(GetCompanyEmployees(companyId).OrderBy(p => p.FirstName).ThenBy(p => p.MiddleName).ThenBy(p => p.LastName), "Id", "FullName", opportunityAction?.PersonId);
            ViewData["PipelineId"] = new SelectList(_context.Pipelines, "Id", "Stage", opportunityAction?.PipelineId);
            ViewData["OpportunityId"] = new SelectList(_context.Opportunities, "Id", "Id", opportunityId);

        }
        private IEnumerable<Person> GetCompanyEmployees(int companyId)
        {
            return _context.People
                .Where(p => _context.CompanyEmployees
                    .Any(e => e.CompanyId == companyId
                    && e.Status == true
                    && e.PersonId == p.Id))
                .OrderBy(p => p.FirstName).ThenBy(p => p.MiddleName).ThenBy(p => p.LastName)
                .ToList();
        }
        private static List<DateTime> CalculateRepetitionDates(RecurrenceType recurrenceType, int quantity, DateTime date)
        {
            List<DateTime> dates = new();
            TimeSpan ts = new(9, 0, 0);

            for (int i = 1; i < quantity; i++)
            {
                switch (recurrenceType)
                {
                    case RecurrenceType.Daily: dates.Add(date.AddDays(i) + ts); break;
                    case RecurrenceType.Weekly: dates.Add(date.AddDays((i) * 7) + ts); break;
                    case RecurrenceType.Monthly: dates.Add(date.AddMonths(i) + ts); break;
                    case RecurrenceType.Annually: dates.Add(date.AddYears(i) + ts); break;
                }
            }
            return dates;
        }
    }
}

namespace CRM_Sample
{
    public enum RecurrenceType
    {
        [Display(Name = "Única")]
        None,
        [Display(Name = "Diário")]
        Daily,
        [Display(Name = "Semanal")]
        Weekly,
        [Display(Name = "Mensal")]
        Monthly,
        [Display(Name = "Anual")]
        Annually
    }
}