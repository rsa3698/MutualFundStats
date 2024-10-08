using System;
namespace ExcelReader.DTO
{
    public class SchemeDataDTO
    {
        public string SchemeName { get; set; }
        public int NumberOfSchemes { get; set; }
        public int NumberOfFolios { get; set; }
        public DateTime Month {  get; set; }
        public decimal FundsMobilized { get; set; }
        public decimal Redemption { get; set; }
        public decimal NetInflow { get; set; }
        public decimal AUM { get; set; }
        public decimal AverageAUM { get; set; }
    }
}
