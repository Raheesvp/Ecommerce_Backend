using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Mscc.GenerativeAI.Types;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public int Rating { get; set; } 
    public string Comment { get; set; }

    public List<ReviewImage> ReviewImages { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  
}