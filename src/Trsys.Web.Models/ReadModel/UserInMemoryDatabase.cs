using System;
using System.Collections.Generic;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models
{
    public class UserInMemoryDatabase
    {
        public readonly List<UserDto> List = new();
        public readonly Dictionary<Guid, UserDto> ById = new();
        public readonly Dictionary<string, UserDto> ByUsername = new();

        public void Add(UserDto userDto)
        {
            ById.Add(userDto.Id, userDto);
            ByUsername.Add(userDto.Username, userDto);
            List.Add(userDto);
        }

        public void Remove(Guid id)
        {
            var item = ById[id];
            ById.Remove(id);
            ByUsername.Remove(item.Username);
            List.RemoveAt(List.IndexOf(item));
        }
    }
}
