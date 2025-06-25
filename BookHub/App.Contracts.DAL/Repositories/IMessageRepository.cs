using DALDTO = App.DAL.DTO;
using APPDomain = App.Domain.Entities;
using Base.Contracts.DAL;

namespace App.Contracts.DAL.Repositories;

public interface IMessageRepository : IEntityRepository<DALDTO.Message>, IMessageRepositoryCustom<DALDTO.Message>
{
    // define your DAL only custom methods here
    Task<IEnumerable<DALDTO.Message>> GetAllByTopicsAsync(IEnumerable<DALDTO.Topic>? topics);
    Task<IEnumerable<DALDTO.Message>> GetAllIncludeAll();
    Task<DALDTO.Message?> FirstOrDefaultIncludeAllAsync(Guid id);
    Task<IEnumerable<DALDTO.Message>> GetAllBy1TopicIncludeUserAsync(Guid topicId);
}

// define your shared (with bll) custom methods here
public interface IMessageRepositoryCustom<TEntity>
{
    Task<IEnumerable<TEntity>> GetAllSortedAsync(Guid userId);
    Task<IEnumerable<TEntity>> GetAllByTopicsAsync(IEnumerable<DALDTO.Topic>? topics);
    Task<IEnumerable<TEntity>> GetAllIncludeAll();
    Task<TEntity?> FirstOrDefaultIncludeAllAsync(Guid id);
    Task<IEnumerable<TEntity>> GetAllBy1TopicIncludeUserAsync(Guid topicId);
}