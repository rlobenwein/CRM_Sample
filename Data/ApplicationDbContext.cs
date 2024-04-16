using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RLBW_ERP.Models.CustomerModels;
using RLBW_ERP.Models.CRMModels;
using RLBW_ERP.Models.LocationModels;
using RLBW_ERP.Models.SalesModels;
using RLBW_ERP.Models.FinanceModels;

namespace RLBW_ERP.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        //CRM Tables
        public DbSet<ActionType> ActionTypes { get; set; }
        public DbSet<ErpUser> ErpUsers { get; set; }

        // Company Tables
        public DbSet<Person> People { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Software> Software { get; set; }
        public DbSet<CompanyBusiness> CompanyBusinesses { get; set; }
        public DbSet<SoftwareCompany> SoftwareCompanies { get; set; }
        public DbSet<CompanyEmployee> CompanyEmployees { get; set; }

        //LocationsController Tables
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }

        //Sales Tables
        public DbSet<Pipeline> Pipelines { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubProduct> SubProducts { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<OpportunityAction> OpportunityActions { get; set; }
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<LicenseTime> LicenseTimes { get; set; }
        public DbSet<CommercialLicense> CommercialLicenses { get; set; }
        public DbSet<LicenseNetworkType> LicenseNetworkTypes { get; set; }
        public DbSet<LicenseOptional> LicenseOptionals { get; set; }
        public DbSet<ProposalProduct> ProposalProducts { get; set; }
        public DbSet<SubproductsList> SubproductsLists { get; set; }
        public DbSet<OptionalsPackage> OptionalsPackages { get; set; }
        public DbSet<SoftwareParams> SoftwareParams { get; set; }

        //Finance Tables
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PaymentTerm> PaymentTerms { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CostCenter> CostCenters { get; set; }
        public DbSet<TransactionCategory> TransactionCategories { get; set; }
        public DbSet<TransactionSubcategory> TransactionSubcategories { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<TransactionDistribution> TransactionDistributions { get; set; }
        public DbSet<PaymentAccount> PaymentAccounts { get; set; }
        public DbSet<Budget> Budgets { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CRM Models
            modelBuilder.Entity<ActionType>()
                .ToTable("ActionTypes")
                .HasIndex(a => a.Name)
                .IsUnique();
            modelBuilder.Entity<ErpUser>()
                .ToTable("ErpUsers")
                .HasIndex(e => e.Name)
                .IsUnique();

            // Company Models
            modelBuilder.Entity<Company>()
                .ToTable("Companies")
                .HasIndex(c => c.TaxpayerNumber)
                .IsUnique();
            modelBuilder.Entity<Company>()
                .HasIndex(c => c.CompanyName)
                .IsUnique();
            modelBuilder.Entity<Field>()
                .ToTable("Fields")
                .HasIndex(f => f.Name)
                .IsUnique();
            modelBuilder.Entity<Person>()
                .ToTable("People");
            modelBuilder.Entity<Person>()
                .HasIndex(p => p.TaxpayerNumber)
                .IsUnique();
            modelBuilder.Entity<Software>()
                .ToTable("Software")
                .HasIndex(s => s.Name)
                .IsUnique();
            modelBuilder.Entity<SoftwareCompany>()
                .ToTable("SoftwareCompany")
                .HasKey(s => new { s.SoftwareId, s.CompanyId });
            modelBuilder.Entity<CompanyBusiness>()
                .ToTable("Businesses")
                .HasKey(f => new { f.CompanyId, f.FieldId });
            modelBuilder.Entity<CompanyEmployee>()
                .ToTable("Employees")
                .HasKey(f => new { f.CompanyId, f.PersonId });

            //LocationsController Models

            modelBuilder.Entity<City>().ToTable("Cities");
            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<State>().ToTable("States");

            //Sales Models
            modelBuilder.Entity<Pipeline>().ToTable("Pipelines");
            modelBuilder.Entity<Opportunity>().ToTable("Opportunities");
            modelBuilder.Entity<Opportunity>().Property(p => p.Value).HasPrecision(15, 2);
            modelBuilder.Entity<OpportunityAction>().ToTable("OpportunityActions");
            modelBuilder.Entity<Product>()
                .ToTable("Products")
                .HasIndex(p => p.Name)
                .IsUnique();
            modelBuilder.Entity<SubProduct>().ToTable("Subproducts");
            modelBuilder.Entity<SubProduct>().Property(p => p.Price).HasPrecision(15, 2);
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<LicenseTime>().ToTable("LicenseTimes");
            modelBuilder.Entity<LicenseNetworkType>().ToTable("LicenseNetworkTypes");
            modelBuilder.Entity<LicenseOptional>().Property(p => p.Price).HasPrecision(15, 2);
            modelBuilder.Entity<LicenseOptional>()
                .ToTable("LicenseOptional")
                .HasIndex("OptionalName")
                .IsUnique();
            modelBuilder.Entity<CommercialLicense>().ToTable("CommercialLicenses");
            modelBuilder.Entity<Proposal>().ToTable("Proposals");
            modelBuilder.Entity<Proposal>().Property(p => p.Discount).HasPrecision(5, 4);
            modelBuilder.Entity<Proposal>().Property(p => p.BasePrice).HasPrecision(15, 2);
            modelBuilder.Entity<Proposal>().Property(p => p.Price).HasPrecision(15, 2);
            modelBuilder.Entity<Proposal>().Property(p => p.ExchangeRate).HasPrecision(15, 4);
            modelBuilder.Entity<Proposal>().Property(p => p.ExchangeRate).HasDefaultValue(1);
            modelBuilder.Entity<Proposal>().Property(p => p.PriceBrl).HasPrecision(15, 2);

            modelBuilder.Entity<ProposalProduct>().ToTable("ProposalProducts");
            modelBuilder.Entity<ProposalProduct>().Property(p => p.Discount).HasPrecision(5, 4);
            modelBuilder.Entity<ProposalProduct>().Property(p => p.BasePrice).HasPrecision(15, 2);
            modelBuilder.Entity<ProposalProduct>().Property(p => p.Price).HasPrecision(15, 2);

            modelBuilder.Entity<SubproductsList>().ToTable("SubproductsLists");
            modelBuilder.Entity<SubproductsList>().Property(x => x.Quantity).HasPrecision(15, 2);
            modelBuilder.Entity<SubproductsList>().Property(x => x.BasePrice).HasPrecision(15, 2);
            modelBuilder.Entity<SubproductsList>().Property(x => x.Price).HasPrecision(15, 2);
            modelBuilder.Entity<SubproductsList>().Property(x => x.Discount).HasPrecision(5, 4);

            modelBuilder.Entity<SoftwareParams>().ToTable("SoftwareParams")
                .HasIndex("SubproductsListId")
                .IsUnique();
            modelBuilder.Entity<SoftwareParams>().Property(p => p.TimeQuantity).HasPrecision(15, 2);
            modelBuilder.Entity<SoftwareParams>().Property(p => p.Coefficient).HasPrecision(9, 4);

            modelBuilder.Entity<OptionalsPackage>()
                .ToTable("OptionalsPackages")
                .HasKey(o => new { o.SubproductsListId, o.OptionalId })
                ;
            modelBuilder.Entity<OptionalsPackage>().Property(p => p.BasePrice).HasPrecision(15, 2);
            modelBuilder.Entity<OptionalsPackage>().Property(p => p.Price).HasPrecision(15, 2);
            modelBuilder.Entity<OptionalsPackage>().Property(p => p.Discount).HasPrecision(5, 4);

            // Finance Models
            modelBuilder.Entity<PurchaseOrder>().ToTable("PurchaseOrders");
            modelBuilder.Entity<PurchaseOrder>().Property(p => p.Value).HasPrecision(15, 2);
            modelBuilder.Entity<PurchaseOrder>().Property(p => p.Value).HasDefaultValue(0);
            modelBuilder.Entity<PurchaseOrder>().Property(p => p.ValueBRL).HasPrecision(15, 2);
            modelBuilder.Entity<PurchaseOrder>().Property(p => p.ValueBRL).HasDefaultValue(0);
            modelBuilder.Entity<PurchaseOrder>().Property(p => p.ExchangeRate).HasPrecision(15, 4);
            modelBuilder.Entity<PurchaseOrder>().Property(p => p.ExchangeRate).HasDefaultValue(1);
            modelBuilder.Entity<PurchaseOrder>().Property(p => p.DeliveryTime).HasDefaultValue(0);
            modelBuilder.Entity<PurchaseOrder>().Property(p => p.DirectInvoicing).HasDefaultValue(true);

            modelBuilder.Entity<PaymentTerm>().ToTable("PaymentTerms");

            modelBuilder.Entity<CostCenter>().ToTable("CostsCenters").HasIndex("Id").IsUnique();
            modelBuilder.Entity<CostCenter>().Property(x => x.Status).HasDefaultValue(false);
            modelBuilder.Entity<CostCenter>().Property(x => x.IsActive).HasDefaultValue(true);

            modelBuilder.Entity<PaymentAccount>().ToTable("PaymentAccounts");

            modelBuilder.Entity<Transaction>().ToTable("Transactions");
            modelBuilder.Entity<Transaction>().Property(x => x.Value).HasPrecision(15, 2);
            modelBuilder.Entity<Transaction>().HasIndex("BankTransactionId").IsUnique();

            modelBuilder.Entity<TransactionCategory>().ToTable("TransactionCategories");
            modelBuilder.Entity<TransactionCategory>().Property(x => x.IsExpense).HasDefaultValue(true);

            modelBuilder.Entity<TransactionSubcategory>().ToTable("TransactionSubcategories");

            modelBuilder.Entity<TransactionType>().ToTable("TransactionTypes");

            modelBuilder.Entity<TransactionDistribution>().ToTable("TransactionDistributions");
            modelBuilder.Entity<TransactionDistribution>().Property(x => x.Proportion).HasPrecision(5, 4);
            modelBuilder.Entity<TransactionDistribution>().Property(x => x.Proportion).HasDefaultValue(1);

            
            modelBuilder.Entity<Budget>().ToTable("Budgets");
            modelBuilder.Entity<Budget>().Property(x=>x.TotalBudgeted).HasPrecision(15,2).HasDefaultValue(0);
        }


    }
}
