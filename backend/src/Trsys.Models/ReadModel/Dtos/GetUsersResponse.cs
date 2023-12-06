using System.Collections.Generic;

namespace Trsys.Models.ReadModel.Dtos
{
    public class SearchResponseDto<T>
    {
        public SearchResponseDto()
        {
        }

        public SearchResponseDto(int totalCount, List<T> items)
        {
            TotalCount = totalCount;
            Items = items;
        }

        public int TotalCount { get; }
        public List<T> Items { get; } = new();
    }
}