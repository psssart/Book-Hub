using App.Contracts.DAL.Repositories;
using App.Domain.Identity;
using Base.Contracts.DAL;

namespace App.Contracts.DAL;

public interface IAppUnitOfWork : IUnitOfWork
{
    // list your repos here

    ITopicRepository Topics { get; }
    IMessageRepository Messages { get; }
    IRatingRepository Ratings { get; }
    IEntityRepository<AppUser> Users { get; }
}