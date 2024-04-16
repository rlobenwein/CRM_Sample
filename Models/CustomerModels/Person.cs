using CRM_Sample.Models.LocationModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Sample.Models.CustomerModels
{
    public class Person
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Primeiro nome")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Nome do meio")]
        [StringLength(50)]
        public string MiddleName { get; set; }

        [Display(Name = "Último nome")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Display(Name = "Nome")]
        public string FullName
        {
            get
            {
                if (MiddleName != null)
                {
                    return FirstName + " " + MiddleName + " " + LastName;
                }
                else
                {
                    return FirstName + " " + LastName;
                }
            }
        }

        [Display(Name ="CPF")]
        [StringLength(20)]
        public string TaxpayerNumber { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Data de Nascimento")]
        public DateTime? Birthday { get; set; }

        [Display(Name = "LinkedIn")]
        [DataType(DataType.Url)]
        [StringLength(1000)]
        public string LinkedinProfile { get; set; }

        [Display(Name = "Observações")]
        [DataType(DataType.MultilineText)]
        [StringLength(2000)]
        public string Notes { get; set; }

        [Required]
        [Display(Name = "Ativo")]
        public bool Status { get; set; } = true;

        [Display(Name = "Última atualização")]
        public DateTime LastUpdate { get; set; }

        [Display(Name ="Email Pessoal")]
        [DataType(DataType.EmailAddress)]
        [StringLength(100)]
        public string Email { get; set; }

        [Display(Name ="Tel. Pessoal")]

        [StringLength(20)]
        public string HomePhone { get; set; }

        [Display(Name ="Cel. Pessoal")]

        [StringLength(20)]
        public string CellPhone { get; set; }

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

        public ICollection<CompanyEmployee> CompanyEmployees { get; set; }

        public Person()
        {
            LastUpdate = DateTime.UtcNow;
        }

    }
}
