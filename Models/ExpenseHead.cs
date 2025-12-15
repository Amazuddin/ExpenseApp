using System.ComponentModel.DataAnnotations;

namespace ExpenseApp.Models
{
    public class ExpenseHead
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the Expense Head")]
        public string Name { get; set; }
    }
}
