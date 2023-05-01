using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;
using BEASTAPI.Core.ViewModel;

namespace BEASTAPI.Core.Contract.Persistence;

public interface IXDriverVehicleRepository
{
    Task<PaginatedListModel<XDriverVehicleViewModel>> GetXDriverVehicleModels(int pageNumber);
    Task<XDriverVehicleReadViewModel> GetXDriverVehicleModelById(string xDriverVehicleModelId);
    Task<string> InsertXDriverVehicleModel(XDriverVehicleModel xDriverVehicleModel, LogModel logModel);
    Task UpdateXDriverVehicleModel(XDriverVehicleModel xDriverVehicleModel, LogModel logModel);
    Task DeleteXDriverVehicleModel(string xDriverVehicleModelId, LogModel logModel);
    Task<List<XDriverVehicleViewModel>> Export();
    Task<List<XDriverVehicleReadViewModel>> GetDriversBySearchPrefix(string prefix);
    Task<List<XDriverVehicleReadViewModel>> GetVehiclesBySearchPrefix(string prefix);
}