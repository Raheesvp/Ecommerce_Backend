using System;
using System.Collections.Generic;

namespace Application.DTOs.Product
{
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; } // It is good practice to include this

        // 1. Default Empty Constructor
        public PagedResponse() { }

        // 2. The Constructor your Service is looking for (Fixes CS1729)
        public PagedResponse(List<T> items, int pageNumber, int pageSize, int totalCount)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;

            // Optional: Calculate total pages automatically
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }
    }
}