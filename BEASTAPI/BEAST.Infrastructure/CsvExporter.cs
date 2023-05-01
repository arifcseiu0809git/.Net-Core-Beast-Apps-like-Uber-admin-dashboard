using CsvHelper;
using BEASTAPI.Core.Contract.Infrastructure;
using System.Globalization;

namespace BEASTAPI.Infrastructure;

public class CsvExporter : ICsvExporter
{
    public byte[] ExportToCsv<T>(List<T> items)
    {
        using var memoryStream = new MemoryStream();
        using (var streamWriter = new StreamWriter(memoryStream))
        {
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(items);
        }

        return memoryStream.ToArray();
    }
}