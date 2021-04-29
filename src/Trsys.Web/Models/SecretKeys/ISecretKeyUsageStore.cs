namespace Trsys.Web.Models.SecretKeys
{
    public interface ISecretKeyUsageStore
    {
        void Add(string key);
        SecretKeyUsage Find(string key);
        void Touch(string key);
        void Remove(string key);
    }
}
