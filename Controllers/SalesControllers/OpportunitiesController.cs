using CRM_Sample.Common;
using CRM_Sample.Data;
using CRM_Sample.Models.SalesModels;
using CRM_Sample.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CRM_Sample.Controllers.SalesControllers
{
    public partial class OpportunitiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string BIND_STRING = "Id,Date,CompanyId,ProductId,ErpUserId,Value,PipelineId,Status,Notes,OpportunityGroup,Title";

        public OpportunitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Opportunities
        public async Task<IActionResult> Index(
            string status,
            string sortOrder,
            string searchstring,
            string currentFilter,
            int? pageNumber,
            int? pageSize)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["OpportunityIdSortParm"] = sortOrder == "Id" ? "OpportunityId_desc" : "Id";
            ViewData["OpportunityDateSortParm"] = sortOrder == "Date" ? "OpportunityDate_desc" : "Date";
            ViewData["CompanyNameSortParm"] = sortOrder == "CompanyName" ? "CompanyName_desc" : "CompanyName";
            ViewData["ProductSortParm"] = sortOrder == "Product" ? "Product_desc" : "Product";
            ViewData["ValueSortParm"] = sortOrder == "Value" ? "Value_desc" : "Value";
            ViewData["ManagerSortParm"] = sortOrder == "Manager" ? "Manager_desc" : "Manager";
            ViewData["PipelineSortParm"] = sortOrder == "Pipeline" ? "Pipeline_desc" : "Pipeline";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "Status_desc" : "Status";
            ViewData["LastActionSortParm"] = sortOrder == "LastAction" ? "LastAction_desc" : "LastAction";
            ViewData["ActionsCountSortParm"] = sortOrder == "ActionsCount" ? "ActionsCount_desc" : "ActionsCount";
            ViewData["TitleSortParm"] = sortOrder == "Title" ? "Title_desc" : "Title";

            if (pageSize == null)
            {
                pageSize = 25;
            }

            if (searchstring != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchstring = currentFilter;
            }

            ViewData["CurrentFilter"] = searchstring;
            ViewData["CurrentPageSize"] = pageSize;
            ViewData["Status"] = status;

            var opportunities = from o in _context.Opportunities
                .Include(o => o.OpportunityActions)
                .Include(o => o.Company)
                .Include(o => o.Manager)
                .Include(o => o.Pipeline)
                .Include(o => o.Product)
                    .ThenInclude(p => p.Category)
                .AsNoTracking()
                                select o;

            switch (status)
            {
                case "Opened":
                    opportunities = opportunities.Where(o => o.Status == Opportunity.OpportunityStatus.Opened);
                    break;
                case "Sold":
                    opportunities = opportunities.Where(o => o.Status == Opportunity.OpportunityStatus.Sold);
                    break;
                case "Freezed":
                    opportunities = opportunities.Where(o => o.Status == Opportunity.OpportunityStatus.Closed);
                    break;
                case "Canceled":
                    opportunities = opportunities.Where(o => o.Status == Opportunity.OpportunityStatus.Canceled);
                    break;
                case "Lost":
                    opportunities = opportunities.Where(o => o.Status == Opportunity.OpportunityStatus.Lost);
                    break;
            }

            if (!string.IsNullOrEmpty(searchstring))
            {
                opportunities = opportunities.Where(a =>
                    a.Company.FriendlyName.Contains(searchstring, StringComparison.OrdinalIgnoreCase) ||
                    a.Title.Contains(searchstring, StringComparison.OrdinalIgnoreCase) ||
                    a.Product.Category.CategoryName.Contains(searchstring, StringComparison.OrdinalIgnoreCase) ||
                    a.Product.Name.Contains(searchstring, StringComparison.OrdinalIgnoreCase)
                    );
            }

            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "OpportunityId_desc";
            }

            switch (sortOrder)
            {
                case "Id":
                    opportunities = opportunities.OrderBy(s => s.Id);
                    break;
                case "OpportunityId_desc":
                    opportunities = opportunities.OrderByDescending(s => s.Id);
                    break;
                case "Date":
                    opportunities = opportunities.OrderBy(s => s.Date);
                    break;
                case "OpportunityDate_desc":
                    opportunities = opportunities.OrderByDescending(s => s.Date);
                    break;
                case "CompanyName":
                    opportunities = opportunities.OrderBy(s => s.Company.FriendlyName);
                    break;
                case "CompanyName_desc":
                    opportunities = opportunities.OrderByDescending(s => s.Company.FriendlyName);
                    break;
                case "Product":
                    opportunities = opportunities.OrderBy(s => s.Product.Name);
                    break;
                case "Product_desc":
                    opportunities = opportunities.OrderByDescending(s => s.Product.Name);
                    break;
                case "Value":
                    opportunities = opportunities.OrderBy(s => s.Value);
                    break;
                case "Value_desc":
                    opportunities = opportunities.OrderByDescending(s => s.Value);
                    break;
                case "Manager":
                    opportunities = opportunities.OrderBy(s => s.Manager.Name);
                    break;
                case "Manager_desc":
                    opportunities = opportunities.OrderByDescending(s => s.Manager.Name);
                    break;
                case "Pipeline":
                    opportunities = opportunities.OrderBy(s => s.Id);
                    break;
                case "Pipeline_desc":
                    opportunities = opportunities.OrderByDescending(s => s.Id);
                    break;
                case "Status":
                    opportunities = opportunities.OrderBy(s => s.Status);
                    break;
                case "Status_desc":
                    opportunities = opportunities.OrderByDescending(s => s.Status);
                    break;
                case "LastAction":
                    opportunities = opportunities.OrderBy(s => s.OpportunityActions.Max(o => o.Date));
                    break;
                case "LastAction_desc":
                    opportunities = opportunities.OrderByDescending(s => s.OpportunityActions.Max(o => o.Date));
                    break;
                case "ActionsCount":
                    opportunities = opportunities.OrderBy(s => s.OpportunityActions.Count);
                    break;
                case "ActionsCount_desc":
                    opportunities = opportunities.OrderByDescending(s => s.OpportunityActions.Count);
                    break;
                case "Title":
                    opportunities = opportunities.OrderBy(s => s.Title);
                    break;
                case "Title_desc":
                    opportunities = opportunities.OrderByDescending(s => s.Title);
                    break;
                default:
                    opportunities = opportunities.OrderByDescending(s => s.Id);
                    break;


            }
            return View(await PaginetedList<Opportunity>.CreateAsync(opportunities.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Opportunities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await (from o in _context.Opportunities
                .Include(o => o.Company)
                .Include(o => o.Manager)
                .Include(o => o.Pipeline)
                .Include(o => o.Product)
                    .ThenInclude(p => p.Category)
                                     where o.Id == id
                                     select new OpportunityDetailsViewModel()
                                     {
                                         Id = o.Id,
                                         Title = o.Title,
                                         Date = o.Date.ToString("g"),
                                         Company = o.Company.FriendlyName,
                                         CompanyId = o.CompanyId,
                                         Category = o.Product.Category.CategoryName,
                                         Product = o.Product.Name,
                                         Manager = o.Manager.Name,
                                         Value = o.Value,
                                         Pipeline = o.Pipeline.Stage,
                                         Status = o.GetStatusDisplayName(),
                                         Notes = o.Notes,
                                         OpportunityGroup = o.OpportunityGroup,

                                     }).AsNoTracking().FirstAsync();

            var plannedActions = await (from a in _context.OpportunityActions
                            .Include(x => x.Person).Include(x => x.ErpUser).Include(x => x.ActionType)
                                        where a.OpportunityId == id && a.Status == OpportunityAction.ActionStatus.Planned
                                        orderby a.Date descending
                                        select new OpportunityActionsViewModel
                                        {
                                            Id = a.Id,
                                            Date = a.Date,
                                            ActionType = a.ActionType.Name,
                                            Contact = a.Person.FullName,
                                            Manager = a.ErpUser.Name,
                                            Description = a.Description,
                                            Pipeline = a.Pipeline.Stage,
                                            Status = a.GetStatusDisplayName()
                                        }).AsNoTracking().ToListAsync();

            var otherActions = await (from a in _context.OpportunityActions
                                      .Include(x => x.Person).Include(x => x.ErpUser).Include(x => x.ActionType)
                                      where a.OpportunityId == id && a.Status != OpportunityAction.ActionStatus.Planned
                                      orderby a.Date descending
                                      select new OpportunityActionsViewModel
                                      {
                                          Id = a.Id,
                                          Date = a.Date,
                                          ActionType = a.ActionType.Name,
                                          Contact = a.Person.FullName,
                                          Manager = a.ErpUser.Name,
                                          Description = a.Description,
                                          Pipeline = a.Pipeline.Stage,
                                          Status = a.GetStatusDisplayName()
                                      }).AsNoTracking().ToListAsync();

            var combinedActions = plannedActions.Concat(otherActions).ToList();

            opportunity.Actions = combinedActions;
            opportunity.Proposals = await (from p in _context.Proposals
                                           where p.OpportunityId == id
                                           select new ProposalViewModel()
                                           {
                                               Id = p.Id.ToString(),
                                               Date = p.Date.ToString("dd/MM/yyyy"),
                                               Price = string.Format("{0:N2}", p.Price),
                                               Status = p.GetStatusDisplayName(),
                                               Currency = p.Currency.ToString()
                                           }).AsNoTracking().ToListAsync();


            if (opportunity.OpportunityGroup != null)
            {
                opportunity.OpportunitiesRelated = await (from o in _context.Opportunities.Include(x => x.Product).ThenInclude(x => x.Category)
                                                          where o.OpportunityGroup == opportunity.OpportunityGroup
                                                          select new OpportunitiesRelatedViewModel()
                                                          {
                                                              Id = o.Id,
                                                              Title = o.Title,
                                                              Category = o.Product.Category.CategoryName
                                                          }).AsNoTracking().ToListAsync();
            }

            if (opportunity == null)
            {
                return NotFound();
            }

            Regex regex = ConvertUrl();

            for (int i = 0; i < opportunity.Actions.Count; i++)
            {
                string result = regex.Replace(opportunity.Actions[i].Description, match =>
                {
                    string url = match.Value;

                    if (url.StartsWith("www.") || url.StartsWith("ftp."))
                    {
                        url = "http://" + url;
                    }

                    if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                    {
                        return $"<a href=\"{uri}\" target=\"_blank\">{uri}</a>";
                    }
                    else
                    {
                        return match.Value;
                    }
                });
                opportunity.Actions[i].Description = result;
            }

            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();

            return View(opportunity);
        }

        // GET: Opportunities/Create
        public IActionResult Create(int? companyId)
        {
            Opportunity opportunity = new()
            {
                Date = DateTime.Today
            };
            if (companyId != null)
            {
                ViewData["Id"] = new SelectList(_context.Companies.OrderBy(o => o.Id), "Id", "FriendlyName", companyId);
            }
            else
            {
                ViewData["Id"] = new SelectList(_context.Companies.OrderBy(o => o.Id), "Id", "FriendlyName");
            }

            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();
            CreateViewData(true);
            return PartialView(opportunity);
        }


        // POST: Opportunities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] Opportunity opportunity)
        {
            opportunity.Value = 0;

            if (ModelState.IsValid)
            {
                _context.Add(opportunity);
                await _context.SaveChangesAsync();

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }
            CreateViewData(true, opportunity);
            return PartialView(opportunity);
        }

        // GET: Opportunities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await _context.Opportunities.Include(o => o.Product).FirstOrDefaultAsync(o => o.Id == id);
            if (opportunity == null)
            {
                return NotFound();
            }
            CreateViewData(false,opportunity);
            return PartialView(opportunity);
        }

        // POST: Opportunities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] Opportunity opportunity)
        {
            if (id != opportunity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(opportunity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OpportunityExists(opportunity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }
            CreateViewData(false,opportunity);
            return View(opportunity);
        }

        // GET: Opportunities/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await _context.Opportunities
                .Include(o => o.Company)
                .Include(o => o.Manager)
                .Include(o => o.Pipeline)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (opportunity == null)
            {
                return NotFound();
            }

            return PartialView(opportunity);
        }

        // POST: Opportunities/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var opportunity = await _context.Opportunities.FindAsync(id);
            _context.Opportunities.Remove(opportunity);
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OpportunityExists(int id)
        {
            return _context.Opportunities.Any(e => e.Id == id);
        }

        //GET
        public async Task<IActionResult> CreateGroup(int opportunityId, int companyId)
        {
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);

            Opportunity opportunitiesRelated = new()
            {
                OpportunitiesRelated = await _context.Opportunities
                .Where(o =>
                    o.OpportunityGroup == opportunity.OpportunityGroup &&
                    o.CompanyId == companyId &&
                    o.Id != opportunityId &&
                    (o.OpportunityGroup == null || o.OpportunityGroup == opportunity.OpportunityGroup))
                .ToListAsync()
            };

            foreach (var op in opportunitiesRelated.OpportunitiesRelated)
            {
                op.Checked = op.OpportunityGroup != null;
            }

            Opportunity companyOpportunities = new()
            {
                OpportunitiesRelated = await _context.Opportunities
                .Where(o =>
                    o.CompanyId == companyId &&
                    o.Id != opportunityId &&
                    (o.OpportunityGroup == null || o.OpportunityGroup == opportunity.OpportunityGroup))
                .Include(o => o.Product).ThenInclude(p => p.Category)
                .ToListAsync()
            };

            if (companyOpportunities.OpportunitiesRelated.Count.Equals(0))
            {
                ViewData["NoOpportunities"] = true;
                return View();
            }
            else
            {
                ViewData["NoOpportunities"] = false;
            }
            var company = await _context.Companies.FindAsync(companyId);
            ViewData["OpportunityId"] = opportunity.Id;
            ViewData["CompanyId"] = company.FriendlyName;
            ViewData["CompanyOpportunities"] = companyOpportunities;

            return PartialView(companyOpportunities);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(int currentOpportunityId, Opportunity opportunities)
        {
            Opportunity currentOpportunity = new();
            currentOpportunity = await _context.Opportunities.FindAsync(currentOpportunityId);
            int? groupNumber = 0;


            var opCount = opportunities.OpportunitiesRelated.Where(o => o.Checked == true).Count();

            if (opCount == 0)
            {
                groupNumber = null;
            }
            else if (currentOpportunity.OpportunityGroup != null)
            {
                groupNumber = currentOpportunity.OpportunityGroup;
            }
            else
            {
                groupNumber = _context.Opportunities.Max(o => o.OpportunityGroup) + 1;
                if (groupNumber == null)
                {
                    groupNumber = 1;
                }
            }
            currentOpportunity.OpportunityGroup = groupNumber;
            _context.Update(currentOpportunity);
            foreach (var item in opportunities.OpportunitiesRelated)
            {
                Opportunity opportunityTemp = new();
                opportunityTemp = await _context.Opportunities.FindAsync(item.Id);
                opportunityTemp.OpportunityGroup = item.Checked ? groupNumber : null;
                _context.Update(opportunityTemp);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Opportunities", new { id = currentOpportunity.Id });
        }

        // GET: Opportunities/Duplicate
        public async Task<IActionResult> Duplicate(int opportunityId)
        {
            Opportunity opportunityToDuplicate = new Opportunity();
            opportunityToDuplicate = await _context.Opportunities.FindAsync(opportunityId);
            if (opportunityToDuplicate.OpportunityGroup == null)
            {
                opportunityToDuplicate.OpportunityGroup = _context.Opportunities.Max(o => o.OpportunityGroup) + 1;
            }
            CreateViewData(false, opportunityToDuplicate);
            return View(opportunityToDuplicate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(int opportunityToDuplicate,
            [Bind(BIND_STRING)] Opportunity opportunity)
        {
            var actionsToDuplicate = await _context.OpportunityActions.Where(o => o.OpportunityId == opportunityToDuplicate).ToListAsync();


            var opportunityToUpdate = await _context.Opportunities.FindAsync(opportunityToDuplicate);
            opportunityToUpdate.OpportunityGroup = opportunity.OpportunityGroup;
            if (ModelState.IsValid)
            {
                _context.Update(opportunityToUpdate);
                _context.Add(opportunity);

                await _context.SaveChangesAsync();
                foreach (var item in actionsToDuplicate)
                {
                    OpportunityAction action = new()
                    {
                        Date = item.Date,
                        Description = item.Description,
                        ActionTypeId = item.ActionTypeId,
                        ErpUserId = item.ErpUserId,
                        PersonId = item.PersonId,
                        Id = item.Id,
                        Status = item.Status,
                        OpportunityId = opportunity.Id
                    };
                    _context.Add(action);
                }
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Companies", new { id = opportunity.CompanyId });
            }
            CreateViewData(false, opportunity);
            return View(opportunity);
        }
        public JsonResult GetProductList(int categoryId, int? selectedProduct)
        {
            List<Product> productList = (from products in _context.Products
                                         where products.CategoryId == categoryId
                                         select products).ToList();

            productList.Insert(0, new Product { Id = 0, Name = "Selecione" });

            return Json(new SelectList(productList, "Id", "Name", selectedProduct));
        }

        [GeneratedRegex("\\b(?:(?:https?|ftp):\\/\\/|www\\.)[^\\s]+")]
        private static partial Regex ConvertUrl();

        //Get
        public async Task<IActionResult> CancelOpportunity(int id)
        {
            var opportunity = await _context.Opportunities
                .Include(x => x.OpportunityActions)
                .Include(x => x.Company)
                .Include(x => x.Product)
                    .ThenInclude(x => x.Category)
                .Include(x => x.Manager)
                .Include(x => x.Proposals)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (opportunity == null)
            {
                return NotFound();
            }

            return PartialView(opportunity);
        }
        //Get
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOpportunity(int id, string cancelReason)
        {
            var opportunity = await _context.Opportunities.FindAsync(id);
            if (opportunity.Notes != null)
            {
                opportunity.Notes += " ";
            }
            opportunity.Notes += "Motivo do cancelamento: " + cancelReason;

            var plannedActions = await _context.OpportunityActions.Where(x => x.OpportunityId == id && x.Status == OpportunityAction.ActionStatus.Planned).ToListAsync();
            foreach (var action in plannedActions)
            {
                action.Status = OpportunityAction.ActionStatus.Canceled;
                action.Description += $" (Oportunidade cancelada em {new DateTimeFunctions().GetNow()})";
            }
            var proposals = await _context.Proposals.Where(x => x.OpportunityId == id && x.Status == Proposal.ProposalStatus.Opened).ToListAsync();
            foreach (var proposal in proposals)
            {
                proposal.Status = Proposal.ProposalStatus.Canceled;
            }

            _context.UpdateRange(plannedActions);
            _context.Update(opportunity);
            _context.UpdateRange(proposals);
            opportunity.Status = Opportunity.OpportunityStatus.Canceled;
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction("Details", "Opportunities", new { id });
        }
        private void CreateViewData(bool activeUsers, Opportunity opportunity = null)
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "FriendlyName", opportunity?.CompanyId);
            ViewData["PipelineId"] = new SelectList(_context.Pipelines, "Id", "Stage", opportunity?.PipelineId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(p => p.CategoryName), "Id", "Name", opportunity?.CategoryId);
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name", opportunity?.ProductId);
            if (activeUsers)
            {
                ViewData["ErpUserId"] = new SelectList(_context.ErpUsers.Where(e => e.Active == true).OrderBy(e => e.Name), "Id", "Name");
            }
            else
            {
                ViewData["ErpUserId"] = new SelectList(_context.ErpUsers.OrderBy(e => e.Name), "Id", "Name");
            }
        }

    }
}
