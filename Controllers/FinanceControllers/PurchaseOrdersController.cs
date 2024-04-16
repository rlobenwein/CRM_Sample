using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NuGet.Packaging;
using RLBW_ERP.Common;
using RLBW_ERP.Data;
using RLBW_ERP.Models.FinanceModels;
using RLBW_ERP.Models.SalesModels;
using RLBW_ERP.Models.ViewModels;

namespace RLBW_ERP.Controllers.FinanceControllers
{
    public class PurchaseOrdersController : Controller
    {
        private const string BIND_STRING = "Id,PONumber,Date,Id,Contract,Value,Currency,DeliveryTime,PaymentTermId,Description,DirectInvoicing,ExchangeRate,ValueBRL,POStatus,Notes,Id";
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public PurchaseOrdersController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: PurchaseOrders
        public async Task<IActionResult> Index(string status,
            string sortOrder,
            string searchstring,
            string currentFilter,
            int? pageNumber,
            int? pageSize)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["OpportunityIdSortParm"] = sortOrder == "Id" ? "OpportunityId_desc" : "Id";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "Date_desc" : "Date";
            ViewData["CompanyNameSortParm"] = sortOrder == "CompanyName" ? "CompanyName_desc" : "CompanyName";
            ViewData["PONumberSortParm"] = sortOrder == "PONumber" ? "PONumber_desc" : "PONumber";
            ViewData["ValueSortParm"] = sortOrder == "Value" ? "Value_desc" : "Value";
            ViewData["ContractSortParm"] = sortOrder == "Contract" ? "Contract_desc" : "Contract";
            ViewData["ProposalSortParm"] = sortOrder == "Proposal" ? "Proposal_desc" : "Proposal";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "Status_desc" : "Status";
            ViewData["CurrencySortParm"] = sortOrder == "Currency" ? "Currency_desc" : "Currency";

            pageSize ??= 25;

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

            var model = from po in _context.PurchaseOrders
                .Include(x => x.Opportunity)
                    .ThenInclude(x => x.Company)
                    .AsNoTracking()
                        select po;

            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(a =>

                    a.Opportunity.Company.FriendlyName.Contains(searchstring) ||
                    a.PONumber.Contains(searchstring) ||
                    a.Opportunity.Title.Contains(searchstring)
                    );
            }
            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "OpportunityId_desc";
            }
            model = sortOrder switch
            {
                "Id" => model.OrderBy(s => s.OpportunityId),
                "OpportunityId_desc" => model.OrderByDescending(s => s.OpportunityId),
                "Date" => model.OrderBy(s => s.Date),
                "Date_desc" => model.OrderByDescending(s => s.Date),
                "CompanyName" => model.OrderBy(s => s.Opportunity.Company.FriendlyName),
                "CompanyName_desc" => model.OrderByDescending(s => s.Opportunity.Company.FriendlyName),
                "PONumber" => model.OrderBy(s => s.PONumber),
                "PONumber_desc" => model.OrderByDescending(s => s.PONumber),
                "Proposal" => model.OrderBy(s => s.ProposalId),
                "Proposal_desc" => model.OrderByDescending(s => s.ProposalId),
                "Contract" => model.OrderBy(s => s.Contract),
                "Contract_desc" => model.OrderByDescending(s => s.Contract),
                "Value" => model.OrderBy(s => s.Value),
                "Value_desc" => model.OrderByDescending(s => s.Value),
                "Status" => model.OrderBy(s => s.POStatus),
                "Status_desc" => model.OrderByDescending(s => s.POStatus),
                "Currency" => model.OrderBy(s => s.Currency),
                "Currency_desc" => model.OrderByDescending(s => s.Currency),
                _ => model.OrderByDescending(s => s.Date),
            };
            return View(await PaginetedList<PurchaseOrder>.CreateAsync(model.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: PurchaseOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PurchaseOrders == null)
            {
                return NotFound();
            }

            PurchaseOrderViewModel purchaseOrder = new()
            {
                PO = await _context.PurchaseOrders.FindAsync(id)
            };
            purchaseOrder.PONumber = purchaseOrder.PO.PONumber;
            purchaseOrder.Contract = purchaseOrder.PO.Contract;
            purchaseOrder.Value = purchaseOrder.PO.Value;
            purchaseOrder.ExchangeRate = purchaseOrder.PO.ExchangeRate;
            purchaseOrder.ValueBRL = purchaseOrder.PO.ValueBRL;
            purchaseOrder.Proposal = await _context.Proposals.FindAsync(purchaseOrder.PO.ProposalId);
            purchaseOrder.Opportunity = await _context.Opportunities.FindAsync(purchaseOrder.Proposal.OpportunityId);
            purchaseOrder.Opportunity.Company = await _context.Companies.FindAsync(purchaseOrder.Opportunity.CompanyId);
            purchaseOrder.DeliveryTime = purchaseOrder.PO.DeliveryTime;
            purchaseOrder.DirectInvoicing = purchaseOrder.PO.DirectInvoicing;
            purchaseOrder.Description = purchaseOrder.PO.Description;
            purchaseOrder.Notes = purchaseOrder.PO.Notes;
            purchaseOrder.PaymentTerms = await _context.PaymentTerms.FindAsync(purchaseOrder.PO.PaymentTermId);
            purchaseOrder.Products = await _context.ProposalProducts
                .Include(x => x.Product)
                .Include(x => x.Category)
                .Include(x => x.Subproducts)
                    .ThenInclude(x => x.Optionals)
                .Include(x => x.Subproducts)
                    .ThenInclude(x => x.Params)
                .Where(x => x.ProposalId == purchaseOrder.PO.ProposalId).ToListAsync();
            purchaseOrder.Subproducts = new List<SubproductsList>();
            foreach (var item in purchaseOrder.Products)
            {
                purchaseOrder.Subproducts.AddRange(_context.SubproductsLists.Where(x => x.ProposalProductId == item.Id).Include(x => x.SubProduct));
            }

            purchaseOrder.Optionals = new List<OptionalsPackage>();
            foreach (var item in purchaseOrder.Subproducts)
            {
                purchaseOrder.Optionals.AddRange(_context.OptionalsPackages.Where(x => x.SubproductsListId == item.Id).Include(x => x.LicenseOptional));
            }

            purchaseOrder.Params = new List<SoftwareParams>();
            foreach (var item in purchaseOrder.Subproducts)
            {
                purchaseOrder.Params.AddRange(_context.SoftwareParams.Where(x => x.SubproductsListId == item.Id)
                    .Include(x => x.LicenseNetworkType)
                    .Include(x => x.LicenseTime)
                    .Include(x => x.CommercialLicense));
            }

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            return View(purchaseOrder);
        }

        // GET: PurchaseOrders/Create
        public async Task<IActionResult> Create(int proposalId)
        {
            var proposal = _context.Proposals.Find(proposalId);
            var prices = new Prices(_cache);
            var excRate = await prices.GetExchangeRate(new DateTimeFunctions().GetNow(), proposal.Currency);
            var purchaseOrder = new PurchaseOrder()
            {
                OpportunityId = proposal.OpportunityId,
                Date = new DateTimeFunctions().GetNow(),
                ProposalId = proposalId,
                Value = proposal.Price,
                ExchangeRate = excRate,
                Currency = proposal.Currency,
                ValueBRL = proposal.Price * excRate,
                POStatus = PurchaseOrder.Status.opened,
                DirectInvoicing = true
            };
            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();
            ViewData["PaymentTerm"] = new SelectList(_context.PaymentTerms, "Id", "Name", purchaseOrder.PaymentTermId);

            return PartialView(purchaseOrder);
        }

        // POST: PurchaseOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] PurchaseOrder purchaseOrder)
        {
            if (ModelState.IsValid)
            {
                var opportunityId = _context.Proposals.Find(purchaseOrder.ProposalId).OpportunityId;
                var opportunity = _context.Opportunities.Include(x => x.OpportunityActions).FirstOrDefault(x => x.Id == opportunityId);
                UpdateProposalStatus((int)purchaseOrder.ProposalId);
                CreateActionForPOReceived(opportunity, purchaseOrder.PONumber, purchaseOrder.DeliveryTime);
                UpdateOpportunity(opportunity, purchaseOrder.Value);
                _context.Add(purchaseOrder);
                await _context.SaveChangesAsync();

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction("Details", "Opportunities", new { id = opportunityId });
            }
            ViewData["Id"] = new SelectList(_context.Proposals, "Id", "Id", purchaseOrder.ProposalId);
            ViewData["PaymentTerm"] = new SelectList(_context.PaymentTerms, "Id", "Name", purchaseOrder.PaymentTermId);
            return View(purchaseOrder);
        }

        // GET: PurchaseOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PurchaseOrders == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(x => x.Opportunity)
                .ThenInclude(x => x.Product)
                .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            var opportunities = await (from o in _context.Opportunities
                           .Where(x => x.CompanyId == purchaseOrder.Opportunity.CompanyId)
                           .Include(x => x.Product).ThenInclude(x => x.Category)
                                       select new
                                       {
                                           o.Id,
                                           o.Title,
                                           o.Product.Category.CategoryName,
                                           o.Product.Name,
                                           DisplayField = string.Format("{0} ({1} {2})", o.Id, o.Title ?? o.Product.Category.CategoryName, o.Product.Name)
                                       }).ToListAsync();

            var proposals = await _context.Proposals.Where(x => x.OpportunityId == purchaseOrder.OpportunityId).ToListAsync();

            ViewData["Id"] = new SelectList(opportunities, "Id", "DisplayField", purchaseOrder.OpportunityId);
            ViewData["Id"] = new SelectList(proposals, "Id", "Id", purchaseOrder.ProposalId);
            ViewData["PaymentTerm"] = new SelectList(_context.PaymentTerms, "Id", "Name", purchaseOrder.PaymentTermId);
            return View(purchaseOrder);
        }

        // POST: PurchaseOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] PurchaseOrder purchaseOrder)
        {
            if (id != purchaseOrder.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchaseOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseOrderExists(purchaseOrder.Id))
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

                return RedirectToAction("Details", "Opportunities", new { id = purchaseOrder.OpportunityId });
            }
            var proposals = await _context.Proposals.Where(x => x.OpportunityId == purchaseOrder.OpportunityId).ToListAsync();

            ViewData["Id"] = new SelectList(proposals, "Id", "Id", purchaseOrder.ProposalId);
            ViewData["PaymentTerm"] = new SelectList(_context.PaymentTerms, "Id", "Name", purchaseOrder.PaymentTermId);
            return View(purchaseOrder);
        }

        // GET: PurchaseOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PurchaseOrders == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(p => p.Proposal)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            return View(purchaseOrder);
        }

        // POST: PurchaseOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PurchaseOrders == null)
            {
                return Problem("Entity set 'ApplicationDbContext.PurchaseOrders'  is null.");
            }
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder != null)
            {
                _context.PurchaseOrders.Remove(purchaseOrder);
            }
            CancelAutomatedPOActions(purchaseOrder);
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction("Details", "Opportunities", new { id = purchaseOrder.OpportunityId });
        }

        private void CancelAutomatedPOActions(PurchaseOrder po)
        {
            var actions = _context.OpportunityActions
                .Where(
                x => x.OpportunityId == po.OpportunityId &&
                (x.Description.Contains($"Pedido de Compras recebido (PO {po.PONumber})") || x.Description == $"Entrega prevista. Acompanhar")).ToList();

            foreach (var action in actions)
            {
                action.Status = OpportunityAction.ActionStatus.Canceled;
                action.Description += $" (Pedido excluído em {new DateTimeFunctions().GetNow()})";
            }
            _context.UpdateRange(actions);
        }

        private bool PurchaseOrderExists(int id)
        {
            return _context.PurchaseOrders.Any(e => e.Id == id);
        }
        public void UpdateProposalStatus(int proposalId)
        {
            var proposal = _context.Proposals.Find(proposalId);
            var otherProposals = _context.Proposals.Where(x => x.OpportunityId == proposal.OpportunityId && x.Id != proposalId).ToList();
            foreach (var otherProposal in otherProposals)
            {
                otherProposal.Status = otherProposal.Status == Proposal.ProposalStatus.Opened ? Proposal.ProposalStatus.Canceled : otherProposal.Status;
            }
            proposal.Status = Proposal.ProposalStatus.Sold;
            _context.UpdateRange(otherProposals);
            _context.Update(proposal);
        }
        public void UpdateOpportunity(Opportunity opportunity, decimal newPOValue)
        {
            opportunity.Status = Opportunity.OpportunityStatus.Sold;
            opportunity.Id = 6;
            var purchaseOrders = _context.PurchaseOrders.Where(x => x.OpportunityId == opportunity.Id);
            decimal value = newPOValue;
            foreach (var po in purchaseOrders)
            {
                value += po.ValueBRL;
            }
            opportunity.Value = value;
            _context.Update(opportunity);
        }
        public void CreateActionForPOReceived(Opportunity opportunity, string purchaseOrderNumber, int deliveryTime)
        {

            var userId = opportunity.ErpUserId;
            var poAction = new OpportunityAction()
            {
                Date = new DateTimeFunctions().GetNow(),
                OpportunityId = opportunity.Id,
                ErpUserId = userId,
                ActionTypeId = 15,
                Description = $"Pedido de Compras recebido (PO {purchaseOrderNumber})",
                PipelineId = 6,
                Status = OpportunityAction.ActionStatus.Done
            };
            var deliveryAction = new OpportunityAction()
            {
                Date = new DateTimeFunctions().GetNow().AddDays(deliveryTime),
                OpportunityId = opportunity.Id,
                ErpUserId = userId,
                ActionTypeId = 8,
                Description = $"Entrega prevista. Acompanhar",
                PipelineId = 7,
                Status = OpportunityAction.ActionStatus.Planned
            };
            opportunity.OpportunityActions.Add(poAction);
            opportunity.OpportunityActions.Add(deliveryAction);
            _context.Update(opportunity);
        }
    }
}
