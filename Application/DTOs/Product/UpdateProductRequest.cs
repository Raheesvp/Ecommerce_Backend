using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Product
{
    public  class UpdateProductRequest
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public string? Category { get; set; }

        public IFormFile? Image { get; set; }
        public List<IFormFile>? Images { get; set; }

        public int? Offer { get; set; }
        public bool? Featured { get; set; }

        public string? Description { get; set; } 
     
      


    }
}
