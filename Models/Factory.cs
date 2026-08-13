using System.ComponentModel.DataAnnotations;

namespace PTS_Apparel.Models
{
    public class Factory
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Factory Code")]
        [Required(ErrorMessage = "Factory Code is required")]
        public string FactoryCode { get; set; }

        [Display(Name = "Factory Name")]
        [Required(ErrorMessage = "Factory Name is required")]
        public string FactoryName { get; set; }

        [Display(Name = "Working Hours")]
        [Required(ErrorMessage = "Working Hours is required")]
        [Range(1, 24, ErrorMessage = "Working Hours must be between 1 and 24")]
        public int WorkingHours { get; set; }

     
        [Display(Name = "Cycle Time")]
        [Required(ErrorMessage = "Cycle Time is required")]
        [Range(0.1, 1000, ErrorMessage = "Invalid Cycle Time")]
        public decimal CycleTime { get; set; }

        [Display(Name = "Prod. Lines")]
        [Required(ErrorMessage = "Production Lines is required")]
        [Range(1, 100, ErrorMessage = "Invalid number")]
        public int ProdLines { get; set; }
    }
}