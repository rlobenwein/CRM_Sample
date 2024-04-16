using CRM_Sample.Models.CRMModels;
using CRM_Sample.Models.CustomerModels;
using CRM_Sample.Models.LocationModels;
using CRM_Sample.Models.SalesModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRM_Sample.Data
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
        public DbSet<CompanyEmployee> CompanyEmployees { get; set; }

        //LocationsController Tables
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }

        //Sales Tables
        public DbSet<Pipeline> Pipelines { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<OpportunityAction> OpportunityActions { get; set; }
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<ProposalProduct> ProposalProducts { get; set; }

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
            modelBuilder.Entity<Person>()
                .ToTable("People");
            modelBuilder.Entity<Person>()
                .HasIndex(p => p.TaxpayerNumber)
                .IsUnique();
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
            modelBuilder.Entity<Category>().ToTable("Categories");
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
        }


    }
}
