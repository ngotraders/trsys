using Microsoft.AspNetCore.Mvc.Formatters;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;

namespace Trsys.Web.Configurations
{
    public class TextPlainInputFormatter : TextInputFormatter
    {
        public TextPlainInputFormatter()
        {
            SupportedMediaTypes.Add("text/plain");
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var result = await context.HttpContext.Request.BodyReader.ReadAsync();
            var str = encoding.GetString(result.Buffer.ToArray());
            return InputFormatterResult.Success(str?.Trim('\0'));
        }
    }
}
