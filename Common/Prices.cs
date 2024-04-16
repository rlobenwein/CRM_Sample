using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using CRM_Sample.Data;
using CRM_Sample.Models.SalesModels;
using CRM_Sample.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM_Sample.Common
{
    public class Prices
    {
        public readonly IExchangeRateService _exchangeRate;
        private readonly IMemoryCache _cache;

        public Prices(IMemoryCache cache)
        {
            _cache = cache;
            _exchangeRate = new ExchangeRateService(_cache);
        }
        public static decimal SetOpportunityValue(Opportunity opportunity)
        {
            decimal opportunityValue = 0;
            foreach (var proposal in opportunity.Proposals)
            {
                if (proposal.Status == Proposal.ProposalStatus.Opened || proposal.Status == Proposal.ProposalStatus.Sold)
                {
                    opportunityValue += proposal.Price;
                }
            }
            return opportunityValue;
        }
        public Opportunity CalcOpportunityValue(Opportunity opportunity, Proposal proposal, bool delete)
        {
            if (opportunity.Proposals.Any(x => x.Id == proposal.Id))
            {
                List<Proposal> allProposals = new();

                foreach (var item in opportunity.Proposals)
                {
                    allProposals.Add(item);
                }
                var index = allProposals.FindIndex(x => x.Id == proposal.Id);
                if (delete)
                {
                    allProposals.RemoveAt(index);
                }
                else
                {
                    allProposals[index] = proposal;
                }
                opportunity.Proposals.Clear();
                opportunity.Proposals = allProposals;
            }
            else
            {
                opportunity.Proposals.Add(proposal);
            }

            opportunity.Value = SetOpportunityValue(opportunity);

            return opportunity;
        }
        public async Task<Opportunity> CalcOpportunityValueAsync(ProposalProduct proposalProduct, ApplicationDbContext context, bool delete)
        {
            proposalProduct.Discount /= 100;
            if (!delete)
            {
                SetProposalProductPrices(proposalProduct);
            }

            proposalProduct.Product = context.Products.Find(proposalProduct.ProductId);
            Proposal proposal = context.Proposals.Include(x => x.Products).ThenInclude(x => x.Product).FirstOrDefault(x => x.Id == proposalProduct.ProposalId);
            await UpdateProposalPrices(proposal, proposalProduct, delete);

            Opportunity opportunity = context.Opportunities
                .Include(x => x.Proposals)
                .FirstOrDefault(x => x.Id == proposal.OpportunityId);
            CalcOpportunityValue(opportunity, proposal, false);

            return opportunity;
        }
        public async Task<Opportunity> CalcOpportunityValueAsync(Proposal proposal, ApplicationDbContext context, bool delete)
        {
            proposal.Discount /= 100;
            if (!delete)
            {
                await SetProposalPricesAsync(proposal);
            }

            Opportunity opportunity = context.Opportunities
                .Include(x => x.Proposals)
                    .ThenInclude(x => x.Products)
                .FirstOrDefault(x => x.Id == proposal.OpportunityId);
            CalcOpportunityValue(opportunity, proposal, delete);

            return opportunity;
        }
        public async Task<Proposal> UpdateProposalPrices(Proposal proposal, ProposalProduct proposalProduct, bool delete)
        {
            if (proposal.Products.Any(x => x.Id == proposalProduct.Id))
            {
                List<ProposalProduct> allProducts = new();
                foreach (var item in proposal.Products)
                {
                    allProducts.Add(item);
                }
                var index = allProducts.FindIndex(x => x.Id == proposalProduct.Id);
                if (delete)
                {
                    allProducts.RemoveAt(index);
                }
                else
                {
                    allProducts[index] = proposalProduct;
                }
                proposal.Products.Clear();
                proposal.Products = allProducts;
            }
            else
            {
                proposal.Products.Add(proposalProduct);
            }

            proposal = await SetProposalPricesAsync(proposal);
            return proposal;

        }
        public ProposalProduct UpdateProposalProductPrices(ProposalProduct proposalProduct, bool delete)
        {
            SetProposalProductPrices(proposalProduct);
            return proposalProduct;
        }
        public async Task<Proposal> SetProposalPricesAsync(Proposal proposal)
        {
            if (!proposal.ManualPrice && proposal.Products != null)
            {
                proposal.BasePrice = 0;

                foreach (var product in proposal.Products)
                {
                    decimal conversion;
                    if (product.Product != null)
                    {
                        conversion = await GetConvertionFactor(proposal.Date, (Currency.Currencies)product.Product.Currency, proposal.Currency);
                    }
                    else
                    {
                        conversion = 1;
                    }
                    proposal.BasePrice += product.Price * conversion;
                }
            }
            proposal.Price = Math.Round(proposal.BasePrice * (1 - proposal.Discount));
            proposal.ExchangeRate = await GetExchangeRate(proposal.Date, proposal.Currency);

            proposal.PriceBrl = proposal.Price * proposal.ExchangeRate;

            return proposal;
        }
        public static ProposalProduct SetProposalProductPrices(ProposalProduct proposalProduct)
        {
            proposalProduct.Price = proposalProduct.BasePrice * (1 - proposalProduct.Discount);
            return proposalProduct;
        }
        private async Task<decimal> GetConvertionFactor(DateTime date, Currency.Currencies currencyFrom, Currency.Currencies currencyTo)
        {
            ExchangeRateViewModel rate = await GetExchangeData(date, currencyFrom, currencyTo);

            if (currencyTo == Currency.Currencies.BRL)
            {
                return rate.ExchangeSale;
            }
            if (currencyFrom == Currency.Currencies.BRL)
            {
                return 1 / rate.ExchangeSale;
            }

            if (currencyFrom == Currency.Currencies.USD && currencyTo == Currency.Currencies.EUR)
            {
                return 1 / rate.ParitySale;
            }
            if (currencyFrom == Currency.Currencies.EUR && currencyTo == Currency.Currencies.USD)
            {
                return rate.ParitySale;
            }
            else
            {
                return 1;
            }
        }
        public async Task<decimal> GetExchangeRate(DateTime date, Currency.Currencies currency)
        {
            if (currency == Currency.Currencies.BRL)
            {
                return 1;
            }
            var strCurrency = currency.ToString();
            var i = 0;
            decimal exchangeRate;
            do
            {
                exchangeRate = (await _exchangeRate.GetFullExchangeData((date.AddDays(i--).Date).ToString("MM-dd-yyyy"), strCurrency)).ExchangeSale;
            } while (exchangeRate == 0);

            return exchangeRate;
        }
        public async Task<ExchangeRateViewModel> GetExchangeData(DateTime date, Currency.Currencies currencyFrom, Currency.Currencies currencyTo)
        {
            ExchangeRateViewModel rate = new();
            if (currencyFrom == currencyTo)
            {
                rate.ExchangeSale = 1;
                rate.ExchangePurchase = 1;
                rate.ExchangeDateTime = null;
                rate.Bulletin = "Não encontrado";
                return rate;
            }
            string currencyToCheck;
            if (currencyFrom == Currency.Currencies.BRL)
            {
                currencyToCheck = currencyTo.ToString();
            }
            else
            {
                currencyToCheck = currencyFrom.ToString();
            }
            var strCurrencyFrom = currencyToCheck;
            var i = 0;
            do
            {
                rate = (await _exchangeRate.GetFullExchangeData((date.AddDays(i--).Date).ToString("MM-dd-yyyy"), strCurrencyFrom));
            } while (_exchangeRate == null);

            return rate;
        }
    }
}
