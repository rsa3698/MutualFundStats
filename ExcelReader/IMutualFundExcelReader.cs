using ExcelReader.DTO;
using System.IO;

namespace ExcelReader
{
    public interface IMutualFundExcelReader
    {
        SchemeDataDTO GetSchemeData(string schemeName, Stream fileStream);
    }
}
