using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RLBW_ERP.Common;
using RLBW_ERP.Data;
using RLBW_ERP.Models.SalesModels;

namespace RLBW_ERP.Controllers.SalesControllers
{
    public class ProposalsController : Controller
    {
        private const string BIND_STRING = "Id,Id,Revision,Date,ExpirationTime,Currency,BasePrice,Discount,Notes,DelireryTime,Status,ManualPrice";
        private readonly ApplicationDbContext _context;
        private readonly DateTimeFunctions _now;
        //private readonly IExchangeRateService _exchangeRate;
        private readonly IMemoryCache _cache;

        public ProposalsController(ApplicationDbContext context, DateTimeFunctions now, IMemoryCache cache)
        {
            _context = context;
            _now = now;
            _cache = cache;
            //_exchangeRate = new ExchangeRateService(_cache);
        }

        // GET: Proposals
        public async Task<IActionResult> Index(string status,
            string sortOrder,
            string searchstring,
            string currentFilter,
            int? pageNumber,
            int? pageSize)
        {

            ViewData["CurrentSort"] = sortOrder;
            ViewData["ProposalNumSortParm"] = sortOrder == "Id" ? "ProposalId_desc" : "Id";
            ViewData["OpportunityIdSortParm"] = sortOrder == "Id" ? "OpportunityId_desc" : "Id";
            ViewData["ProposalDateSortParm"] = sortOrder == "ProposalDate" ? "ProposalDate_desc" : "ProposalDate";
            ViewData["CompanyNameSortParm"] = sortOrder == "CompanyName" ? "CompanyName_desc" : "CompanyName";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "Price_desc" : "Price";
            ViewData["CurrencySortParm"] = sortOrder == "Currency" ? "Currency_desc" : "Currency";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "Status_desc" : "Status";

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

            var proposals = from o in _context.Proposals
                .Include(o => o.Opportunity)
                    .ThenInclude(x => x.Company)
                .Include(x => x.Products)
                .AsNoTracking()
                            select o;

            switch (status)
            {
                case "Opened":
                    proposals = proposals.Where(o => o.Status == Proposal.ProposalStatus.Opened);
                    break;
                case "Sold":
                    proposals = proposals.Where(o => o.Status == Proposal.ProposalStatus.Sold);
                    break;
                case "Revised":
                    proposals = proposals.Where(o => o.Status == Proposal.ProposalStatus.Revised);
                    break;
                case "Canceled":
                    proposals = proposals.Where(o => o.Status == Proposal.ProposalStatus.Canceled);
                    break;
                case "Lost":
                    proposals = proposals.Where(o => o.Status == Proposal.ProposalStatus.Lost);
                    break;
                case "Old":
                    proposals = proposals.Where(o => o.Status == Proposal.ProposalStatus.Old);
                    break;
                case "Declined":
                    proposals = proposals.Where(o => o.Status == Proposal.ProposalStatus.Declined);
                    break;
            }

            if (!string.IsNullOrEmpty(searchstring))
            {
                proposals = proposals.Where(a =>
                    a.Opportunity.Company.FriendlyName.Contains(searchstring));
            }

            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "ProposalDate_desc";
            }

            proposals = sortOrder switch
            {
                "OpportunityId" => proposals.OrderBy(s => s.OpportunityId),
                "OpportunityId_desc" => proposals.OrderByDescending(s => s.OpportunityId),
                "ProposalId" => proposals.OrderBy(s => s.Id),
                "ProposalId_desc" => proposals.OrderByDescending(s => s.Id),
                "ProposalDate" => proposals.OrderBy(s => s.Date).ThenBy(x => x.Id),
                "ProposalDate_desc" => proposals.OrderByDescending(s => s.Date).ThenByDescending(x => x.Id),
                "CompanyName" => proposals.OrderBy(s => s.Opportunity.Company.FriendlyName),
                "CompanyName_desc" => proposals.OrderByDescending(s => s.Opportunity.Company.FriendlyName),
                "Currency" => proposals.OrderBy(s => s.Currency),
                "Currency_desc" => proposals.OrderByDescending(s => s.Currency),
                "Price" => proposals.OrderBy(s => s.Price),
                "Price_desc" => proposals.OrderByDescending(s => s.Price),
                "Status" => proposals.OrderBy(s => s.Status),
                "Status_desc" => proposals.OrderByDescending(s => s.Status),
                _ => proposals.OrderByDescending(s => s.Date),
            };
            return View(await PaginetedList<Proposal>.CreateAsync(proposals.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Proposals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proposal = await _context.Proposals
                .Include(p => p.Opportunity)
                .Include(p => p.Products)
                    .ThenInclude(x => x.Category)
                .Include(x => x.Products)
                    .ThenInclude(x => x.Product)
                .Include(x => x.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (proposal == null)
            {
                return NotFound();
            }
            proposal.Discount *= 100;
            return PartialView(proposal);
        }

        // GET: Proposals/Create
        public IActionResult Create(int opportunityId, int? proposalId)
        {
            var proposalRevised = proposalId == null ? null : _context.Proposals.Find(proposalId);
            var opportunity = _context.Opportunities.Find(opportunityId);
            if (opportunity == null)
            {
                return NotFound();
            }
            var model = new Proposal()
            {
                Id = opportunityId,
                OpportunityId = opportunity.Id,
                Date = _now.GetNow().Date,
                Revision = proposalRevised?.Id,
                ExpirationTime = 15,
                Currency = proposalRevised != null ? proposalRevised.Currency : Currency.Currencies.BRL,
                BasePrice = proposalRevised != null ? proposalRevised.BasePrice : 0,
                Discount = proposalRevised != null ? proposalRevised.Discount * 100 : 0,
                DelireryTime = proposalRevised != null ? proposalRevised.DelireryTime : 0,
                Price = proposalRevised != null ? proposalRevised.Price : 0,
                PriceBrl = proposalRevised != null ? proposalRevised.PriceBrl : 0,
                ManualPrice = proposalRevised != null ? proposalRevised.ManualPrice : false,
                Status = Proposal.ProposalStatus.Opened,
                Products = proposalRevised != null ? _context.ProposalProducts.Where(x => x.ProposalId == proposalId).ToList() : null
            };
            ViewData["OpportunityTitle"] = opportunity.Title;
            return PartialView(model);
        }

        // POST: Proposals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] Proposal proposal)
        {
            if (ModelState.IsValid)
            {
                if (proposal.Revision != null)
                {
                    proposal.Products = await _context.ProposalProducts
                        .Include(x => x.Subproducts)
                            .ThenInclude(x => x.Optionals)
                        .Include(x => x.Subproducts)
                            .ThenInclude(x => x.Params)
                        .Where(x => x.ProposalId == proposal.Revision).ToListAsync();
                    ReviewProposal((int)proposal.Revision);
                }
                var calcPrices = new Prices(_cache);
                Opportunity opportunity = await calcPrices.CalcOpportunityValue(proposal, _context, false);

                _context.Update(opportunity);
                _context.Add(proposal);
                await _context.SaveChangesAsync();

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }
            ViewData["Id"] = new SelectList(_context.Opportunities, "Id", "Id", proposal.OpportunityId);
            return PartialView(proposal);
        }

        // GET: Proposals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proposal = await _context.Proposals.FindAsync(id);
            var opportunity = await _context.Opportunities.FindAsync(proposal.OpportunityId);
            if (proposal == null)
            {
                return NotFound();
            }
            proposal.Discount *= 100;

            var opportunities = await (from o in _context.Opportunities
                                       .Where(x => x.CompanyId == proposal.Opportunity.CompanyId)
                                       .Include(x => x.Product).ThenInclude(x => x.Category)
                                       select new
                                       {
                                           o.Id,
                                           o.Title,
                                           o.Product.Category.CategoryName,
                                           o.Product.Name,
                                           DisplayField = string.Format("{0} ({1})", o.Id, o.Title ?? o.Product.Category.CategoryName + o.Product.Name)
                                       }).ToListAsync();

            ViewData["Id"] = new SelectList(opportunities, "Id", "DisplayField", proposal.OpportunityId);
            ViewData["OpportunityTitle"] = opportunity.Title;

            return PartialView(proposal);
        }

        // POST: Proposals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] Proposal proposal)
        {
            if (id != proposal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var calcPrices = new Prices(_cache);
                proposal.Discount /= 100;
                proposal.Products = await _context.ProposalProducts
                    .Include(x => x.Product)
                    .Include(x => x.Subproducts)
                        .ThenInclude(x => x.Params)
                    .Include(x => x.Subproducts)
                        .ThenInclude(x => x.Optionals)
                    .Where(x => x.ProposalId == proposal.Id)
                    .ToListAsync();
                proposal.ExchangeRate = await calcPrices.GetExchangeRate(proposal.Date, proposal.Currency);
                proposal = await calcPrices.SetProposalPricesAsync(proposal);


                //Opportunity opportunity = await calcPrices.CalcOpportunityValue(proposal, _context, false);
                //var checkProposal = opportunity.Proposals.FirstOrDefault(x=>x.Id==proposal.Id);
                //var products=checkProposal.Products.Count;
                //await TryUpdateModelAsync<Proposal>(proposal);

                try
                {
                    //_context.Update(opportunity);
                    _context.Update(proposal);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProposalExists(proposal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction("Details", "Opportunities", new { id = proposal.OpportunityId });
            }
            ViewData["Id"] = new SelectList(_context.Opportunities, "Id", "Id", proposal.OpportunityId);
            return PartialView(proposal);
        }

        // GET: Proposals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proposal = await _context.Proposals
                .Include(p => p.Opportunity)
                .Include(x => x.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            proposal.Discount *= 100;
            if (proposal == null)
            {
                return NotFound();
            }

            return PartialView(proposal);
        }

        // POST: Proposals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proposal = await _context.Proposals.FindAsync(id);
            var calcPrices = new Prices(_cache);
            Opportunity opportunity = await calcPrices.CalcOpportunityValue(proposal, _context, true);
            _context.Update(opportunity);

            _context.Proposals.Remove(proposal);
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
        }

        private bool ProposalExists(int id)
        {
            return _context.Proposals.Any(e => e.Id == id);
        }
        private async void ReviewProposal(int proposalId)
        {
            var proposalRevised = await _context.Proposals.FindAsync(proposalId);
            proposalRevised.Status = Proposal.ProposalStatus.Revised;
            _context.Update(proposalRevised);
        }
    }

}
