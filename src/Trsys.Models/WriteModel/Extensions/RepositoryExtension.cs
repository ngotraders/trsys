using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using System.Threading.Tasks;
using Trsys.Models.WriteModel.Domain;

namespace Trsys.Models.WriteModel.Extensions
{
    public static class RepositoryExtension
    {
        public static async Task<WorldStateAggregate> GetWorldState(this IRepository repository)
        {
            try
            {
                return await repository.Get<WorldStateAggregate>(WorldStateAggregate.WORLD_STATE_ID);
            }
            catch (AggregateNotFoundException)
            {
                var state = new WorldStateAggregate(WorldStateAggregate.WORLD_STATE_ID);
                await repository.Save(state);
                return state;
            }
        }
    }
}
