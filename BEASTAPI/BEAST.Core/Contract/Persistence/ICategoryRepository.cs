using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence;

public interface ICategoryRepository
{
    Task<PaginatedListModel<CategoryModel>> GetCategories(int pageNumber);
    Task<List<CategoryModel>> GetDistinctCategories();
    Task<CategoryModel> GetCategoryById(string categoryId);
    Task<CategoryModel> GetCategoryByName(string categoryName);
    Task<string> InsertCategory(CategoryModel category, LogModel logModel);
    Task UpdateCategory(CategoryModel category, LogModel logModel);
    Task DeleteCategory(string categoryId, LogModel logModel);
    Task<List<CategoryModel>> GetCategoriesWithPies();
    Task<List<CategoryModel>> Export();
}