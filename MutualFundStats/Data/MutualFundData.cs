using MutualFundStats.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MutualFundStats.Models
{ 
    public class MutualFundData
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StatId { get; set; }
        [Required]
        public int SchemeId { get; set; }
        [Required]
        public int NumberOfSchemes { get; set; }
        [Required]
        public DateTime Month {  get; set; }
        [Required]
        public int NumberOfFolios { get; set; }
        [Required]
        public decimal FundsMobilized { get; set; } // In INR crore
        [Required]
        public decimal RepurchaseRedemption { get; set; } // In INR crore
        [Required]
        public decimal NetInflowOutflow { get; set; } // In INR crore
        [Required]
        public decimal NetAssetsUnderManagement { get; set; } // In INR crore
        [Required] 
        public decimal AverageNetAssetsUnderManagement { get; set; } // In INR crore

        // Navigation Property for the Scheme
        [ForeignKey("SchemeId")] // Specifies that SchemeId is the foreign key
        public Scheme Scheme { get; set; }
    }
}
