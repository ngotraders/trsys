namespace Trsys.Web.Services
{
    public class RegisterSecretKeyResult
    {
        public bool Success { get; set; }
        public string Key { get; set; }
        public string ErrorMessage { get; set; }

        public static RegisterSecretKeyResult Fail(string errorMessage)
        {
            return new RegisterSecretKeyResult()
            {
                Success = false,
                ErrorMessage = errorMessage,
            };
        }

        public static RegisterSecretKeyResult Ok(string secretKey)
        {
            return new RegisterSecretKeyResult()
            {
                Success = true,
                Key = secretKey,
            };
        }
    }
}
