using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Sample.Models.CustomerModels
{
    public class CompanyEmployee
    {
        [Required(ErrorMessage ="Campo obrigatório")]
        [Display(Name ="Contato")]
        public int PersonId { get; set; }

        [Required(ErrorMessage ="Campo obrigatório")]
        [Display(Name ="Cliente")]
        public int CompanyId { get; set; }
        
        [ForeignKey("PersonId")]
        [Display(Name ="Contato")]
        public Person Person { get; set; }
        
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [Display(Name ="Cargo")]
        public string Position { get; set; }

        [Display(Name ="Departamento")]
        public string Department { get; set; }

        [Display(Name ="Data Inicial")]
        [DataType(DataType.Date)]
        public DateTime InitialDate { get; set; } 

        [Display(Name ="Data Final")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name ="Ativo")]
        public bool Status { get; set; } = true;

        [Display(Name ="Última Atualização")]
        public DateTime LastUpdate { get; set; }

        [Display(Name="Tel. Corporativo")]
        [StringLength(50)]
        public string WorkPhone { get; set; }

        [Display(Name="Cel. Corporativo")]
        [StringLength(20)]
        public string CellPhone { get; set; }

        [Display(Name="Email Corporativo")]
        [DataType(DataType.EmailAddress)]
        [StringLength(100)]
        public string WorkEmail { get; set; }

        [Display(Name="Observações")]
        [StringLength(2000)]
        public string Notes { get; set; }
    }
}
