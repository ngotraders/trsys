using System;

namespace Trsys.Models.ReadModel.Dtos
{
    public class UserPasswordHashDto
    {
        public Guid Id { get; set; }
        public string PasswordHash { get; set; }
    }
}
