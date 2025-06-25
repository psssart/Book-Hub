using DALDTO = App.DAL.DTO;
using APPDomain = App.Domain.Entities;
using Base.Contracts.DAL;

namespace App.Contracts.DAL.Repositories;

public interface IRatingRepository : IEntityRepository<DALDTO.Rating>, IRatingRepositoryCustom<DALDTO.Rating>
{
    // implement your custom methods here
    Task<IEnumerable<DALDTO.Rating>> GetAllByBookIdAsync(Guid id);
    Task<DALDTO.Rating?> FirstOrDefaultIncludeAllAsync(Guid id);
}

// define your shared (with bll) custom methods here
public interface IRatingRepositoryCustom<TEntity>
{
    Task<IEnumerable<TEntity>> GetAllSortedAsync(Guid userId);
    Task<IEnumerable<TEntity>> GetAllByBookIdAsync(Guid id);
    Task<TEntity?> FirstOrDefaultIncludeAllAsync(Guid id);
}
