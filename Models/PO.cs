using System.ComponentModel.DataAnnotations;

namespace PTS_Apparel.Models
{
    public class PO
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "PO Number")]
        [Required(ErrorMessage = "PO Number is required")]
        public string PONumber { get; set; }

        [Display(Name = "Customer")]
        [Required(ErrorMessage = "Customer is required")]
        public string Customer { get; set; }

        [Display(Name = "Style")]
        [Required(ErrorMessage = "Style is required")]
        public string Style { get; set; }

        [Display(Name = "Color")]
        [Required(ErrorMessage = "Color is required")]
        public string Color { get; set; }

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}