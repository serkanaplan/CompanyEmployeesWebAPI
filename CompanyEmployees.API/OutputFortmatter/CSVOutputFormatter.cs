using MHHA = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Shared.DTO;

namespace CompanyEmployees.API.OutputFortmatter;

public class CsvOutputFormatter : TextOutputFormatter
{
    public CsvOutputFormatter()
    {
        SupportedMediaTypes.Add(MHHA.Parse("text/csv"));
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanWriteType(Type? type)
    {
        if (typeof(CompanyDto).IsAssignableFrom(type) || typeof(IEnumerable<CompanyDto>).IsAssignableFrom(type))
            return base.CanWriteType(type);
        return false;
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var response = context.HttpContext.Response;
        var buffer = new StringBuilder();

        if (context.Object is IEnumerable<CompanyDto> enumerable)
            foreach (var company in enumerable)
                FormatCsv(buffer, company);
        else
            FormatCsv(buffer, (CompanyDto)context.Object);

        await response.WriteAsync(buffer.ToString());
    }
    private static void FormatCsv(StringBuilder buffer, CompanyDto company)
    => buffer.AppendLine($"{company.Id},\"{company.Name},\"{company.FullAddress}\"");
}