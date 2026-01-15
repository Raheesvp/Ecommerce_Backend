using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reviews
{
    public class CreateReviewRequest
    {
        public int ProductId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }
        public List<IFormFile> ReviewImages { get; set; }
    }

    public class ReviewResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public List<string> ImageUrls { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
