using System.ComponentModel.DataAnnotations;

namespace PTS_Apparel.Models
{
    public class Style
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Customer Name")]
        [Required(ErrorMessage = "Customer is required")]
        public string CustomerName { get; set; }

        [Display(Name = "Style Code")]
        [Required(ErrorMessage = "Style Code is required")]
        public string StyleCode { get; set; }

        [Display(Name = "Color Code")]
        [Required(ErrorMessage = "Color Code is required")]
        public string ColorCode { get; set; }

        [Display(Name = "Sizes")]
        [Required(ErrorMessage = "Sizes are required")]
        public string Sizes { get; set; } 
    }
}