using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Product
{
    public  class ProductFilterRequest
    {
      public bool? Featured { get; set; }

      public string? SortBy { get; set; }

      public string? Order { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 5;
    }
}
