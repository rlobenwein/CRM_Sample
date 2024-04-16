using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RLBW_ERP.Data;
using RLBW_ERP.Models.SalesModels;
using RLBW_ERP.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RLBW_ERP.Common
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

        public async Task<Opportunity> CalcOpportunityValueAsync(SoftwareParams softwareParams, ApplicationDbContext context, bool delete)
        {
            SubproductsList subproductsListItem = softwareParams.SubproductsList = context.SubproductsLists
                .Include(x => x.ProposalProduct)
                .Include(x => x.Optionals)
                .Include(x => x.Params)
                .FirstOrDefault(x => x.Id == softwareParams.SubproductsListId);

            if (!delete)
            {
                softwareParams.Seats = (int)softwareParams.SubproductsList.Quantity;
                softwareParams.Coefficient = SetParamsMultiplier(softwareParams, (int)softwareParams.SubproductsList.Quantity);
                subproductsListItem.Params.Clear();
                subproductsListItem.Params.Add(softwareParams);
            }
            else { subproductsListItem.Params.Clear(); }

            UpdateOptionalsPackagePrices(subproductsListItem.Optionals, softwareParams);

            subproductsListItem.BasePrice = context.SubProducts.FirstOrDefault(x => x.Id == subproductsListItem.SubproductId).Price;
            UpdateSubproductsListPrices(subproductsListItem);

            ProposalProduct proposalProduct = context.ProposalProducts
                .Include(x => x.Subproducts)
                .FirstOrDefault(x => x.Id == subproductsListItem.ProposalProductId);
            UpdateProposalProductPrices(proposalProduct, subproductsListItem, false);

            Proposal proposal = context.Proposals.Include(x => x.Products).ThenInclude(x => x.Product).FirstOrDefault(x => x.Id == proposalProduct.ProposalId);
            await UpdateProposalPrices(proposal, proposalProduct, false);

            Opportunity opportunity = context.Opportunities
                .Include(x => x.Proposals)
                .FirstOrDefault(x => x.Id == proposal.OpportunityId);
            CalcOpportunityValue(opportunity, proposal, false);

            return opportunity;
        }
        public async Task<Opportunity> CalcOpportunityValue(Proposal proposal, ApplicationDbContext context, bool delete)
        {
            proposal.Discount /= 100;
            if (!delete)
            {
                await SetProposalPricesAsync(proposal);
            }

            Opportunity opportunity = context.Opportunities
                .Include(x => x.Proposals)
                    .ThenInclude(x => x.Products)
                        .ThenInclude(x => x.Subproducts)
                            .ThenInclude(x => x.Params)
                .Include(x => x.Proposals)
                    .ThenInclude(x => x.Products)
                        .ThenInclude(x => x.Subproducts)
                            .ThenInclude(x => x.Optionals)
                .FirstOrDefault(x => x.Id == proposal.OpportunityId);
            CalcOpportunityValue(opportunity, proposal, delete);

            return opportunity;
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
        public ProposalProduct UpdateProposalProductPrices(ProposalProduct proposalProduct, SubproductsList subproductsListItem, bool delete)
        {
            if (proposalProduct.Subproducts.Any(x => x.Id == subproductsListItem.Id))
            {
                List<SubproductsList> allSubproducts = new();
                foreach (var item in proposalProduct.Subproducts)
                {
                    allSubproducts.Add(item);
                }
                var index = allSubproducts.FindIndex(x => x.Id == subproductsListItem.Id);
                if (delete)
                {
                    allSubproducts.RemoveAt(index);
                }
                else
                {
                    allSubproducts[index] = subproductsListItem;
                }
                proposalProduct.Subproducts.Clear();
                proposalProduct.Subproducts = allSubproducts;
            }
            else
            {
                proposalProduct.Subproducts.Add(subproductsListItem);
            }

            SetProposalProductPrices(proposalProduct);
            return proposalProduct;
        }
        public SubproductsList UpdateSubproductsListPrices(SubproductsList subproductsListItem, OptionalsPackage optional, bool delete)
        {
            if (subproductsListItem.Optionals.Any(x => x.OptionalId == optional.OptionalId && x.SubproductsListId == optional.SubproductsListId))
            {
                List<OptionalsPackage> allOptionals = new();
                foreach (var item in subproductsListItem.Optionals)
                {
                    allOptionals.Add(item);
                }
                var index = allOptionals.FindIndex(x => x.OptionalId == optional.OptionalId && x.SubproductsListId == optional.SubproductsListId);
                if (delete)
                {
                    allOptionals.RemoveAt(index);
                }
                else
                {
                    allOptionals[index] = optional;
                }
                subproductsListItem.Optionals.Clear();
                subproductsListItem.Optionals = allOptionals;
            }
            else
            {
                subproductsListItem.Optionals.Add(optional);
            }

            SetSubproductListItemPrices(subproductsListItem);
            return subproductsListItem;
        }
        public SubproductsList UpdateSubproductsListPrices(SubproductsList subproductsListItem, OptionalsPackage optional, bool delete, bool edit)
        {
            if (subproductsListItem.Optionals.Any(x => x.OptionalId == optional.OptionalId && x.SubproductsListId == optional.SubproductsListId))
            {
                List<OptionalsPackage> allOptionals = new();
                foreach (var item in subproductsListItem.Optionals)
                {
                    allOptionals.Add(item);
                }
                var index = allOptionals.FindIndex(x => x.OptionalId == optional.OptionalId && x.SubproductsListId == optional.SubproductsListId);
                if (delete)
                {
                    allOptionals.RemoveAt(index);
                }
                else if (!edit)
                {
                    allOptionals[index] = optional;
                }
                subproductsListItem.Optionals.Clear();
                subproductsListItem.Optionals = allOptionals;
            }
            else
            {
                subproductsListItem.Optionals.Add(optional);
            }

            SetSubproductListItemPrices(subproductsListItem);
            return subproductsListItem;
        }

        public SubproductsList UpdateSubproductsListPrices(SubproductsList subproductsListItem)
        {
            return SetSubproductListItemPrices(subproductsListItem);
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



        public static ProposalProduct SetProposalProductPrices(ProposalProduct proposalProduct)
        {

            if (!proposalProduct.ManualPrice && proposalProduct.Subproducts != null)
            {
                proposalProduct.BasePrice = 0;

                foreach (var subproduct in proposalProduct.Subproducts)
                {
                    proposalProduct.BasePrice += subproduct.Price;
                }
            }
            proposalProduct.Price = proposalProduct.BasePrice * (1 - proposalProduct.Discount);
            return proposalProduct;
        }

        public SubproductsList SetSubproductListItemPrices(SubproductsList subproductsListItem)
        {
            decimal coeff = 1;
            int software = subproductsListItem.SubProduct.ProductId;
            if (subproductsListItem.Params != null && subproductsListItem.Params.Count > 0)
            {
                coeff = SetParamsMultiplier(subproductsListItem.Params.FirstOrDefault(), (int)subproductsListItem.Quantity, software);
            }
            var basePrice = subproductsListItem.BasePrice * coeff;
            if (software == 1)
            {
                subproductsListItem.Price = basePrice * (1 - subproductsListItem.Discount) * subproductsListItem.Quantity;
            }
            else
            {
                subproductsListItem.Price = basePrice * (1 - subproductsListItem.Discount);
            }

            if (subproductsListItem.Optionals != null)
            {
                foreach (var optional in subproductsListItem.Optionals)
                {
                    subproductsListItem.Price += optional.Price;
                }
            }

            return subproductsListItem;
        }

        public ICollection<OptionalsPackage> UpdateOptionalsPackagePrices(ICollection<OptionalsPackage> optionals, SoftwareParams softwareParams)
        {
            List<OptionalsPackage> allOptionals = new();
            foreach (var item in optionals)
            {
                allOptionals.Add(SetOptionalsPrice(item, softwareParams));
            }
            return allOptionals;
        }

        public OptionalsPackage SetOptionalsPrice(OptionalsPackage optional, SoftwareParams softwareParams)
        {

            var seatsCoeff = SetParamsMultiplier(softwareParams, optional.Quantity);
            optional.Price = optional.BasePrice * (1 - optional.Discount) * seatsCoeff;
            return optional;
        }
        public OptionalsPackage SetOptionalsPrice(OptionalsPackage optional, SoftwareParams softwareParams, int software)
        {
            var seatsCoeff = SetParamsMultiplier(softwareParams, optional.Quantity, software);
            optional.Price = optional.BasePrice * (1 - optional.Discount) * seatsCoeff;
            return optional;
        }
        public List<OptionalsPackage> SetOptionalsPrice(List<OptionalsPackage> optionals, SoftwareParams softwareParams, int software, ApplicationDbContext context)
        {
            decimal coeff = 1;
            if (softwareParams != null)
            {
                coeff = SetParamsMultiplier(softwareParams, 1, software);
            }
            optionals = GetJmatproOptionalsPrice(optionals, coeff, context);
            return optionals;
        }
        public decimal SetParamsMultiplier(SoftwareParams softwareParams, int seats)
        {
            int software = softwareParams.SubproductsList.ProposalProduct.ProductId;

            int networkType = (int)softwareParams.LicenseNetworkId;
            int _academicType = (int)softwareParams.CommercialLicenseId;
            int timeType = (int)softwareParams.LicenseTimeId;
            decimal timeQuantity = (decimal)softwareParams.TimeQuantity;
            int cores = (int)softwareParams.Cores;

            int tasks = (int)softwareParams.Tasks;
            int workbenchs = (int)softwareParams.Workbenchs;

            decimal timeCoeff = SetTimeCoefficient(software, timeType, timeQuantity);
            decimal coresCoef = SetCoresCoefficient(software, cores);
            decimal tasksCoeff = SetTasksCoefficient(software, tasks);
            decimal workbenchsCoeff = SetWorkbenchCoefficient(software, networkType, workbenchs);

            decimal networkCoeff = SetNetworkCoefficient(software, networkType, seats);
            decimal academicCoeff = SetAcademicCoefficient(software, _academicType);

            decimal multiplier = (1 + coresCoef + workbenchsCoeff + tasksCoeff) * timeCoeff * academicCoeff * networkCoeff;

            return multiplier;
        }

        public decimal SetParamsMultiplier(SoftwareParams softwareParams, int seats, int software)
        {
            int networkType = (int)softwareParams.LicenseNetworkId;
            int _academicType = (int)softwareParams.CommercialLicenseId;
            int timeType = (int)softwareParams.LicenseTimeId;
            decimal timeQuantity = (decimal)softwareParams.TimeQuantity;
            int cores = (int)softwareParams.Cores;

            int tasks = (int)softwareParams.Tasks;
            int workbenchs = (int)softwareParams.Workbenchs;

            decimal timeCoeff = SetTimeCoefficient(software, timeType, timeQuantity);
            decimal coresCoef = SetCoresCoefficient(software, cores);
            decimal tasksCoeff = SetTasksCoefficient(software, tasks);
            decimal workbenchsCoeff = SetWorkbenchCoefficient(software, networkType, workbenchs);

            decimal networkCoeff = SetNetworkCoefficient(software, networkType, seats);
            decimal academicCoeff = SetAcademicCoefficient(software, _academicType);

            decimal multiplier = (1 + coresCoef + workbenchsCoeff + tasksCoeff) * timeCoeff * academicCoeff * networkCoeff;

            return multiplier;
        }

        public decimal SetNetworkCoefficient(int software, int networkType, int seats)
        {
            if (software == 4) return JmatproOptionalsQuantityCoefficient(seats);
            if (software != 2) return 0;

            switch (networkType)
            {
                case 1: //Local
                    return seats;
                case 2: //Flutuante
                    if (seats == 1)
                    {
                        return 1.10m;
                    }
                    else
                    {
                        return 1 + (0.2m * seats);
                    }
                case 3://Cliente-servidor
                    seats++;
                    return 1 + (0.2m * seats);
                case 4: //Cloud
                default:
                    return 0;
            }
        }

        public static decimal SetAcademicCoefficient(int software, int academicType) => academicType switch
        {
            //Academic
            2 => software == 2 ? 0.2m : 0.5m,
            //Non-profit
            3 => software == 1? 0.75m : 1,
            //Comercial
            _ => 1,
        };

        public static decimal SetTimeCoefficient(int software, int timeType, decimal timeQuantity)
        {
            switch (timeType)
            {
                case 1: //Anual
                    return 0.4m * timeQuantity;
                case 2: //Perpétua
                    return 1 + 0.15m * timeQuantity;
                case 3: //Manutenção
                case 4: //Atualização
                    var coef = software == 2 ? 0.15m : 0.2m;
                    return (coef * timeQuantity);
                default:
                    return 1;
            }
        }

        public static decimal SetCoresCoefficient(int software, int cores)
        {
            if (software != 2) return 0;

            if (cores >= 8) return 0.1m;

            if (cores >= 6) return (cores - 6) * 0.05m;

            if (cores >= 2) return (cores - 6) * 0.1m;

            return 0.4m;
        }

        public static decimal SetTasksCoefficient(int software, int tasks)
        {
            if (software != 2) return 0;

            if (tasks >= 6) return 1;

            return ((tasks - 1) * 0.2m);
        }

        public static decimal SetWorkbenchCoefficient(int software, int networkType, int workbenchs)
        {
            if (software != 2 || networkType == 1) return 0;

            return (0.05m * workbenchs);
        }

        public List<OptionalsPackage> GetJmatproOptionalsPrice(List<OptionalsPackage> optionals, decimal coeff, ApplicationDbContext context)
        {
            optionals = ManageJmatproPack(optionals);
            for (int i = 0; i < optionals.Count; i++)
            {
                optionals[i].BasePrice = context.LicenseOptionals.Find(optionals[i].OptionalId).Price;
            }

            optionals = SortJmatproOptionals(optionals);

            if (optionals.Count == 1)
            {
                optionals[0].Discount = optionals[0].Quantity - coeff;
                optionals[0].Price = optionals[0].BasePrice * (1 - optionals[0].Discount);
            }
            else if (optionals.Count == 2)
            {
                optionals[0].Discount = optionals[0].Quantity - coeff;
                optionals[0].Price = optionals[0].BasePrice * (1 - optionals[0].Discount);
                optionals[1].Discount = optionals[1].Quantity - coeff + 0.2m;
                optionals[1].Price = optionals[1].BasePrice * (1 - optionals[1].Discount);
            }
            else
            {
                optionals[0].Discount = optionals[0].Quantity - coeff;
                optionals[0].Price = optionals[0].BasePrice * (1 - optionals[0].Discount);
                optionals[1].Discount = optionals[1].Quantity - coeff + 0.2m;
                optionals[1].Price = optionals[1].BasePrice * (1 - optionals[1].Discount);
                for (int i = 2; i < optionals.Count; i++)
                {
                    optionals[i].Discount = optionals[i].Quantity - coeff + 0.3m;
                    optionals[i].Price = optionals[i].BasePrice * (1 - optionals[i].Discount);
                }
            }
            return optionals;
        }

        private static List<OptionalsPackage> ManageJmatproPack(List<OptionalsPackage> optionals)
        {
            List<OptionalsPackage> fullPack = new();

            foreach (var item in optionals)
            {
                fullPack.Add(item);
            }

            if (fullPack.Any(c => c.OptionalId == 16) && fullPack.Any(c => c.OptionalId == 17) && fullPack.Any(c => c.OptionalId == 18))
            {
                fullPack.Add(new OptionalsPackage { OptionalId = 19, Quantity = 1, Discount = fullPack.Find(c => c.OptionalId == 16).Discount, SubproductsListId = fullPack.First().SubproductsListId });
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 16));
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 17));
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 18));
            }
            if (fullPack.Any(c => c.OptionalId == 19))
            {
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 16));
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 17));
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 18));
            }
            if (fullPack.Any(c => c.OptionalId == 45))
            {
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 20));
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 21));
            }
            if (fullPack.Any(c => c.OptionalId == 20) && fullPack.Any(c => c.OptionalId == 21))
            {
                fullPack.Add(new OptionalsPackage { OptionalId = 45, Quantity = 1, Discount = fullPack.Find(c => c.OptionalId == 20).Discount, SubproductsListId = fullPack.First().SubproductsListId });
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 20));
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 21));
            }
            if (fullPack.Any(c => c.OptionalId == 44))
            {
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 22));
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 23));
            }
            if (fullPack.Any(c => c.OptionalId == 22) && fullPack.Any(c => c.OptionalId == 23))
            {
                fullPack.Add(new OptionalsPackage { OptionalId = 44, Quantity = 1, Discount = fullPack.Find(c => c.OptionalId == 22).Discount, SubproductsListId = fullPack.First().SubproductsListId });
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 22));
                fullPack.Remove(fullPack.Find(c => c.OptionalId == 23));
            }
            optionals = fullPack;
            return optionals;
        }

        public List<OptionalsPackage> SortJmatproOptionals(List<OptionalsPackage> optionals)
        {
            optionals = optionals.OrderByDescending(x => x.BasePrice).ToList();
            return optionals;
        }

        public decimal JmatproOptionalsQuantityCoefficient(int jmatproOptionalQuantity)
        {
            if (jmatproOptionalQuantity == 1)
            {
                return 1;
            }
            if (jmatproOptionalQuantity == 2)
            {
                return 1.6m;
            }
            if (jmatproOptionalQuantity == 3)
            {
                return 2.1m;
            }
            if (jmatproOptionalQuantity == 4)
            {
                return 2.5m;
            }
            if (jmatproOptionalQuantity >= 5)
            {
                return (2.5m + (jmatproOptionalQuantity - 4) * 0.3m);
            }
            return 0;
        }
    }
}
