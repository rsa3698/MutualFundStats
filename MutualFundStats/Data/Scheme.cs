using Microsoft.EntityFrameworkCore;
using MutualFundStats.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MutualFundStats.Data
{
    public class Scheme
    {
        // Primary Key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SchemeId { get; set; }

        // Properties
        [Required]
        [Key]
        public string SchemeName { get; set; }
        [Required]
        public string Category { get; set; }

        // Navigation Property for Monthly_Scheme_Stats
        public ICollection<MutualFundData> MutualFundDatas { get; set; }
    }
}
