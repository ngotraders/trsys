namespace Trsys.Web.Models.WriteModel.Commands
{
    public class CreateUserIfNotExistsCommand : CreateUserCommand
    {
        public CreateUserIfNotExistsCommand(string name, string username, string passwordHash) : base(name, username, passwordHash)
        {
        }
    }
}
