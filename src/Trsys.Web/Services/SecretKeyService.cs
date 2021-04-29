using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Services
{
    public class SecretKeyService
    {
        private readonly ISecretKeyRepository repository;
        private readonly ISecretKeyUsageStore usageStore;

        public SecretKeyService(ISecretKeyRepository repository, ISecretKeyUsageStore usageStore)
        {
            this.repository = repository;
            this.usageStore = usageStore;
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
                newSecretKey = new SecretKey()
                {
                    Key = key,
                    KeyType = keyType,
                    Description = description,
                };
            }
            await repository.SaveAsync(newSecretKey);
            return RegisterSecretKeyResult.Ok(newSecretKey.Key);
        }

        public async Task<OperationResult> UpdateSecretKey(string secretKey, SecretKeyType keyType, string description)
        {
            var secretKeyEntity = await repository.FindBySecretKeyAsync(secretKey);
            if (secretKeyEntity == null)
            {
                return OperationResult.Fail($"シークレットキー: {secretKey} を編集できません。");
            }

            if (secretKeyEntity.IsValid && keyType != secretKeyEntity.KeyType.Value)
            {
                return OperationResult.Fail($"シークレットキー: {secretKey} を編集できません。");
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
                if (secretKey == null)
                {
                    secretKey = new SecretKey()
                    {
                        Key = key,
                    };
                    await repository.SaveAsync(secretKey);
                }
                return GenerateSecretTokenResult.InvalidSecretKey();
            }

            var usage = await usageStore.FindAsync(key);
            if (usage != null)
            {
                if (usage.IsInUse())
                {
                    return GenerateSecretTokenResult.SecretKeyInUse();
                }
                usage.Reset();
            }
            else
            {
                await usageStore.AddAsync(key);
            }

            var token = secretKey.GenerateToken();
            await repository.SaveAsync(secretKey);
            return GenerateSecretTokenResult.Ok(token, secretKey.Key, secretKey.KeyType.Value);
        }

        public Task TouchSecretTokenAsync(string key)
        {
            usageStore.TouchAsync(key);
            return Task.CompletedTask;
        }

        public async Task ReleaseSecretTokenAsync(string secretKey)
        {
            var secretKeyEntity = await repository.FindBySecretKeyAsync(secretKey);
            if (secretKeyEntity != null)
            {
                secretKeyEntity.ReleaseToken();
                await repository.SaveAsync(secretKeyEntity);
            }
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

            secretKey.Revoke();
            await repository.SaveAsync(secretKey);
            await usageStore.RemoveAsync(key);
            return OperationResult.Ok();
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
