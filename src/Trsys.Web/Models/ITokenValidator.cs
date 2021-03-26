namespace Trsys.Web.Models
{
    public interface ITokenValidator
    {
        bool Validate(string token);
    }
}