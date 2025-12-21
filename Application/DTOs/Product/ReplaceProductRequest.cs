using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Product
{
    public class ReplaceProductRequest
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }

     
        public List<IFormFile>? Images { get; set; }

        public string Offer { get; set; }
        public int Rating { get; set; }
        public bool Featured { get; set; }
    }
}
