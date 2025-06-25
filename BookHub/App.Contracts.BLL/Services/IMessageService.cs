using App.Contracts.DAL.Repositories;
using Base.Contracts.DAL;

namespace App.Contracts.BLL.Services;

public interface IMessageService : IEntityRepository<App.BLL.DTO.Message>, IMessageRepositoryCustom<App.BLL.DTO.Message>
{
    
}