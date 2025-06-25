using App.Contracts.BLL.Services;
using App.Domain.Identity;
using Base.Contracts.BLL;

namespace App.Contracts.BLL;

public interface IAppBLL : IBLL
{
    IMessageService Messages { get; }
    IRatingService Ratings { get; }
    ITopicService Topics { get; }
    IEntityService<AppUser> Users { get; }
}