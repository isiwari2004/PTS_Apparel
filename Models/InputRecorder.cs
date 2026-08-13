using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PTS_Apparel.Models
{
    public class InputRecorder
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Factory")]
        [Required(ErrorMessage = "Factory is required")]
        public string FactoryName { get; set; }

        [Display(Name = "Line No")]
        [Required(ErrorMessage = "Line No is required")]
        public string LineNo { get; set; }

        [Display(Name = "PO No")]
        [Required(ErrorMessage = "PO No is required")]
        public string PONo { get; set; }

        [Display(Name = "Style No")]
        [Required(ErrorMessage = "Style No is required")]
        public string StyleNo { get; set; }

        [Display(Name = "Colour")]
        [Required(ErrorMessage = "Colour is required")]
        public string Colour { get; set; }

        [Display(Name = "Record Date")]
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime RecordDate { get; set; }

        [Display(Name = "Status")]
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        [Display(Name = "Sizes Json")]
        [Required(ErrorMessage = "Sizes data is required")]
        public string SizesJson { get; set; }

        [Display(Name = "Total")]
        [Required(ErrorMessage = "Total is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Total must be a positive number")]
        public int Total { get; set; }

        // Helper method to parse JSON into Dictionary
        public Dictionary<string, int> GetSizes()
        {
            if (string.IsNullOrEmpty(SizesJson)) return new Dictionary<string, int>();
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, int>>(SizesJson);
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }
    }
}