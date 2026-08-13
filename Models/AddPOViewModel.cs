using System.ComponentModel.DataAnnotations;

namespace PTS_Apparel.Models
{
    public class AddPOViewModel
    {
        [Required]
        [Display(Name = "PO Number")]
        public string PONumber { get; set; }

        [Required]
        [Display(Name = "Style")]
        public string StyleCode { get; set; }

        [Required]
        [Display(Name = "Colour")]
        public string ColorCode { get; set; }

        [Required]
        [Display(Name = "Tolerance (%)")]
        [Range(0, 100, ErrorMessage = "Tolerance must be between 0 and 100")]
        public decimal Tolerance { get; set; } = 5; // Default 5%

        public List<POSizeDetail> SizeDetails { get; set; } = new List<POSizeDetail>();
    }

    public class POSizeDetail
    {
        public string SizeName { get; set; }
        public int OrderQty { get; set; }
        public int ToleranceQty { get; set; }
        public int FinalQuantity { get; set; }
    }
}