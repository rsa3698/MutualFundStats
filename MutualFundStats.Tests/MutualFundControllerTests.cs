using Xunit;
using Moq;
using ExcelReader;
using ExcelReader.DTO;
using MutualFundStats.Controllers;
using MutualFundStats.Data;
using MutualFundStats.Models;
using MutualFundStats.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

public class MutualFundControllerTests
{
    private readonly Mock<IMutualFundExcelReader> _mockExcelReader;
    private readonly MutualFundDataContext _context;
    private readonly MutualFundController _controller;

    public MutualFundControllerTests()
    {
        _mockExcelReader = new Mock<IMutualFundExcelReader>();

        // Setup InMemory database
        var options = new DbContextOptionsBuilder<MutualFundDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for each test run
            .Options;
        _context = new MutualFundDataContext(options); // Ensure your DbContext has a constructor that accepts DbContextOptions

        _controller = new MutualFundController(_mockExcelReader.Object, _context);
    }

    private Stream CreateMockExcelStream(List<SchemeDataDTO> schemeDataList)
    {
        // This helper can be more sophisticated if needed, for now, it's a simple placeholder
        // as the actual Excel reading is mocked.
        // If you were testing the ExcelReader itself, this would generate a real Excel file stream.
        var stream = new MemoryStream();
        // For controller tests, we often don't need a real Excel stream if IMutualFundExcelReader is mocked.
        // However, if the controller logic itself tries to read the stream before passing to reader,
        // a minimal, valid stream might be needed.
        if (schemeDataList != null && schemeDataList.Any())
        {
            // Simulate some content to make the stream appear valid if controller checks Length > 0
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true)) 
            {
                writer.WriteLine("SchemeName,NAV,Date"); // Dummy header
                foreach (var scheme in schemeDataList)
                {
                    writer.WriteLine($"{scheme.SchemeName},{scheme.NAV},{scheme.Date}");
                }
            }
            stream.Position = 0;
        }
        return stream;
    }
    
    private IFormFile CreateMockFormFile(List<SchemeDataDTO> schemeDataList)
    {
        var stream = CreateMockExcelStream(schemeDataList);
        return new FormFile(stream, 0, stream.Length, "file", "test.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
    }


    [Fact]
    public async Task ReadExcelFrom_FormData_ValidFile_ReturnsOkResult()
    {
        // Arrange
        var testSchemeName = MutualFundSchemeConstants.schemeDictionary.Keys.First();
        var schemeDataDto = new SchemeDataDTO 
        { 
            SchemeName = testSchemeName, 
            NumberOfSchemes = 1, 
            NumberOfFolios = 100,
            FundsMobilized = 1000,
            Redemption = 500,
            NetInflow = 500,
            AUM = 2000,
            AverageAUM = 1900
        };
        _mockExcelReader.Setup(r => r.GetSchemeData(It.IsAny<string>(), It.IsAny<Stream>()))
                        .Returns((string sName, Stream str) => {
                            if (sName == testSchemeName) return schemeDataDto;
                            // For other schemes in MutualFundSchemeConstants.schemeDictionary, return a default DTO
                            // or specific mock DTOs if their interaction needs to be tested.
                            // If a scheme is not found by GetSchemeData, it returns null.
                            if (MutualFundSchemeConstants.schemeDictionary.ContainsKey(sName))
                            {
                                return new SchemeDataDTO { SchemeName = sName, /* other properties with default values */ };
                            }
                            return null; 
                        });

        var formFile = CreateMockFormFile(new List<SchemeDataDTO> { schemeDataDto });
        var month = new DateTime(2023, 1, 1);

        // Act
        var result = await _controller.ReadExcelFrom_FormData(formFile, month);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("entries have been successfully added", okResult.Value.ToString());
        
        // Verify data was added to the context
        Assert.True(await _context.MutualFundDatas.AnyAsync(mf => mf.SchemeId == MutualFundSchemeConstants.schemeDictionary[testSchemeName]));
        var addedData = await _context.MutualFundDatas.FirstAsync(mf => mf.SchemeId == MutualFundSchemeConstants.schemeDictionary[testSchemeName]);
        Assert.Equal(schemeDataDto.NumberOfFolios, addedData.NumberOfFolios);
        Assert.Equal(month, addedData.Month);
    }

    [Fact]
    public async Task ReadExcelFrom_FormData_NullFile_ReturnsBadRequest()
    {
        // Arrange
        var month = new DateTime(2023, 1, 1);

        // Act
        var result = await _controller.ReadExcelFrom_FormData(null, month);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is not provided or is empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task ReadExcelFrom_FormData_EmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var emptyStream = new MemoryStream();
        var formFile = new FormFile(emptyStream, 0, 0, "file", "empty.xlsx"); // Empty file
        var month = new DateTime(2023, 1, 1);

        // Act
        var result = await _controller.ReadExcelFrom_FormData(formFile, month);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is not provided or is empty.", badRequestResult.Value);
    }
    
    [Fact]
    public async Task ReadExcelFrom_FormData_SchemeNotFoundInExcel_ReturnsNotFound()
    {
        // Arrange
        // Ensure MutualFundSchemeConstants.schemeDictionary has more than one scheme for this test
        if (MutualFundSchemeConstants.schemeDictionary.Count < 2)
        {
            // This test might not be meaningful or needs adjustment if there's only one scheme
            // For now, let's assume there are multiple schemes configured.
            // You could also dynamically add to MutualFundSchemeConstants.schemeDictionary for test setup if needed.
             Assert.True(false, "Test requires more than one scheme in MutualFundSchemeConstants.schemeDictionary or mock adjustment.");
        }

        var firstSchemeKey = MutualFundSchemeConstants.schemeDictionary.Keys.ElementAt(0);
        var secondSchemeKey = MutualFundSchemeConstants.schemeDictionary.Keys.ElementAt(1); // This one will "not be found"

        _mockExcelReader.Setup(r => r.GetSchemeData(firstSchemeKey, It.IsAny<Stream>()))
                        .Returns(new SchemeDataDTO { SchemeName = firstSchemeKey });
        _mockExcelReader.Setup(r => r.GetSchemeData(secondSchemeKey, It.IsAny<Stream>()))
                        .Returns((SchemeDataDTO)null); // Simulate second scheme not found in Excel

        // If there are more schemes, ensure they return some default DTO to avoid null issues for them,
        // unless the goal is to test cascade failure (which is not the case here).
        foreach(var schemeKey in MutualFundSchemeConstants.schemeDictionary.Keys.Skip(2))
        {
            _mockExcelReader.Setup(r => r.GetSchemeData(schemeKey, It.IsAny<Stream>()))
                            .Returns(new SchemeDataDTO { SchemeName = schemeKey });
        }


        var formFile = CreateMockFormFile(new List<SchemeDataDTO>()); // Content doesn't matter as GetSchemeData is mocked
        var month = new DateTime(2023, 1, 1);

        // Act
        var result = await _controller.ReadExcelFrom_FormData(formFile, month);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        // The controller returns NotFound for the *first* scheme it can't find data for (and isn't "Flexi Cap Fund")
        // The message includes the scheme name that was not found.
        Assert.Contains($"Scheme '{secondSchemeKey}' not found", notFoundResult.Value.ToString());
    }


    [Fact]
    public async Task ReadExcelFrom_FormData_ExcelReaderThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockExcelReader.Setup(r => r.GetSchemeData(It.IsAny<string>(), It.IsAny<Stream>()))
                        .Throws(new Exception("Excel processing error"));
        
        var formFile = CreateMockFormFile(new List<SchemeDataDTO>());
        var month = new DateTime(2023, 1, 1);

        // Act
        var result = await _controller.ReadExcelFrom_FormData(formFile, month);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Contains("Internal server error: Excel processing error", statusCodeResult.Value.ToString());
    }

    [Fact]
    public async Task ReadExcelFrom_FormData_FlexiCapFundNotFound_StillProcessesOtherSchemes()
    {
        // Arrange
        var flexiCapSchemeName = "Flexi Cap Fund"; // Assuming this is the exact name treated specially
        var otherSchemeName = MutualFundSchemeConstants.schemeDictionary.Keys.FirstOrDefault(k => k != flexiCapSchemeName);

        if (otherSchemeName == null)
        {
            Assert.True(false, "This test requires at least one scheme other than 'Flexi Cap Fund' in MutualFundSchemeConstants.");
        }

        // Simulate Flexi Cap Fund data missing in Excel
        _mockExcelReader.Setup(r => r.GetSchemeData(flexiCapSchemeName, It.IsAny<Stream>()))
                        .Returns((SchemeDataDTO)null);
        
        // Simulate other scheme data present
        var otherSchemeDto = new SchemeDataDTO { SchemeName = otherSchemeName, NumberOfFolios = 200 };
        _mockExcelReader.Setup(r => r.GetSchemeData(otherSchemeName, It.IsAny<Stream>()))
                        .Returns(otherSchemeDto);
        
        // Mock any other schemes if necessary
         foreach(var schemeKey in MutualFundSchemeConstants.schemeDictionary.Keys.Where(k => k != flexiCapSchemeName && k != otherSchemeName))
        {
            _mockExcelReader.Setup(r => r.GetSchemeData(schemeKey, It.IsAny<Stream>()))
                            .Returns(new SchemeDataDTO { SchemeName = schemeKey }); // Default mock
        }

        var formFile = CreateMockFormFile(new List<SchemeDataDTO>());
        var month = new DateTime(2023, 1, 1);

        // Act
        var result = await _controller.ReadExcelFrom_FormData(formFile, month);

        // Assert
        Assert.IsType<OkObjectResult>(result); // Should still be OK if at least one other scheme is processed
        
        // Verify Flexi Cap Fund was "added" with default values (due to ?? 0 logic in controller)
        var flexiCapDbId = MutualFundSchemeConstants.schemeDictionary.GetValueOrDefault(flexiCapSchemeName);
        if(flexiCapDbId != -1) // If Flexi Cap is in the dictionary
        {
            var flexiCapData = await _context.MutualFundDatas.FirstOrDefaultAsync(mf => mf.SchemeId == flexiCapDbId && mf.Month == month);
            Assert.NotNull(flexiCapData);
            Assert.Equal(0, flexiCapData.NumberOfFolios); // Default value due to null DTO
        }
        
        // Verify other scheme was added with its data
        var otherSchemeDbId = MutualFundSchemeConstants.schemeDictionary[otherSchemeName];
        var otherSchemeDataInDb = await _context.MutualFundDatas.FirstOrDefaultAsync(mf => mf.SchemeId == otherSchemeDbId && mf.Month == month);
        Assert.NotNull(otherSchemeDataInDb);
        Assert.Equal(otherSchemeDto.NumberOfFolios, otherSchemeDataInDb.NumberOfFolios);
    }

    // Test Cases for GetMonthWiseData:

    private void SeedData(List<MutualFundData> testData)
    {
        _context.MutualFundDatas.AddRange(testData);
        _context.SaveChanges();
    }

    [Fact]
    public void GetMonthWiseData_ValidRequest_ReturnsOkWithData()
    {
        // Arrange
        var schemeId = 1;
        var columnName = "NumberOfFolios";
        var testData = new List<MutualFundData>
        {
            new MutualFundData { Id = 1, SchemeId = schemeId, Month = new DateTime(2023, 1, 1), NumberOfFolios = 100 },
            new MutualFundData { Id = 2, SchemeId = schemeId, Month = new DateTime(2023, 2, 1), NumberOfFolios = 110 },
            new MutualFundData { Id = 3, SchemeId = 2, Month = new DateTime(2023, 1, 1), NumberOfFolios = 50 } // Different scheme
        };
        SeedData(testData);

        // Act
        var result = _controller.GetMonthWiseData(schemeId, columnName, "2023-01", "2023-02");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Equal(2, data.Count()); 
        // Add more specific assertions on the returned data structure if needed
        // For example, check if the Month and NumberOfFolios properties exist and have correct values
    }

    [Fact]
    public void GetMonthWiseData_NoDataForScheme_ReturnsNotFound()
    {
        // Arrange
        SeedData(new List<MutualFundData>()); // Empty data

        // Act
        var result = _controller.GetMonthWiseData(99, "NumberOfFolios"); // Non-existent schemeId

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetMonthWiseData_InvalidColumnName_ReturnsBadRequest()
    {
        // Arrange
        var schemeId = 1;
         var testData = new List<MutualFundData>
        {
            new MutualFundData { Id = 1, SchemeId = schemeId, Month = new DateTime(2023, 1, 1), NumberOfFolios = 100 }
        };
        SeedData(testData);

        // Act
        var result = _controller.GetMonthWiseData(schemeId, "InvalidColumn");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Theory]
    [InlineData("2023/01", "2023-02")] // Invalid start month format
    [InlineData("2023-01", "2023/02")] // Invalid end month format
    public void GetMonthWiseData_InvalidMonthFormat_ReturnsBadRequest(string startMonth, string endMonth)
    {
        // Arrange
        var schemeId = 1;

        // Act
        var result = _controller.GetMonthWiseData(schemeId, "NumberOfFolios", startMonth, endMonth);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void GetMonthWiseData_DateRangeFiltersCorrectly()
    {
        // Arrange
        var schemeId = 1;
        var columnName = "NumberOfFolios";
        var testData = new List<MutualFundData>
        {
            new MutualFundData { Id = 1, SchemeId = schemeId, Month = new DateTime(2022, 12, 1), NumberOfFolios = 90 },
            new MutualFundData { Id = 2, SchemeId = schemeId, Month = new DateTime(2023, 1, 1), NumberOfFolios = 100 },
            new MutualFundData { Id = 3, SchemeId = schemeId, Month = new DateTime(2023, 2, 1), NumberOfFolios = 110 },
            new MutualFundData { Id = 4, SchemeId = schemeId, Month = new DateTime(2023, 3, 1), NumberOfFolios = 120 }
        };
        SeedData(testData);

        // Act
        var result = _controller.GetMonthWiseData(schemeId, columnName, "2023-01", "2023-02");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Equal(2, data.Count());
        // Add more specific assertions here based on expected values for Jan and Feb
    }

    // Add similar test cases for each valid columnName:
    // "FundsMobilized", "RepurchaseRedemption", "NetInflowOutflow", 
    // "NetAssetsUnderManagement", "AverageNetAssetsUnderManagement"
    // Example for one more column:
    [Fact]
    public void GetMonthWiseData_ColumnFundsMobilized_ReturnsCorrectData()
    {
        // Arrange
        var schemeId = 1;
        var columnName = "FundsMobilized";
        var testData = new List<MutualFundData>
        {
            new MutualFundData { Id = 1, SchemeId = schemeId, Month = new DateTime(2023, 1, 1), FundsMobilized = 1000.50m }
        };
        SeedData(testData);

        // Act
        var result = _controller.GetMonthWiseData(schemeId, columnName, "2023-01", "2023-01");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value).ToList();
        Assert.Single(data);
        // Example of how to check the actual data:
        // var firstItem = data.First() as dynamic; // Or cast to a known anonymous type structure if possible
        // Assert.Equal(1000.50m, firstItem.FundsMobilized); 
        // Assert.Equal("2023-01", firstItem.Month);
        // Note: Accessing properties of anonymous types in tests can be tricky.
        // Consider defining a simple result DTO for testing if this becomes too complex.
    }
}
