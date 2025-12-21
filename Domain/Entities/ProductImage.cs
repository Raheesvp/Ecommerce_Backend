using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public  class ProductImage : BaseEntity
    {

        public string Url { get; set; }

        public int ProductId { get; set; }

    }
}
