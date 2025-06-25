using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using AutoMapper;
using Base.BLL;
using Topic = App.BLL.DTO.Topic;

namespace App.BLL.Services;

public class TopicService :
    BaseEntityService<App.DAL.DTO.Topic, App.BLL.DTO.Topic, ITopicRepository>,
    ITopicService
{
    public TopicService(IAppUnitOfWork uoW, ITopicRepository repository, IMapper mapper) : base(uoW,
        repository, new BllDalMapper<App.DAL.DTO.Topic, App.BLL.DTO.Topic>(mapper))
    {
    }
    
    
    public async Task<IEnumerable<Topic>> GetAllSortedAsync(Guid userId)
    {
        return (await Repository.GetAllSortedAsync(userId)).Select(e => Mapper.Map(e));
    }

    public async Task<IEnumerable<Topic>> GetAllByDiscussionIdAsync(Guid id)
    {
        return (await Repository.GetAllByDiscussionIdAsync(id)).Select(e => Mapper.Map(e));
    }

    public async Task<IEnumerable<Topic>> GetAllIncludeAll()
    {
        return (await Repository.GetAllIncludeAll()).Select(e => Mapper.Map(e));
    }

    public async Task<Topic?> FirstOrDefaultIncludeAllAsync(Guid id)
    {
        return Mapper.Map(await Repository.FirstOrDefaultIncludeAllAsync(id));
    }
}