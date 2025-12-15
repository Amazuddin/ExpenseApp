using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseApp.Models
{
    public class LoanPayment
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("LoanId")]
        [Required]
        public Loan Loan { get; set; }
        public long LoanId { get; set; }

        [Required(ErrorMessage = "Please enter the payment amount")]
        [Display(Name = "Payment Amount")]
        public decimal PaymentAmount { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        [DisplayFormat(DataFormatString = "{0:dd-MMMM-yyyy}")]
        public DateTime PaymentDate { get; set; }

        public string? Note { get; set; }
    }
}
