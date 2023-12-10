namespace Trsys.Models.WriteModel.Commands
{
    public class UserCreateIfNotExistsCommand : UserCreateCommand
    {
        public UserCreateIfNotExistsCommand(string name, string username, string emailAddress, string passwordHash, string role) : base(name, username, emailAddress, passwordHash, role)
        {
        }
    }
}
