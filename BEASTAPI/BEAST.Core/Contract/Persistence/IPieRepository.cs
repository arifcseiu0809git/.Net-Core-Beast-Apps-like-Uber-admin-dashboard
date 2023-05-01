using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence;

public interface IPieRepository
{
    Task<PaginatedListModel<PieModel>> GetPies(int pageNumber);
    Task<PieModel> GetPieById(string pieId);
    Task<List<PieModel>> GetPieByCategoryId(string categoryId);
    Task<PieModel> GetPieByName(string pieName);
    Task<string> InsertPie(PieModel pie, LogModel logModel);
    Task UpdatePie(PieModel pie, LogModel logModel);
    Task DeletePie(string pieId, LogModel logModel);
    Task<List<PieModel>> Export();
}