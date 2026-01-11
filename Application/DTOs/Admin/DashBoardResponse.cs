namespace Application.DTOs.Admin
{
    public class DashBoardResponse
    {
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TodayOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public List<RecentOrderDTO> RecentOrders { get; set; } = new();
        public List<RecentStockDTO> TopSellingProducts { get; set; } = new();
        public List<ProductStockDTO> LowStockProducts { get; set; } = new();
        public List<SalesChartDTO> SalesHistory { get; set; } = new();
    }

    public class RecentOrderDTO
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }

    public class RecentStockDTO
    {
        public string ProductName { get; set; }
        public int Value { get; set; } 
    }

    public class ProductStockDTO
    {
        public string ProductName { get; set; }
        public int Value { get; set; } 
    }

    public class SalesChartDTO
    {
        public string Date { get; set; }
        public decimal Revenue { get; set; }
    }
}