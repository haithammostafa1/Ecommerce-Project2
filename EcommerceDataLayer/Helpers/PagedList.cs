using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceDataLayer.Helpers
{
    public class PagedList<T> : List<T>
    {
        public PaginationMetaData MetaData { get; set; }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            MetaData = new PaginationMetaData(count, pageNumber, pageSize);
            AddRange(items);
        }
    }
}
