namespace Trsys.Web.Services
{
    public class RevokeSecretKeyResult : OperationResult
    {
        public string Token { get; set; }

        public static RevokeSecretKeyResult Ok(string token)
        {
            return new RevokeSecretKeyResult()
            {
                Success = true,
                Token = token,
            };
        }
    }
}
