using App.Contracts.DAL.Repositories;
using Base.Contracts.DAL;

namespace App.Contracts.BLL.Services;

public interface IRatingService : IEntityRepository<App.BLL.DTO.Rating>, IRatingRepositoryCustom<App.BLL.DTO.Rating>
{
    
}