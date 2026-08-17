using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PTS_Apparel.Models
{
    public class StyleMaster
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Customer Name")]
        [Required(ErrorMessage = "Customer is required")]
        public string CustomerName { get; set; }

        [Display(Name = "Style Code")]
        [Required(ErrorMessage = "Style Code is required")]
        public string StyleCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<StyleDetail> StyleDetails { get; set; }
    }
}