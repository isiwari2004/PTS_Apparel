using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PTS_Apparel.Models
{
    public class FactoryProductionLine
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("FactoryMaster")]
        public int FactoryMasterId { get; set; }

        public int LineNumber { get; set; }

        public string LineName { get; set; }

        // Navigation Property (Entity Framework සඳහා)
        public virtual FactoryMaster FactoryMaster { get; set; }
    }
}