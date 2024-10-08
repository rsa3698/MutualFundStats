using ClosedXML.Excel;
using ExcelReader.DTO;


namespace ExcelReader
{
    public class MutualFundExcelReader
    {
        public SchemeDataDTO GetSchemeData(string schemeName, Stream fileStream)
        {
            //if (schemeName == "Dynamic Asset Allocation/Balanced Advantage Fund")
            //{
            //    schemeName = "Dynamic Asset Allocation/Balanced Advantage";
            //}
            //if (schemeName == "Multi Asset Allocation Fund")
            //{
            //    schemeName = "Multi Asset Allocation";
            //}
            //if (schemeName == "Equity Savings Fund")
            //{
            //    schemeName = "Equity Savings";
            //}

            using (var workbook = new XLWorkbook(fileStream))
            {
                var worksheet = workbook.Worksheet(1); // Assuming the data is in the first sheet

                // Iterate through rows to find the scheme name
                foreach (var row in worksheet.RowsUsed())
                {
                    var currentSchemeName = row.Cell(2).GetString(); // Scheme Name in Column 2 (B)

                    if (currentSchemeName.Equals(schemeName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract data from the row where scheme name matches
                        return new SchemeDataDTO
                        {
                            SchemeName = currentSchemeName,
                            NumberOfSchemes = row.Cell(3).GetValue<int>(),       // Column 3 (C)
                            NumberOfFolios = row.Cell(4).GetValue<int>(),        // Column 4 (D)
                            FundsMobilized = row.Cell(5).GetValue<decimal>(),   // Column 4 (E)
                            Redemption = row.Cell(6).GetValue<decimal>(),       // Column 6 (F)
                            NetInflow = row.Cell(7).GetValue<decimal>(),        // Column 7 (G)
                            AUM = row.Cell(8).GetValue<decimal>(),              // Column 8 (H)
                            AverageAUM = row.Cell(9).GetValue<decimal>()        // Column 9 (I)
                        };
                    }
                }
            }

            // If no match is found
            return null;
        }
    }
}
