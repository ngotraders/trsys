using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Services
{
    public class SecretKeyService
    {
        private readonly ISecretKeyRepository repository;
        private readonly ISecretKeyTokenStore secretKeyTokenStore;
        private readonly ISecretTokenStore tokenStore;

        public SecretKeyService(ISecretKeyRepository repository, ISecretKeyTokenStore validKeyStore, ISecretTokenStore tokenStore)
        {
            this.repository = repository;
            this.secretKeyTokenStore = validKeyStore;
            this.tokenStore = tokenStore;
        }

        public Task<List<SecretKey>> SearchAllAsync()
        {
            return repository.SearchAllAsync();
        }

        public Task<SecretKey> FindBySecretKeyAsync(string key)
        {
            return repository.FindBySecretKeyAsync(key);
        }

        public async Task<RegisterSecretKeyResult> RegisterSecretKeyAsync(string key, SecretKeyType keyType, string description)
        {
            SecretKey newSecretKey;
            if (string.IsNullOrEmpty(key))
            {
                newSecretKey = await repository.CreateNewSecretKeyAsync(keyType);
            }
            else
            {
                newSecretKey = await repository.FindBySecretKeyAsync(key);
                if (newSecretKey != null)
                {
                    return RegisterSecretKeyResult.Fail("既に存在するキーです。");
                }
                newSecretKey = SecretKey.Create(key, keyType, description);
            }
            await repository.SaveAsync(newSecretKey);
            return RegisterSecretKeyResult.Ok(newSecretKey.Key);
        }

        public async Task<OperationResult> UpdateSecretKeyAsync(string key, SecretKeyType keyType, string description)
        {
            var secretKey = await repository.FindBySecretKeyAsync(key);
            if (secretKey == null)
            {
                return OperationResult.Fail($"シークレットキー: {key} を編集できません。");
            }

            if (secretKey.IsValid && keyType != secretKey.KeyType.Value)
            {
                return OperationResult.Fail($"シークレットキー: {key} を編集できません。");
            }

            secretKey.KeyType = keyType;
            secretKey.Description = description;

            await repository.SaveAsync(secretKey);
            return OperationResult.Ok();
        }

        public async Task<GenerateSecretTokenResult> GenerateSecretTokenAsync(string key)
        {
            var secretKey = await secretKeyTokenStore.FindAsync(key);
            if (secretKey == null || !secretKey.IsValid)
            {
                if (secretKey == null)
                {
                    await repository.SaveAsync(SecretKey.Create(key, null, null));
                }
                return GenerateSecretTokenResult.InvalidSecretKey(true);
            }

            if (secretKey.HasToken)
            {
                if (secretKey.IsInUse())
                {
                    return GenerateSecretTokenResult.SecretKeyInUse();
                }
                await tokenStore.RemoveAsync(secretKey.Token);
            }

            var token = secretKey.GenerateToken();
            await tokenStore.AddAsync(secretKey.Key, secretKey.KeyType.Value, secretKey.Token);
            return GenerateSecretTokenResult.Ok(token, secretKey.Key, secretKey.KeyType.Value);
        }

        public async Task<OperationResult> VerifyAndTouchSecretTokenAsync(string token, SecretKeyType? keyType = null)
        {
            var secretToken = await tokenStore.FindAsync(token);
            if (secretToken == null)
            {
                return OperationResult.Fail("token is invalid");
            }

            if (keyType.HasValue && !secretToken.KeyType.HasFlag(keyType))
            {
                return OperationResult.Fail("token is invalid");
            }
            var secretKey = await secretKeyTokenStore.FindAsync(secretToken.Key);
            secretKey.Touch();
            await secretKeyTokenStore.SaveAsync(secretKey);
            return SecretTokenVerifyResult.Ok(secretToken.Key, secretToken.KeyType);
        }

        public async Task<OperationResult> ReleaseSecretTokenAsync(string token)
        {
            var secretToken = await tokenStore.FindAsync(token);
            if (secretToken == null)
            {
                return OperationResult.Fail("Token is not valid");
            }
            await tokenStore.RemoveAsync(token);

            var key = secretToken.Key;
            var secretKeyToken = await secretKeyTokenStore.FindAsync(key);
            if (secretKeyToken == null)
            {
                return OperationResult.Fail("Specified key not found.");
            }
            secretKeyToken.ReleaseToken();
            await secretKeyTokenStore.SaveAsync(secretKeyToken);
            return ReleaseSecretTokenResult.Ok(key, secretKeyToken.KeyType.Value);
        }

        public async Task<OperationResult> ApproveSecretKeyAsync(string key)
        {
            var secretKey = await repository.FindBySecretKeyAsync(key);
            if (secretKey == null || secretKey.IsValid)
            {
                return OperationResult.Fail($"シークレットキー: {key} を有効化できません。");
            }

            secretKey.Approve();
            await repository.SaveAsync(secretKey);
            return OperationResult.Ok();
        }

        public async Task<OperationResult> RevokeSecretKeyAsync(string key)
        {
            var secretKey = await repository.FindBySecretKeyAsync(key);
            if (secretKey == null || !secretKey.IsValid)
            {
                return OperationResult.Fail($"シークレットキー: {key} を無効化できません。");
            }

            var secretKeyToken = await secretKeyTokenStore.FindAsync(key);
            var currentToken = secretKeyToken.Token;

            secretKey.Revoke();

            await repository.SaveAsync(secretKey);
            await tokenStore.RemoveAsync(currentToken);
            return RevokeSecretKeyResult.Ok(currentToken);
        }

        public async Task<OperationResult> DeleteSecretKeyAsync(string key)
        {
            var secretKey = await repository.FindBySecretKeyAsync(key);
            if (secretKey == null || secretKey.IsValid)
            {
                return OperationResult.Fail($"シークレットキー: {key} を削除できません。");
            }

            await repository.RemoveAsync(secretKey);
            return OperationResult.Ok();
        }

        public async Task RefreshScretKeysAsync()
        {
            var keys = await SearchAllAsync();
            foreach (var key in keys)
            {
                await secretKeyTokenStore.SaveAsync(new SecretKeyToken()
                {
                    KeyType = key.KeyType,
                    Key = key.Key,
                    IsValid = key.IsValid,
                });
            }
        }
    }
}
