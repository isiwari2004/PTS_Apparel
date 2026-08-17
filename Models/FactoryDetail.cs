using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PTS_Apparel.Models
{
    public class FactoryDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FactoryMasterId { get; set; }

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

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonIgnore]
        public virtual FactoryMaster FactoryMaster { get; set; }
    }
}