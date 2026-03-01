
using EcommerceDataLayer;
using EcommerceDataLayer.Helpers;

using Microsoft.Extensions.Logging;
namespace EcommerceBuisnessLayer
{
    public enum CategoryOperationResult
    {
        Success = 1,
        NotFound = 0,
        DuplicateName = -1,
        HasChildren = -2,
        HasProducts = -3,
        InvalidData = -4,
        Failed = -99
    }

    public interface ICategoriesRepository
    {
        Task<PagedList<CategoryDto>> GetCategoriesPaged(PaginationParams paginationParams);

        Task<CategoryDto?> GetCategoryById(int id);
        Task<int> AddNewCategory(CategoryDto dto);
        Task<int> UpdateCategory(CategoryDto dto);
        Task<int> DeleteCategory(int id);
        Task<bool> CheckCategoryNameExists(string CategoryName, int? excludeId = null);
        Task<bool> CheckCategoryIdExists(int id);
    }
    public class CategoriesRepository : ICategoriesRepository
    {
        public async Task<PagedList<CategoryDto>> GetCategoriesPaged(PaginationParams paginationParams)
        => await CategoriesData.GetCategoriesPagedAsync(paginationParams);
        public async Task<CategoryDto?> GetCategoryById(int id)
        => await CategoriesData.GetCategoryById(id);
        public async Task<int> AddNewCategory(CategoryDto dto)
        => await CategoriesData.AddNewCategory(dto);

        public async Task<int> UpdateCategory(CategoryDto dto)
       => await CategoriesData.UpdateCategory(dto);
        public async Task<int> DeleteCategory(int id)
            => await CategoriesData.DeleteCategory(id);
        public async Task<bool> CheckCategoryNameExists(string CategoryName, int? excludeId = null)
            => await CategoriesData.CategoryExists(CategoryName, excludeId);
        public async Task<bool> CheckCategoryIdExists(int id)
            => await CategoriesData.IsCategoryExist(id);
    }
    public interface ICategoriesService
    {
        Task<PagedList<CategoryDto>> GetCategoriesPaged(PaginationParams paginationParams);
        Task<CategoryDto?> GetCategoryById(int id);
        Task<CategoryOperationResult> AddNewCategory(CategoryDto dto);
        Task<CategoryOperationResult> UpdateCategory(CategoryDto dto);
        Task<CategoryOperationResult> DeleteCategory(int id);

    }

    public class CategoriesService : ICategoriesService
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ILogger<CategoriesService> _logger;
        public CategoriesService(ICategoriesRepository categoriesRepository, ILogger<CategoriesService> logger )
        {
            _categoriesRepository = categoriesRepository;
            _logger = logger;
        }

        public async Task<PagedList<CategoryDto>> GetCategoriesPaged(PaginationParams paginationParams)
        {

            _logger.LogInformation("Service: Fetching paged categories. Page: {Page}, Size: {Size}",
          paginationParams.PageNumber,
             paginationParams.PageSize);

            return   await _categoriesRepository.GetCategoriesPaged(paginationParams);



        }

        public async Task<CategoryDto?> GetCategoryById(int id)
        {
            _logger.LogInformation("Service: Fetching category ID {Id}", id);

            var category= await _categoriesRepository.GetCategoryById(id);
            if (category == null)
            {
                _logger.LogWarning("Service: Category not found with ID {Id}", id);

            }

            return category;
        }
        public async Task<CategoryOperationResult> AddNewCategory(CategoryDto dto)
        {
            _logger.LogInformation("Service: Attempting to add category {Name}", dto?.Name);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogWarning("Service: Invalid category data");

                return CategoryOperationResult.InvalidData;
            }
            if (await _categoriesRepository.CheckCategoryNameExists(dto.Name))
            {
                _logger.LogWarning("Service: Duplicate category name detected {Name}", dto.Name);

                return CategoryOperationResult.DuplicateName;
            }
            int NewId = await _categoriesRepository.AddNewCategory(dto);
            if (NewId <= 0)
            {
                _logger.LogError("Service: Failed to insert category into DB");

                return CategoryOperationResult.Failed;
            }
            dto.Id = NewId;
            _logger.LogInformation("Service: Category added successfully with ID {Id}", NewId);

            return CategoryOperationResult.Success;

        }
        public async Task<CategoryOperationResult> UpdateCategory(CategoryDto dto)
        {
            _logger.LogInformation("Service: Attempting to update category ID {Id}", dto?.Id);

            if (dto == null || dto.Id < 1)
            {
                _logger.LogWarning("Service: Invalid update data");

                return CategoryOperationResult.InvalidData;
            }

            if (!await _categoriesRepository.CheckCategoryIdExists(dto.Id))
            {
                _logger.LogWarning("Service: Category not found for update ID {Id}", dto.Id);

                return CategoryOperationResult.NotFound;
            }
            // Rule: Name must be unique 
            if (await _categoriesRepository.CheckCategoryNameExists(dto.Name, dto.Id))
            {
                _logger.LogWarning("Service: Duplicate name detected during update {Name}", dto.Name);

                return CategoryOperationResult.DuplicateName;
            }

            int result = await _categoriesRepository.UpdateCategory(dto);

            return result == 1 ? CategoryOperationResult.Success : CategoryOperationResult.Failed;

        }

        public async Task<CategoryOperationResult> DeleteCategory(int id)
        {
            _logger.LogInformation("Service: Attempting to delete category ID {Id}", id);

            if (id < 1)
            {
                _logger.LogWarning("Service: Invalid category ID {Id}", id);

                return CategoryOperationResult.InvalidData;

            }
            int result = await _categoriesRepository.DeleteCategory(id);
            return result switch
            {
                1 => CategoryOperationResult.Success,
                0 => CategoryOperationResult.NotFound,
                -1 => CategoryOperationResult.HasProducts,
                -2 => CategoryOperationResult.HasChildren,
                _ => CategoryOperationResult.Failed
            };
        }

    }
}
    

