using System;
using System.Collections.Generic;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models
{
    public static class SecretKeyInMemoryDatabase
    {
        public static readonly List<SecretKeyDto> List = new();
        public static readonly Dictionary<Guid, SecretKeyDto> Map = new();

        public static void Add(SecretKeyDto secretKeyDto)
        {
            Map.Add(secretKeyDto.Id, secretKeyDto);
            List.Add(secretKeyDto);
        }

        public static void Remove(Guid id)
        {
            var item = Map[id];
            Map.Remove(id);
            List.RemoveAt(List.IndexOf(item));
        }
    }
}
