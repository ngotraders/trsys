namespace Trsys.Models.WriteModel.Commands
{
    public class UserCreateIfNotExistsCommand : UserCreateCommand
    {
        public UserCreateIfNotExistsCommand(string name, string username, string passwordHash, string role) : base(name, username, passwordHash, role)
        {
        }
    }
}
