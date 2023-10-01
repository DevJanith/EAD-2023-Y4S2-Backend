using Rest.Entities;

namespace Rest.Repositories
{
    public interface IProductService
    {
        public Task<List<ProductDetails>> ProductListAsync();
        public Task<ProductDetails> GetProductDetailByIdAsync(string productId);
        public Task AddProductAsync(ProductDetails productDetails);
        public Task UpdateProductAsync(string productId, ProductDetails productDetails);
        public Task DeleteProductAsync(string productId);
    }
}
