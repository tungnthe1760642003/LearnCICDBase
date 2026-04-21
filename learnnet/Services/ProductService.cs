using learnnet.DTOs;
using learnnet.Entities;
using learnnet.Exceptions;
using learnnet.Repositories;

namespace learnnet.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProductReadDto>> GetAllProductsAsync()
        {
            var products = await _repository.GetAllAsync();
            return products.Select(MapToReadDto);
        }

        public async Task<ProductReadDto> GetProductByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {id} not found.");
            }
            return MapToReadDto(product);
        }

        public async Task<ProductReadDto> CreateProductAsync(ProductCreateDto productCreateDto)
        {
            var product = new Product
            {
                Name = productCreateDto.Name,
                Price = productCreateDto.Price,
                Stock = productCreateDto.Stock,
                Details = productCreateDto.Details != null ? new ProductDetail
                {
                    Description = productCreateDto.Details.Description,
                    Manufacturer = productCreateDto.Details.Manufacturer,
                    WarrantyPeriodMonths = productCreateDto.Details.WarrantyPeriodMonths
                } : null
            };

            await _repository.CreateAsync(product);
            await _repository.SaveChangesAsync();

            return MapToReadDto(product);
        }

        public async Task UpdateProductAsync(int id, ProductCreateDto productUpdateDto)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {id} not found.");
            }

            product.Name = productUpdateDto.Name;
            product.Price = productUpdateDto.Price;
            product.Stock = productUpdateDto.Stock;

            if (productUpdateDto.Details != null)
            {
                if (product.Details == null)
                {
                    product.Details = new ProductDetail();
                }
                product.Details.Description = productUpdateDto.Details.Description;
                product.Details.Manufacturer = productUpdateDto.Details.Manufacturer;
                product.Details.WarrantyPeriodMonths = productUpdateDto.Details.WarrantyPeriodMonths;
            }

            await _repository.UpdateAsync(product);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {id} not found.");
            }

            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();
        }

        private static ProductReadDto MapToReadDto(Product product)
        {
            return new ProductReadDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                CreatedAt = product.CreatedAt,
                Details = product.Details != null ? new ProductDetailDto
                {
                    Description = product.Details.Description,
                    Manufacturer = product.Details.Manufacturer,
                    WarrantyPeriodMonths = product.Details.WarrantyPeriodMonths
                } : null
            };
        }
    }
}
