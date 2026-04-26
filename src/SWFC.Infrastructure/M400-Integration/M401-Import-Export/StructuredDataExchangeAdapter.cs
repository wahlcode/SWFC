using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace SWFC.Infrastructure.M400_Integration.M401_ImportExport;

public enum StructuredDataFormat
{
    Csv,
    Excel
}

public enum IntegrationFieldKind
{
    Text,
    Integer,
    Decimal,
    DateTime,
    Boolean
}

public sealed record IntegrationFieldMapping(
    string ExternalColumn,
    string TargetField,
    IntegrationFieldKind FieldKind = IntegrationFieldKind.Text);

public sealed record StructuredImportRequest(
    string FileName,
    StructuredDataFormat Format,
    byte[] Content,
    string TargetModule,
    string TargetOperation,
    IReadOnlyCollection<IntegrationFieldMapping> FieldMappings,
    IReadOnlyCollection<string> RequiredExternalColumns);

public sealed record IntegrationMappedRecord(
    string TargetModule,
    string TargetOperation,
    int SourceRowNumber,
    IReadOnlyDictionary<string, object?> Fields);

public sealed record IntegrationTargetResult(
    bool Accepted,
    string? ExternalReference,
    string? Message)
{
    public static IntegrationTargetResult Success(string? externalReference = null, string? message = null)
        => new(true, externalReference, message);

    public static IntegrationTargetResult Rejected(string message)
        => new(false, null, message);
}

public interface IIntegrationModuleTarget
{
    Task<IntegrationTargetResult> SubmitAsync(
        IntegrationMappedRecord record,
        CancellationToken cancellationToken = default);
}

public sealed record StructuredImportRowResult(
    int RowNumber,
    bool Accepted,
    string? ExternalReference,
    IReadOnlyList<string> Messages);

public sealed record StructuredImportResult(
    string TargetModule,
    string TargetOperation,
    int TotalRows,
    int AcceptedRows,
    int RejectedRows,
    IReadOnlyList<StructuredImportRowResult> Rows,
    IReadOnlyList<string> TechnicalLog);

public sealed record StructuredExportRequest(
    string FileName,
    StructuredDataFormat Format,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Records);

public sealed record StructuredExportResult(
    string FileName,
    StructuredDataFormat Format,
    string ContentType,
    byte[] Content);

public sealed class StructuredDataExchangeAdapter
{
    public async Task<StructuredImportResult> ImportAsync(
        StructuredImportRequest request,
        IIntegrationModuleTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(target);

        ValidateImportRequest(request);

        var sourceRows = StructuredFileReader.Read(request);
        var technicalLog = new List<string>
        {
            $"M401 read {sourceRows.Count} structured rows from {request.FileName}."
        };

        ValidateRequiredColumns(sourceRows, request.RequiredExternalColumns);

        var rowResults = new List<StructuredImportRowResult>();

        foreach (var row in sourceRows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var messages = new List<string>();
            var fields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var mapping in request.FieldMappings)
            {
                var rawValue = row.Values.TryGetValue(mapping.ExternalColumn, out var value)
                    ? value
                    : string.Empty;

                if (!TryConvert(rawValue, mapping.FieldKind, out var converted, out var error))
                {
                    messages.Add($"{mapping.ExternalColumn}: {error}");
                    continue;
                }

                fields[mapping.TargetField] = converted;
            }

            if (messages.Count > 0)
            {
                rowResults.Add(new StructuredImportRowResult(row.RowNumber, false, null, messages));
                continue;
            }

            var targetResult = await target.SubmitAsync(
                new IntegrationMappedRecord(
                    request.TargetModule,
                    request.TargetOperation,
                    row.RowNumber,
                    fields),
                cancellationToken);

            rowResults.Add(new StructuredImportRowResult(
                row.RowNumber,
                targetResult.Accepted,
                targetResult.ExternalReference,
                string.IsNullOrWhiteSpace(targetResult.Message)
                    ? Array.Empty<string>()
                    : new[] { targetResult.Message! }));
        }

        return new StructuredImportResult(
            request.TargetModule,
            request.TargetOperation,
            sourceRows.Count,
            rowResults.Count(x => x.Accepted),
            rowResults.Count(x => !x.Accepted),
            rowResults,
            technicalLog);
    }

    public StructuredExportResult Export(StructuredExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Columns.Count == 0)
        {
            throw new InvalidOperationException("At least one export column is required.");
        }

        return request.Format switch
        {
            StructuredDataFormat.Csv => new StructuredExportResult(
                EnsureExtension(request.FileName, ".csv"),
                request.Format,
                "text/csv",
                Encoding.UTF8.GetBytes(WriteCsv(request.Columns, request.Records))),
            StructuredDataFormat.Excel => new StructuredExportResult(
                EnsureExtension(request.FileName, ".xml"),
                request.Format,
                "application/vnd.ms-excel",
                Encoding.UTF8.GetBytes(WriteExcelXml(request.Columns, request.Records))),
            _ => throw new InvalidOperationException($"Unsupported export format '{request.Format}'.")
        };
    }

    private static void ValidateImportRequest(StructuredImportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new InvalidOperationException("Import file name is required.");

        if (request.Content.Length == 0)
            throw new InvalidOperationException("Import content is empty.");

        if (string.IsNullOrWhiteSpace(request.TargetModule))
            throw new InvalidOperationException("Target module is required.");

        if (string.IsNullOrWhiteSpace(request.TargetOperation))
            throw new InvalidOperationException("Target operation is required.");

        if (request.FieldMappings.Count == 0)
            throw new InvalidOperationException("At least one field mapping is required.");
    }

    private static void ValidateRequiredColumns(
        IReadOnlyList<StructuredSourceRow> rows,
        IReadOnlyCollection<string> requiredColumns)
    {
        if (rows.Count == 0 || requiredColumns.Count == 0)
        {
            return;
        }

        var columns = rows[0].Values.Keys;
        var missing = requiredColumns
            .Where(required => !columns.Contains(required, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        if (missing.Length > 0)
        {
            throw new InvalidOperationException(
                $"Import file is missing required column(s): {string.Join(", ", missing)}.");
        }
    }

    private static bool TryConvert(
        string? rawValue,
        IntegrationFieldKind fieldKind,
        out object? converted,
        out string? error)
    {
        var value = rawValue?.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            converted = null;
            error = null;
            return true;
        }

        switch (fieldKind)
        {
            case IntegrationFieldKind.Text:
                converted = value;
                error = null;
                return true;
            case IntegrationFieldKind.Integer:
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
                {
                    converted = integer;
                    error = null;
                    return true;
                }
                break;
            case IntegrationFieldKind.Decimal:
                if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
                {
                    converted = number;
                    error = null;
                    return true;
                }
                break;
            case IntegrationFieldKind.DateTime:
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTime))
                {
                    converted = dateTime;
                    error = null;
                    return true;
                }
                break;
            case IntegrationFieldKind.Boolean:
                if (bool.TryParse(value, out var boolean))
                {
                    converted = boolean;
                    error = null;
                    return true;
                }
                break;
        }

        converted = null;
        error = $"Value '{value}' cannot be converted to {fieldKind}.";
        return false;
    }

    private static string WriteCsv(
        IReadOnlyList<string> columns,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> records)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", columns.Select(EscapeCsv)));

        foreach (var record in records)
        {
            var values = columns.Select(column => record.TryGetValue(column, out var value)
                ? EscapeCsv(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty)
                : string.Empty);

            builder.AppendLine(string.Join(",", values));
        }

        return builder.ToString();
    }

    private static string WriteExcelXml(
        IReadOnlyList<string> columns,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> records)
    {
        XNamespace ss = "urn:schemas-microsoft-com:office:spreadsheet";

        var rows = new List<XElement>
        {
            new(ss + "Row", columns.Select(column => new XElement(ss + "Cell",
                new XElement(ss + "Data", new XAttribute(ss + "Type", "String"), column))))
        };

        rows.AddRange(records.Select(record => new XElement(ss + "Row",
            columns.Select(column => new XElement(ss + "Cell",
                new XElement(
                    ss + "Data",
                    new XAttribute(ss + "Type", "String"),
                    record.TryGetValue(column, out var value)
                        ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
                        : string.Empty))))));

        var document = new XDocument(
            new XElement(ss + "Workbook",
                new XAttribute(XNamespace.Xmlns + "ss", ss),
                new XElement(ss + "Worksheet",
                    new XAttribute(ss + "Name", "Export"),
                    new XElement(ss + "Table", rows))));

        return document.ToString(SaveOptions.DisableFormatting);
    }

    private static string EscapeCsv(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    private static string EnsureExtension(string fileName, string extension)
    {
        return Path.HasExtension(fileName)
            ? fileName
            : fileName + extension;
    }
}

internal sealed record StructuredSourceRow(
    int RowNumber,
    IReadOnlyDictionary<string, string?> Values);

internal static class StructuredFileReader
{
    public static IReadOnlyList<StructuredSourceRow> Read(StructuredImportRequest request)
    {
        return request.Format switch
        {
            StructuredDataFormat.Csv => ReadCsv(request.Content),
            StructuredDataFormat.Excel => ReadExcel(request.Content),
            _ => throw new InvalidOperationException($"Unsupported import format '{request.Format}'.")
        };
    }

    private static IReadOnlyList<StructuredSourceRow> ReadCsv(byte[] content)
    {
        var text = Encoding.UTF8.GetString(content);
        var lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        if (lines.Length == 0)
        {
            return Array.Empty<StructuredSourceRow>();
        }

        var headers = ParseCsvLine(lines[0]);
        var rows = new List<StructuredSourceRow>();

        for (var index = 1; index < lines.Length; index++)
        {
            var values = ParseCsvLine(lines[index]);
            var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
            {
                row[headers[columnIndex]] = columnIndex < values.Count
                    ? values[columnIndex]
                    : null;
            }

            rows.Add(new StructuredSourceRow(index + 1, row));
        }

        return rows;
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var builder = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var current = line[index];

            if (current == '"' && inQuotes && index + 1 < line.Length && line[index + 1] == '"')
            {
                builder.Append('"');
                index++;
                continue;
            }

            if (current == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (current == ',' && !inQuotes)
            {
                values.Add(builder.ToString());
                builder.Clear();
                continue;
            }

            builder.Append(current);
        }

        values.Add(builder.ToString());
        return values;
    }

    private static IReadOnlyList<StructuredSourceRow> ReadExcel(byte[] content)
    {
        using var stream = new MemoryStream(content);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);

        var sharedStrings = ReadSharedStrings(archive);
        var worksheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml")
            ?? throw new InvalidOperationException("Excel import requires a first worksheet.");

        using var worksheetStream = worksheetEntry.Open();
        var worksheet = XDocument.Load(worksheetStream);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var rows = worksheet.Descendants(ns + "row").ToArray();

        if (rows.Length == 0)
        {
            return Array.Empty<StructuredSourceRow>();
        }

        var headerCells = ReadRow(rows[0], sharedStrings);
        var headers = headerCells
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToArray();
        var result = new List<StructuredSourceRow>();

        foreach (var rowElement in rows.Skip(1))
        {
            var rowNumber = (int?)rowElement.Attribute("r") ?? result.Count + 2;
            var values = ReadRow(rowElement, sharedStrings);
            var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < headers.Length; index++)
            {
                row[headers[index]] = values.TryGetValue(index + 1, out var value)
                    ? value
                    : null;
            }

            result.Add(new StructuredSourceRow(rowNumber, row));
        }

        return result;
    }

    private static Dictionary<int, string> ReadRow(
        XElement rowElement,
        IReadOnlyList<string> sharedStrings)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var values = new Dictionary<int, string>();

        foreach (var cell in rowElement.Elements(ns + "c"))
        {
            var reference = cell.Attribute("r")?.Value ?? string.Empty;
            var columnIndex = GetColumnIndex(reference);
            var raw = cell.Element(ns + "v")?.Value ?? string.Empty;

            if (string.Equals(cell.Attribute("t")?.Value, "s", StringComparison.Ordinal) &&
                int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sharedStringIndex) &&
                sharedStringIndex >= 0 &&
                sharedStringIndex < sharedStrings.Count)
            {
                values[columnIndex] = sharedStrings[sharedStringIndex];
                continue;
            }

            values[columnIndex] = raw;
        }

        return values;
    }

    private static IReadOnlyList<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");

        if (entry is null)
        {
            return Array.Empty<string>();
        }

        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        return document.Descendants(ns + "si")
            .Select(item => string.Concat(item.Descendants(ns + "t").Select(text => text.Value)))
            .ToArray();
    }

    private static int GetColumnIndex(string cellReference)
    {
        var letters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        var column = 0;

        foreach (var letter in letters)
        {
            column *= 26;
            column += char.ToUpperInvariant(letter) - 'A' + 1;
        }

        return column;
    }
}
