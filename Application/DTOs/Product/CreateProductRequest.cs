using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Product
{
    public class CreateProductRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Category { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; }
        [Required]
 

        public List<IFormFile> Images { get; set; } = new();
      
        public string Offer { get; set; } = string.Empty;
        [Range(0,5)]
        public int Rating { get; set; }
        [Required]
        public decimal  OriginalPrice { get; set; }

        public bool Featured { get; set; }
        [Required]
        public int Stock { get; set; }


    }
}
