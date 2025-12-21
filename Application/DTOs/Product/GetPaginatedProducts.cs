//using Application.Contracts.Repositories;
//using Application.DTOs.Product;
//using Application.Interfaces;
//using System.ComponentModel.DataAnnotations;

//public class GetPaginatedProducts
//{
//    private readonly IProductRepository _repository;

//    public GetPaginatedProducts(IProductRepository repository)
//    {
//        _repository = repository;
//    }

//    public async Task<PagedResponse<ProductResponse>> ExecuteAsync(
//        int pageNumber,
//        int pageSize)
//    {
//        if (pageNumber < 1)
//            throw new ValidationException("PageNumber must be >= 1");

//        if (pageSize < 1 || pageSize > 100)
//            throw new ValidationException("PageSize must be between 1 and 100");

//        var products = await _repository.GetPaginatedAsync(pageNumber, pageSize);
//        var totalCount = await _repository.GetTotalAsync();

//        return new PagedResponse<ProductResponse>
//        {
//            PageNumber = pageNumber,
//            PageSize = pageSize,
//            TotalCount = totalCount,
//            Items = products.Select(p => new ProductResponse
//            {
//                Id = p.Id,
//                Name = p.Name,
//                Price = p.Price,
//                Stock = p.Stock,
//                Category = p.Category
//            }).ToList()
//        };
//    }
//}
