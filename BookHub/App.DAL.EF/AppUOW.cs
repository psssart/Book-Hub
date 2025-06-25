using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DAL.EF.Repositories;
using App.Domain.Identity;
using AutoMapper;
using Base.Contracts.DAL;
using Base.DAL.EF;

namespace App.DAL.EF;

public class AppUOW : BaseUnitOfWork<AppDbContext>, IAppUnitOfWork
{
    private readonly IMapper _mapper;
    public AppUOW(AppDbContext dbContext, IMapper mapper) : base(dbContext)
    {
        _mapper = mapper;
    }

    private ITopicRepository? _topics;
    public ITopicRepository Topics => _topics ?? new TopicRepository(UowDbContext, _mapper);
    
    private IMessageRepository? _messages;
    public IMessageRepository Messages => _messages ?? new MessageRepository(UowDbContext, _mapper);
    
    private IRatingRepository? _ratings;
    public IRatingRepository Ratings => _ratings ?? new RatingRepository(UowDbContext, _mapper);

    
    private IEntityRepository<AppUser>? _users;

    public IEntityRepository<AppUser> Users => _users ??
                                               new BaseEntityRepository<AppUser, AppUser, AppDbContext>(UowDbContext,
                                                   new DalDomainMapper<AppUser, AppUser>(_mapper));
}