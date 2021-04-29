namespace Trsys.Web.Services
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public static OperationResult Fail(string errorMessage)
        {
            return new OperationResult()
            {
                Success = false,
                ErrorMessage = errorMessage,
            };
        }

        public static OperationResult Ok()
        {
            return new OperationResult()
            {
                Success = true,
            };
        }
    }
}