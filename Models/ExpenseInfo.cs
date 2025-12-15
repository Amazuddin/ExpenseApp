using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ExpenseApp.Models
{
    public class ExpenseInfo
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("ExpenseHeadId")]
        [Required(ErrorMessage = "Please select the Expense Head")]
        public ExpenseHead ExpenseHead { get; set; }
        public int ExpenseHeadId { get; set; }

        [Display(Name = "Expense Date")]
        [DisplayFormat(DataFormatString = "{0:dd-MMMM-yyyy}")]
        public DateTime ExpenseDate { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Please enter the Expense Amount")]
        [Display(Name = "Expense Amount")]
        public decimal ExpenseAmount { get; set; }
    }
}
