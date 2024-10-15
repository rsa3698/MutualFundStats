
using Microsoft.AspNetCore.Mvc;
using ExcelReader;
using MutualFundStats.Models;
using MutualFundStats.Constants;


using System;
using MutualFundStats.Data;
using System.ComponentModel.DataAnnotations;

namespace MutualFundStats.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MutualFundController : ControllerBase
    {
        private readonly MutualFundExcelReader _excelReader;
        private readonly MutualFundDataContext _dataContext;

        public MutualFundController()
        {
            _excelReader = new MutualFundExcelReader();  // Instantiate ExcelReader
            _dataContext = new MutualFundDataContext();
        }

        [HttpPost("ReadExcelFrom_FormData")]
        public async Task<IActionResult> ReadExcelFrom_FormData(IFormFile file, [FromQuery, Required] DateTime month)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is not provided or is empty.");
            }
            int totalEntriesAdded = 0; // Counter to track the number of successful entries
            try
            {
                // Open the file stream from the uploaded file
                using (var stream = file.OpenReadStream())
                {
                    foreach(var schemeName in MutualFundSchemeConstants.schemeDictionary.Keys)
                    {
                        var schemeData = _excelReader.GetSchemeData(schemeName, stream);

                        if (schemeData == null && !schemeName.Equals("Flexi Cap Fund"))
                        {
                            return NotFound($"Scheme '{schemeName}' not found in the Excel file.");
                        }

                        var mutualFundData = new MutualFundData
                        {
                            SchemeId = MutualFundSchemeConstants.schemeDictionary.GetValueOrDefault(schemeName, -1),  // Reference to the existing scheme
                            NumberOfSchemes = schemeData?.NumberOfSchemes ?? 0, // Use schemeData value or 0 if null
                            Month = month ,
                            FundsMobilized = schemeData?.FundsMobilized ?? 0m, // Use schemeData value or 0m if null
                            RepurchaseRedemption = schemeData?.Redemption ?? 0m, // Use schemeData value or 0m if null
                            NumberOfFolios = schemeData?.NumberOfFolios ?? 0,
                            NetInflowOutflow = schemeData?.NetInflow ?? 0m, // Use schemeData value or 0m if null
                            NetAssetsUnderManagement = schemeData?.AUM ?? 0m, // Use schemeData value or 0m if null
                            AverageNetAssetsUnderManagement = schemeData?.AverageAUM ?? 0m // Use schemeData value or 0m if null
                        };
                        _dataContext.MutualFundDatas.Add(mutualFundData);
                        totalEntriesAdded++;
                        
                    }

                    await _dataContext.SaveChangesAsync();
                    return Ok($"{totalEntriesAdded} entries have been successfully added to the database."); 
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // API to get month-wise data for a particular schemeId and column name, with optional startMonth and endMonth in "YYYY-MM" format
        [HttpGet("GetMonthWiseData")]
        public IActionResult GetMonthWiseData(int schemeId, string columnName, string startMonth = null, string endMonth = null)
        {
            try
            {
                // Prepare default DateTime range if startMonth and endMonth are not provided
                DateTime? startDate = null;
                DateTime? endDate = null;

                // Parse startMonth and endMonth if they are provided in "YYYY-MM" format
                if (!string.IsNullOrEmpty(startMonth))
                {
                    if (!DateTime.TryParseExact(startMonth, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedStartDate))
                    {
                        return BadRequest("Invalid startMonth format. Use 'YYYY-MM'.");
                    }
                    startDate = new DateTime(parsedStartDate.Year, parsedStartDate.Month, 1); // First day of the start month
                }

                if (!string.IsNullOrEmpty(endMonth))
                {
                    if (!DateTime.TryParseExact(endMonth, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedEndDate))
                    {
                        return BadRequest("Invalid endMonth format. Use 'YYYY-MM'.");
                    }
                    endDate = new DateTime(parsedEndDate.Year, parsedEndDate.Month, DateTime.DaysInMonth(parsedEndDate.Year, parsedEndDate.Month)); // Last day of the end month
                }

                // Fetch the data for the specified schemeId
                var schemeDataQuery = _dataContext.MutualFundDatas
                    .Where(mf => mf.SchemeId == schemeId);

                // Apply the month filter if startMonth and endMonth are provided
                if (startDate.HasValue)
                {
                    schemeDataQuery = schemeDataQuery.Where(mf => mf.Month >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    schemeDataQuery = schemeDataQuery.Where(mf => mf.Month <= endDate.Value);
                }

                // Order the data by Month
                var schemeData = schemeDataQuery
                    .OrderBy(mf => mf.Month)
                    .ToList();

                if (!schemeData.Any())
                {
                    return NotFound($"No data found for schemeId: {schemeId} in the specified month range.");
                }

                // Prepare the result based on the column name provided
                var result = new List<object>();

                foreach (var data in schemeData)
                {
                    switch (columnName)
                    {
                        case "NumberOfFolios":
                            result.Add(new { Month = data.Month.ToString("yyyy-MM"), NumberOfFolios = data.NumberOfFolios });
                            break;

                        case "FundsMobilized":
                            result.Add(new { Month = data.Month.ToString("yyyy-MM"), FundsMobilized = data.FundsMobilized });
                            break;

                        case "RepurchaseRedemption":
                            result.Add(new { Month = data.Month.ToString("yyyy-MM"), RepurchaseRedemption = data.RepurchaseRedemption });
                            break;

                        case "NetInflowOutflow":
                            result.Add(new { Month = data.Month.ToString("yyyy-MM"), NetInflowOutflow = data.NetInflowOutflow });
                            break;

                        case "NetAssetsUnderManagement":
                            result.Add(new { Month = data.Month.ToString("yyyy-MM"), NetAssetsUnderManagement = data.NetAssetsUnderManagement });
                            break;

                        case "AverageNetAssetsUnderManagement":
                            result.Add(new { Month = data.Month.ToString("yyyy-MM"), AverageNetAssetsUnderManagement = data.AverageNetAssetsUnderManagement });
                            break;

                        default:
                            return BadRequest("Invalid column name provided.");
                    }
                }

                // Return the result in JSON format
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
