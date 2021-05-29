namespace Trsys.Web.Models.SecretKeys
{
    public class SecretToken
    {
        public string Token { get; set; }
        public string Key { get; set; }
        public SecretKeyType KeyType { get; set; }
    }
}
