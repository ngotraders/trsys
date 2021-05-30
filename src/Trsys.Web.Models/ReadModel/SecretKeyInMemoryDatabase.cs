using System;
using System.Collections.Generic;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models
{
    public class SecretKeyInMemoryDatabase
    {
        public readonly List<SecretKeyDto> List = new();
        public readonly Dictionary<Guid, SecretKeyDto> ById = new();
        public readonly Dictionary<string, SecretKeyDto> ByKey = new();
        public readonly Dictionary<string, SecretKeyDto> ByToken = new();

        public void Add(SecretKeyDto secretKeyDto)
        {
            ById.Add(secretKeyDto.Id, secretKeyDto);
            ByKey.Add(secretKeyDto.Key, secretKeyDto);
            List.Add(secretKeyDto);
        }

        public void Remove(Guid id)
        {
            var item = ById[id];
            ById.Remove(id);
            ByKey.Remove(item.Key);
            List.RemoveAt(List.IndexOf(item));
        }
    }
}
