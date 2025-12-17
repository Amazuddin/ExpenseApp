using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseApp.Models
{
    public class Loan
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Please enter the lender's name")]
        [Display(Name = "Lender Name")]
        public string LenderName { get; set; }

        [Required(ErrorMessage = "Please enter the loan amount")]
        [Display(Name = "Loan Amount")]
        public decimal LoanAmount { get; set; }

        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }

        [NotMapped]
        [Display(Name = "Balance Due")]
        public decimal BalanceDue => LoanAmount - AmountPaid;

        [Required]
        [Display(Name = "Loan Date")]
        [DisplayFormat(DataFormatString = "{0:dd-MMMM-yyyy}")]
        public DateTime LoanDate { get; set; }

        [Display(Name = "Due Date")]
        [DisplayFormat(DataFormatString = "{0:dd-MMMM-yyyy}")]
        public DateTime? DueDate { get; set; }

        public string? Description { get; set; }

        [NotMapped]
        public bool IsFullyPaid => AmountPaid >= LoanAmount;

        [Display(Name = "Document")]
        public string? DocumentPath { get; set; }

        [NotMapped]
        public bool HasDocument => !string.IsNullOrEmpty(DocumentPath);
    }
}
