using System.ComponentModel.DataAnnotations;

namespace PTS_Apparel.Models
{
    public class ScreenPrivilege
    {
        [Key]
        public int Id { get; set; }
        public string Role { get; set; }
        public string ModuleName { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}