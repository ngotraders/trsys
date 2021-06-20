using System;
using System.Threading.Tasks;

namespace Trsys.Web.Models.WriteModel.Infrastructure
{
    public interface ITokenConnectionManager
    {
        Task InitializeAsync();
        void Add(string token, Guid id);
        void Remove(string token);
        void Touch(string token);
    }
}
