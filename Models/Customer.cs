using System.ComponentModel.DataAnnotations;

namespace PTS_Apparel.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Customer Name")]
        [Required(ErrorMessage = "Customer Name is required")]
        public string CustomerName { get; set; }

        [Display(Name = "Customer Type")]
        [Required(ErrorMessage = "Customer Type is required")]
        public string CustomerType { get; set; }
    }
}