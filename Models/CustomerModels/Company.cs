using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using CRM_Sample.Models.LocationModels;
using CRM_Sample.Models.CRMModels;
using CRM_Sample.Models.SalesModels;

namespace CRM_Sample.Models.CustomerModels
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome Amigável")]
        [StringLength(200)]
        public string FriendlyName { get; set; }

        [Display(Name = "Razão Social")]
        [StringLength(200)]
        public string CompanyName { get; set; }

        [Display(Name = "CNPJ")]
        [StringLength(30)]
        public string TaxpayerNumber { get; set; }

        [Display(Name = "Endereço")]
        [StringLength(200)]
        public string MainAddress { get; set; }

        [Display(Name = "Nº")]
        [StringLength(10)]
        public string AddressNumber { get; set; }

        [Display(Name = "Complemento")]
        [StringLength(200)]
        public string AddressComplement { get; set; }

        [Display(Name = "Bairro")]
        [StringLength(100)]
        public string AddressDistrict { get; set; }

        [Display(Name = "CEP")]
        [StringLength(15)]
        public string PostalCode { get; set; }

        [Display(Name = "Cidade")]
        public int? CityId { get; set; }

        [ForeignKey("CityId")]
        public City City { get; set; }

        [Display(Name = "Email Geral")]
        [DataType(DataType.EmailAddress)]
        [StringLength(100)]
        public string CompanyEmail { get; set; }

        [Display(Name = "Email Financeiro")]
        [DataType(DataType.EmailAddress)]
        [StringLength(100)]
        public string FinanceEmail { get; set; }

        [Display(Name = "Tel. Geral")]
        [StringLength(50)]
        public string CompanyPhone { get; set; }

        [Display(Name = "Cliente Ativo")]
        public bool Status { get; set; } = true;

        [Display(Name = "Última Atualização")]
        public DateTime LastUpdate { get; set; }

        [Display(Name = "Observações")]
        [StringLength(2000)]
        public string Notes { get; set; }

        [Display(Name = "Website")]
        [StringLength(500)]
        public string Website { get; set; }

        [Display(Name ="Oportunidades")]
        public ICollection<Opportunity> Opportunities { get; set; }
        public ICollection<CompanyEmployee> Employees { get; set; }
    }
}
