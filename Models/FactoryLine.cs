using System.ComponentModel.DataAnnotations;

namespace PTS_Apparel.Models
{
    public class FactoryLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FactoryMasterId { get; set; }

        [Display(Name = "Line Number")]
        [Required(ErrorMessage = "Line Number is required")]
        public int LineNumber { get; set; }

        [Display(Name = "Line Name")]
        [Required(ErrorMessage = "Line Name is required")]
        public string LineName { get; set; }

      
        public DateTime CreatedAt { get; set; }
    }
}