using EcommerceDataLayer;
using EcommerceDataLayer.Helpers;
using System;
using System.Collections.Generic;

using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EcommerceBuisnessLayer
{
    public interface IProductsRepostiory
    {
        Task<PagedList<ProductDTO>> GetAllProducts(PaginationParams Params);
        Task<int> AddNewProduct(ProductDTO newproduct);
        Task<bool> CheckProductNameExist(string productName, int? ExcludeId = null);

        Task<ProductDTO?> GetProductById(int Id);
        Task<bool> IsProductExistsById(int id);
        Task<int> UpdatedProduct(ProductDTO product);

        Task<int> DeleteProduct(int id);
    }
    public class ProductsRepository: IProductsRepostiory
    {
        public async Task<PagedList<ProductDTO>> GetAllProducts(PaginationParams Params)
            => await ProductsData.GetAllProductsPaged(Params);
        public  async Task<int> AddNewProduct(ProductDTO newproduct)
            => await ProductsData.AddNewProduct(newproduct);
        public  async Task<bool> CheckProductNameExist(string productName, int? ExcludeId = null)
            =>await ProductsData.CheckProductNameExists(productName, ExcludeId);

        public  async Task<ProductDTO?> GetProductById(int Id)
            => await ProductsData.GetProductById(Id);

        public async Task<bool> IsProductExistsById(int id)
            => await ProductsData.IsProductExistById(id);

        public  async Task<int> UpdatedProduct(ProductDTO product)
            => await ProductsData.UpdateProducts(product);
        public async Task <int> DeleteProduct(int id)
            => await ProductsData.DeleteProduct(id);
    }
    public interface IProductService
    {
        Task<PagedList<ProductDTO>> GetAllProducts(PaginationParams Params);
        Task<ProductOperationResult> AddNewProduct(ProductDTO newproduct);
        Task<ProductDTO?> GetProductById(int Id);
        Task<ProductOperationResult> UpdatedProduct(ProductDTO product);

        Task<ProductOperationResult> DeleteProduct(int id);

    }
    public class ProductService: IProductService
    {
        private readonly IProductsRepostiory _repostiory;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductsRepostiory productRepostiory, ILogger<ProductService> logger)
        {
            _repostiory = productRepostiory;
            _logger = logger;
        }
        public async Task<PagedList<ProductDTO>> GetAllProducts(PaginationParams Params)
            => await _repostiory.GetAllProducts(Params);
        public async  Task<ProductOperationResult> AddNewProduct(ProductDTO newproduct)
        {
            _logger.LogInformation("Service: Attempting to add product {Name}", newproduct?.Name);

            if (newproduct == null || string.IsNullOrWhiteSpace(newproduct.Name))
            {
                _logger.LogWarning("Service: Invalid product data");
                return ProductOperationResult.InvalidData;
            }
            if (await _repostiory.CheckProductNameExist(newproduct.Name))
            {
                _logger.LogWarning("Service: Duplicate product name detected: {Name}", newproduct.Name);

                return ProductOperationResult.DuplicateName;
            }
            int NewId= await _repostiory.AddNewProduct(newproduct);
            if (NewId <= 0)
            {
                _logger.LogError("Service: Failed to insert product into DB");

                return ProductOperationResult.Failed;
            }
            newproduct.ID = NewId;
            _logger.LogInformation("Service: Product added successfully with ID {Id}", NewId);
            return ProductOperationResult.Success;
        }
        public async Task<ProductDTO?> GetProductById(int Id)
        =>await _repostiory.GetProductById(Id);
      public async  Task<ProductOperationResult> UpdatedProduct(ProductDTO product)
        {
            _logger.LogInformation("Service: Updating product ID {Id}", product?.ID);

            if (product == null || product.ID < 1 || string.IsNullOrWhiteSpace(product.Name))
            {
                _logger.LogWarning("Service: Invalid product data for update");

                return ProductOperationResult.InvalidData;
            }
            if (!await _repostiory.IsProductExistsById(product.ID))
            {
                _logger.LogWarning("Service: Product not found for update. ID {Id}", product.ID);

                return ProductOperationResult.NotFound;
            }
            if (await _repostiory.CheckProductNameExist(product.Name, product.ID))
            {
                _logger.LogWarning("Service: Duplicate name detected during update. Name {Name}", product.Name);

                return ProductOperationResult.DuplicateName;
            }
            int result=await _repostiory.UpdatedProduct(product);
            return  result==1?ProductOperationResult.Success : ProductOperationResult.Failed;
        }
        public async  Task<ProductOperationResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Service: Attempting to delete product ID {Id}", id);

            if (id < 1)
            {
                _logger.LogWarning("Service: Invalid product ID {Id}", id);

                return ProductOperationResult.NotFound;
            }
            int result= await _repostiory.DeleteProduct(id);
            return result == 1 ? ProductOperationResult.Success :ProductOperationResult.Failed;

        }
    }
    public enum ProductOperationResult
    {
        Success = 1,
        NotFound = 0,
        DuplicateName = -1,
        HasChildren = -2,
        HasProducts = -3,
        InvalidData = -4,
        Failed = -99
    }
    
}
