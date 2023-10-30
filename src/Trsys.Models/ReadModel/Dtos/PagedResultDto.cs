using System;
using System.Collections.Generic;

namespace Trsys.Models.ReadModel.Dtos
{
    public class PagedResultDto<T>
    {
        public PagedResultDto(int page, int perPage, int totalCount, List<T> list)
        {
            TotalCount = totalCount;
            Page = page;
            PerPage = perPage;
            List = list;
        }

        public int TotalCount { get; set; }
        public int Page { get; }
        public int PerPage { get; }
        public List<T> List { get; set; }
    }
}
