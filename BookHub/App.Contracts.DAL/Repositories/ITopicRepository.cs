using DALDTO = App.DAL.DTO;
using APPDomain = App.Domain.Entities;
using Base.Contracts.DAL;

namespace App.Contracts.DAL.Repositories;

public interface ITopicRepository : IEntityRepository<DALDTO.Topic>, ITopicRepositoryCustom<DALDTO.Topic>
{
    // define your custom methods here
    Task<IEnumerable<DALDTO.Topic>> GetAllByDiscussionIdAsync(Guid id);
    Task<IEnumerable<DALDTO.Topic>> GetAllIncludeAll();
    Task<DALDTO.Topic?> FirstOrDefaultIncludeAllAsync(Guid id);
}

// define your shared (with bll) custom methods here
public interface ITopicRepositoryCustom<TEntity>
{
    Task<IEnumerable<TEntity>> GetAllSortedAsync(Guid userId);
    Task<IEnumerable<TEntity>> GetAllByDiscussionIdAsync(Guid id);
    Task<IEnumerable<TEntity>> GetAllIncludeAll();
    Task<TEntity?> FirstOrDefaultIncludeAllAsync(Guid id);
}
