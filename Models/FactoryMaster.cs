using System.ComponentModel.DataAnnotations;

namespace PTS_Apparel.Models
{
    public class FactoryMaster
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Factory Code")]
        [Required(ErrorMessage = "Factory Code is required")]
        public string FactoryCode { get; set; }

        [Display(Name = "Factory Name")]
        [Required(ErrorMessage = "Factory Name is required")]
        public string FactoryName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}