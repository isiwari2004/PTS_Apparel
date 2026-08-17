using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PTS_Apparel.Models
{
    public class StyleDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StyleMasterId { get; set; }

        [Display(Name = "Color Code")]
        [Required(ErrorMessage = "Color is required")]
        public string ColorCode { get; set; }

        [Display(Name = "Sizes")]
        [Required(ErrorMessage = "Sizes are required")]
        public string Sizes { get; set; } 
        
        public DateTime CreatedAt { get; set; }

        // Navigation Property
        [ForeignKey("StyleMasterId")]
        public virtual StyleMaster StyleMaster { get; set; }
    }
}