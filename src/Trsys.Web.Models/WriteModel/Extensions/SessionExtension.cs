using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Domain;

namespace Trsys.Web.Models.WriteModel.Extensions
{
    public static class SessionExtension
    {
        public static async Task<WorldStateAggregate> GetWorldState(this ISession session)
        {
            try
            {
                return await session.Get<WorldStateAggregate>(WorldStateAggregate.WORLD_STATE_ID);
            }
            catch (AggregateNotFoundException)
            {
                var state = new WorldStateAggregate(WorldStateAggregate.WORLD_STATE_ID);
                await session.Add(state);
                return state;
            }
        }
    }
}
