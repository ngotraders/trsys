using System;

namespace Trsys.Web.Models.WriteModel.Infrastructure
{
    public interface ITokenConnectionManager
    {
        void Add(string token, Guid id);
        void Remove(string token);
        void Touch(string token);
    }
}
