using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Services
{
    public class SecretKeyService
    {
        private readonly ISecretKeyRepository repository;
        private readonly ISecretTokenStore tokenStore;

        public SecretKeyService(ISecretKeyRepository repository, ISecretTokenStore tokenStore)
        {
            this.repository = repository;
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
            var secretKeyEntity = await repository.FindBySecretKeyAsync(key);
            if (secretKeyEntity == null)
            {
                return OperationResult.Fail($"シークレットキー: {key} を編集できません。");
            }

            if (secretKeyEntity.IsValid && keyType != secretKeyEntity.KeyType.Value)
            {
                return OperationResult.Fail($"シークレットキー: {key} を編集できません。");
            }

            secretKeyEntity.KeyType = keyType;
            secretKeyEntity.Description = description;

            await repository.SaveAsync(secretKeyEntity);
            return OperationResult.Ok();
        }

        public async Task<GenerateSecretTokenResult> GenerateSecretTokenAsync(string key)
        {
            var secretKey = await repository.FindBySecretKeyAsync(key);
            if (secretKey == null || !secretKey.IsValid || !secretKey.KeyType.HasValue)
            {
                if (secretKey != null)
                {
                    return GenerateSecretTokenResult.InvalidSecretKey(false);
                }

                secretKey = SecretKey.Create(key, null, null);
                await repository.SaveAsync(secretKey);
                return GenerateSecretTokenResult.InvalidSecretKey(true);
            }

            if (secretKey.HasToken)
            {
                var usage = await tokenStore.FindAsync(secretKey.ValidToken);
                if (usage != null)
                {
                    if (usage.IsInUse())
                    {
                        return GenerateSecretTokenResult.SecretKeyInUse();
                    }
                    await tokenStore.RemoveAsync(secretKey.ValidToken);
                }
            }

            var token = secretKey.GenerateToken();
            await tokenStore.AddAsync(secretKey.Key, secretKey.KeyType.Value, secretKey.ValidToken);
            await repository.SaveAsync(secretKey);
            return GenerateSecretTokenResult.Ok(token, secretKey.Key, secretKey.KeyType.Value);
        }

        public async Task<OperationResult> VerifyAndTouchSecretTokenAsync(string token, SecretKeyType? keyType = null)
        {
            var checkResult = await tokenStore.VerifyAndTouchAsync(token, keyType);
            if (checkResult)
            {
                var secretToken = await tokenStore.FindAsync(token);
                return SecretTokenVerifyResult.Ok(secretToken.Key, secretToken.KeyType);
            }
            return OperationResult.Fail("token is invalid");
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
            var secretKeyEntity = await repository.FindBySecretKeyAsync(key);
            if (secretKeyEntity == null)
            {
                return OperationResult.Fail("Specified key not found.");
            }
            secretKeyEntity.ReleaseToken();
            await repository.SaveAsync(secretKeyEntity);
            return ReleaseSecretTokenResult.Ok(key, secretKeyEntity.KeyType.Value);
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

            var currentToken = secretKey.ValidToken;
            secretKey.Revoke();
            await repository.SaveAsync(secretKey);
            await tokenStore.RemoveAsync(key);
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
    }
}
