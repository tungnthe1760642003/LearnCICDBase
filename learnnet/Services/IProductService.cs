using learnnet.DTOs;

namespace learnnet.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductReadDto>> GetAllProductsAsync();
        Task<ProductReadDto> GetProductByIdAsync(int id);
        Task<ProductReadDto> CreateProductAsync(ProductCreateDto productCreateDto);
        Task UpdateProductAsync(int id, ProductCreateDto productUpdateDto);
        Task DeleteProductAsync(int id);
    }
}
