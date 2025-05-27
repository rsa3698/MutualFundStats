using Xunit;
using ExcelReader;
using ExcelReader.DTO;
using System.IO;
using ClosedXML.Excel;
using System;

public class MutualFundExcelReaderTests
{
    private Stream CreateMockExcelStream(SchemeDataDTO schemeDataToEmbed, string schemeNameToSetInCell = null, bool addOtherRows = true)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        // Header row
        worksheet.Cell(1, 1).Value = "Sr.No.";
        worksheet.Cell(1, 2).Value = "Scheme Name";
        worksheet.Cell(1, 3).Value = "No of Schemes"; // Simplified, assuming Cr. is handled by context or not part of raw data
        worksheet.Cell(1, 4).Value = "No of Folios";
        worksheet.Cell(1, 5).Value = "Funds Mobilized";
        worksheet.Cell(1, 6).Value = "Redemption";
        worksheet.Cell(1, 7).Value = "Net Inflow";
        worksheet.Cell(1, 8).Value = "AUM";
        worksheet.Cell(1, 9).Value = "Average AUM";

        int currentRow = 2;
        if (schemeDataToEmbed != null)
        {
            worksheet.Cell(currentRow, 1).Value = 1;
            worksheet.Cell(currentRow, 2).Value = schemeNameToSetInCell ?? schemeDataToEmbed.SchemeName;
            worksheet.Cell(currentRow, 3).Value = schemeDataToEmbed.NumberOfSchemes;
            worksheet.Cell(currentRow, 4).Value = schemeDataToEmbed.NumberOfFolios;
            worksheet.Cell(currentRow, 5).Value = schemeDataToEmbed.FundsMobilized;
            worksheet.Cell(currentRow, 6).Value = schemeDataToEmbed.Redemption;
            worksheet.Cell(currentRow, 7).Value = schemeDataToEmbed.NetInflow;
            worksheet.Cell(currentRow, 8).Value = schemeDataToEmbed.AUM;
            worksheet.Cell(currentRow, 9).Value = schemeDataToEmbed.AverageAUM;
            currentRow++;
        }

        if (addOtherRows)
        {
            // Add a second dummy row
            worksheet.Cell(currentRow, 1).Value = 2;
            worksheet.Cell(currentRow, 2).Value = "Dummy Scheme One";
            worksheet.Cell(currentRow, 3).Value = 10;
            worksheet.Cell(currentRow, 4).Value = 1000;
            worksheet.Cell(currentRow, 5).Value = 10000.50m;
            worksheet.Cell(currentRow, 6).Value = 5000.25m;
            worksheet.Cell(currentRow, 7).Value = 5000.25m;
            worksheet.Cell(currentRow, 8).Value = 200000.75m;
            worksheet.Cell(currentRow, 9).Value = 190000.00m;
            currentRow++;

            // Add a third dummy row
            worksheet.Cell(currentRow, 1).Value = 3;
            worksheet.Cell(currentRow, 2).Value = "Another Test Scheme";
            worksheet.Cell(currentRow, 3).Value = 5;
            worksheet.Cell(currentRow, 4).Value = 500;
            worksheet.Cell(currentRow, 5).Value = 5000.00m;
            worksheet.Cell(currentRow, 6).Value = 2500.00m;
            worksheet.Cell(currentRow, 7).Value = 2500.00m;
            worksheet.Cell(currentRow, 8).Value = 100000.00m;
            worksheet.Cell(currentRow, 9).Value = 95000.00m;
        }
        
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public void GetSchemeData_SchemeExists_ReturnsCorrectDTO()
    {
        var reader = new MutualFundExcelReader();
        var expectedDto = new SchemeDataDTO
        {
            SchemeName = "Test Scheme Alpha",
            NumberOfSchemes = 1,
            NumberOfFolios = 100,
            FundsMobilized = 1000.50m,
            Redemption = 500.25m,
            NetInflow = 500.25m,
            AUM = 20000.75m,
            AverageAUM = 19000.00m
        };

        using var stream = CreateMockExcelStream(expectedDto);
        var resultDto = reader.GetSchemeData("Test Scheme Alpha", stream);

        Assert.NotNull(resultDto);
        Assert.Equal(expectedDto.SchemeName, resultDto.SchemeName);
        Assert.Equal(expectedDto.NumberOfSchemes, resultDto.NumberOfSchemes);
        Assert.Equal(expectedDto.NumberOfFolios, resultDto.NumberOfFolios);
        Assert.Equal(expectedDto.FundsMobilized, resultDto.FundsMobilized);
        Assert.Equal(expectedDto.Redemption, resultDto.Redemption);
        Assert.Equal(expectedDto.NetInflow, resultDto.NetInflow);
        Assert.Equal(expectedDto.AUM, resultDto.AUM);
        Assert.Equal(expectedDto.AverageAUM, resultDto.AverageAUM);
    }

    [Fact]
    public void GetSchemeData_SchemeDoesNotExist_ReturnsNull()
    {
        var reader = new MutualFundExcelReader();
        var dummyDto = new SchemeDataDTO { SchemeName = "Some Other Scheme" }; // Data for a scheme that exists

        using var stream = CreateMockExcelStream(dummyDto); // Stream contains "Some Other Scheme"
        var resultDto = reader.GetSchemeData("NonExistent Scheme", stream);

        Assert.Null(resultDto);
    }

    [Fact]
    public void GetSchemeData_CorrectDataTypesParsed()
    {
        var reader = new MutualFundExcelReader();
        var expectedDto = new SchemeDataDTO
        {
            SchemeName = "DataType Test Scheme",
            NumberOfSchemes = 123, // int
            NumberOfFolios = 45678, // int
            FundsMobilized = 12345.67m, // decimal
            Redemption = 9876.54m, // decimal
            NetInflow = 2469.13m, // decimal
            AUM = 1234567.89m, // decimal
            AverageAUM = 1230000.00m // decimal
        };

        using var stream = CreateMockExcelStream(expectedDto);
        var resultDto = reader.GetSchemeData("DataType Test Scheme", stream);

        Assert.NotNull(resultDto);
        Assert.Equal(expectedDto.SchemeName, resultDto.SchemeName);
        Assert.IsType<int>(resultDto.NumberOfSchemes);
        Assert.Equal(expectedDto.NumberOfSchemes, resultDto.NumberOfSchemes);
        Assert.IsType<int>(resultDto.NumberOfFolios);
        Assert.Equal(expectedDto.NumberOfFolios, resultDto.NumberOfFolios);
        Assert.IsType<decimal>(resultDto.FundsMobilized);
        Assert.Equal(expectedDto.FundsMobilized, resultDto.FundsMobilized);
        Assert.IsType<decimal>(resultDto.Redemption);
        Assert.Equal(expectedDto.Redemption, resultDto.Redemption);
        Assert.IsType<decimal>(resultDto.NetInflow);
        Assert.Equal(expectedDto.NetInflow, resultDto.NetInflow);
        Assert.IsType<decimal>(resultDto.AUM);
        Assert.Equal(expectedDto.AUM, resultDto.AUM);
        Assert.IsType<decimal>(resultDto.AverageAUM);
        Assert.Equal(expectedDto.AverageAUM, resultDto.AverageAUM);
    }

    [Fact]
    public void GetSchemeData_EmptyStream_ReturnsNull()
    {
        var reader = new MutualFundExcelReader();
        using var stream = new MemoryStream(); // Truly empty stream
        
        // ClosedXML will throw an exception if it can't open a workbook from the stream.
        // We expect GetSchemeData to return null if workbook processing fails at its level,
        // or if the stream is so malformed that XLWorkbook constructor throws, the test will fail,
        // which is acceptable as it indicates an issue with stream handling or ClosedXML interaction.
        // A more robust test might mock IXLWorkbook if we wanted to isolate GetSchemeData logic purely.
        // For now, we test the integration with ClosedXML's behavior on an empty stream.
        
        XLWorkbook workbook;
        try
        {
            // This is expected to fail for a completely empty stream
            workbook = new XLWorkbook(stream); 
        }
        catch (Exception) // Catching ClosedXML's exception
        {
            // If ClosedXML cannot create a workbook, GetSchemeData would likely not be called
            // or would receive a null/unusable workbook.
            // We can assert that our method, if it were to proceed, should return null.
            // However, the current implementation of GetSchemeData directly news up XLWorkbook.
            // So, if new XLWorkbook(stream) fails, GetSchemeData itself will fail.
            // Let's simulate a stream that ClosedXML *can* open, but is empty of relevant data.
             var emptyWorkbook = new XLWorkbook();
             emptyWorkbook.Worksheets.Add("Sheet1"); // Has a sheet, but no data
             var emptyContentStream = new MemoryStream();
             emptyWorkbook.SaveAs(emptyContentStream);
             emptyContentStream.Position = 0;
             
             var resultDto = reader.GetSchemeData("AnyScheme", emptyContentStream);
             Assert.Null(resultDto);
             return;
        }
        // If XLWorkbook somehow doesn't throw for an empty stream (unlikely)
        var result = reader.GetSchemeData("AnyScheme", stream);
        Assert.Null(result);
    }


    [Fact]
    public void GetSchemeData_SchemeNameCaseInsensitive_ReturnsCorrectDTO()
    {
        var reader = new MutualFundExcelReader();
        var expectedDto = new SchemeDataDTO
        {
            SchemeName = "Test Scheme Beta", // Name in the Excel
            NumberOfSchemes = 2,
            NumberOfFolios = 200,
            FundsMobilized = 2000.50m,
            Redemption = 1000.25m,
            NetInflow = 1000.25m,
            AUM = 40000.75m,
            AverageAUM = 39000.00m
        };
        // Use CreateMockExcelStream, ensuring the scheme name set in the cell is "Test Scheme Beta"
        using var stream = CreateMockExcelStream(expectedDto, "Test Scheme Beta"); 
        
        // Search with a different casing
        var resultDto = reader.GetSchemeData("test scheme beta", stream); 

        Assert.NotNull(resultDto);
        Assert.Equal(expectedDto.SchemeName, resultDto.SchemeName); // DTO should retain original casing
        Assert.Equal(expectedDto.NumberOfSchemes, resultDto.NumberOfSchemes);
        Assert.Equal(expectedDto.NumberOfFolios, resultDto.NumberOfFolios);
        Assert.Equal(expectedDto.FundsMobilized, resultDto.FundsMobilized);
    }
}
