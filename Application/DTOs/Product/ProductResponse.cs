using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Product
{
    public  class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public String Description { get; set; }

        public string Category { get; set; }


        public List<string> Images { get; set; }
    }
}
