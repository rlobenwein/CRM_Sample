using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RLBW_ERP
{
    public class PaginetedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; set; }
        public string Status { get; set; }

        public PaginetedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            AddRange(items);
        }
        public bool HasPreviousPage
        {
            get
            {
                return PageIndex > 1;
            }
        }
        public bool HasNextPage
        {
            get
            {
                return PageIndex < TotalPages;
            }
        }
        public static async Task<PaginetedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int? pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * (int)pageSize).Take((int)pageSize).ToListAsync();
            return new PaginetedList<T>(items, count, pageIndex, (int)pageSize);

        }
        public static PaginetedList<T> Create(IQueryable<T> source, int pageIndex, int? pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * (int)pageSize).Take((int)pageSize).ToList();
            return new PaginetedList<T>(items, count, pageIndex, (int)pageSize);

        }
    }
}
