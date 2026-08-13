using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace PTS_Apparel.Models
{
    public class Forecast
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Factory")]
        [Required]
        public string FactoryName { get; set; }

        [Display(Name = "Style Code")]
        [Required]
        public string StyleCode { get; set; }

        [Display(Name = "Line No")]
        [Required]
        [Column("LineNo")] 
        public string LineNo { get; set; }

        [Display(Name = "Forecast Date")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime ForecastDate { get; set; }

        [Display(Name = "Forecast Qty")]
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal ForecastQty { get; set; }
    }
}