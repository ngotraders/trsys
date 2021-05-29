using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;

namespace Trsys.Web.Configurations
{
    public class PasswordHasher
    {
        private readonly string salt;

        public PasswordHasher(string salt)
        {
            this.salt = salt;
        }

        public string Hash(string password)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
    }
}
