using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rest.Configurations;
using Rest.Entities;

namespace Rest.Repositories
{
    public class ProductService : IProductService
    {
        private readonly IMongoCollection<ProductDetails> productCollection;

        public ProductService(IOptions<ProductDBSettings> productDatabasSettings)
        {
            var mongoClient = new MongoClient(productDatabasSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(productDatabasSettings.Value.DatabaseName);
            productCollection = mongoDatabase.GetCollection<ProductDetails>(productDatabasSettings.Value.ProductCollectionName);
        }

        public async Task<List<ProductDetails>> ProductListAsync()
        {
            return await productCollection.Find(_ => true).ToListAsync();
        }
        public async Task<ProductDetails> GetProductDetailByIdAsync(string productId)
        {
            return await productCollection.Find(x => x.Id == productId).FirstOrDefaultAsync();
        }
        public async Task AddProductAsync(ProductDetails productDetails)
        {
            await productCollection.InsertOneAsync(productDetails);
        }

        public async Task UpdateProductAsync(string productId, ProductDetails productDetails)
        {
            await productCollection.ReplaceOneAsync(x => x.Id == productId, productDetails);
        }

        public async Task DeleteProductAsync(string productId)
        {
            await productCollection.DeleteOneAsync(x => x.Id == productId);
        }
    }
}
