using App.Contracts.DAL.Repositories;
using Base.Contracts.DAL;

namespace App.Contracts.BLL.Services;

public interface ITopicService : IEntityRepository<App.BLL.DTO.Topic>, ITopicRepositoryCustom<App.BLL.DTO.Topic>
{
    
}