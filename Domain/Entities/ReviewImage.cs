using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public  class ReviewImage
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int ReviewId { get; set; }
        public Review Review { get; set; }
    }
}
