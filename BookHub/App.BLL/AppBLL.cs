using App.BLL.Services;
using App.Contracts.BLL;
using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.DAL.EF;
using App.Domain.Identity;
using AutoMapper;
using Base.BLL;
using Base.Contracts.BLL;
using Base.Contracts.DAL;

namespace App.BLL;

public class AppBLL : BaseBLL<AppDbContext>, IAppBLL
{
    private readonly IMapper _mapper;
    private readonly IAppUnitOfWork _uow;
    
    public AppBLL(IAppUnitOfWork uoW, IMapper mapper) : base(uoW)
    {
        _mapper = mapper;
        _uow = uoW;
    }
    
    private IMessageService? _messages;
    public IMessageService Messages => _messages ?? new MessageService(_uow, _uow.Messages, _mapper);
    
    private IRatingService? _ratings;
    public IRatingService Ratings => _ratings ?? new RatingService(_uow, _uow.Ratings, _mapper);
    
    private ITopicService? _topics;
    public ITopicService Topics => _topics ?? new TopicService(_uow, _uow.Topics, _mapper);
    
    private IEntityService<AppUser>? _users;

    public IEntityService<AppUser> Users => _users ??
                                               new BaseEntityService<AppUser, AppUser, IEntityRepository<AppUser>>(_uow, _uow.Users, 
                                                   new BllDalMapper<AppUser, AppUser>(_mapper));
}