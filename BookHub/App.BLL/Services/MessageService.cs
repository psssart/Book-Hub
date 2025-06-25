using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DAL.DTO;
using AutoMapper;
using Base.BLL;
using Message = App.BLL.DTO.Message;

namespace App.BLL.Services;

public class MessageService :
    BaseEntityService<App.DAL.DTO.Message, App.BLL.DTO.Message, IMessageRepository>,
    IMessageService
{
    public MessageService(IAppUnitOfWork uoW, IMessageRepository repository, IMapper mapper) : base(uoW,
        repository, new BllDalMapper<App.DAL.DTO.Message, App.BLL.DTO.Message>(mapper))
    {
    }
    
    
    public async Task<IEnumerable<Message>> GetAllSortedAsync(Guid userId)
    {
        return (await Repository.GetAllSortedAsync(userId)).Select(e => Mapper.Map(e));
    }

    public async Task<IEnumerable<Message>> GetAllByTopicsAsync(IEnumerable<Topic>? topics)
    {
        return (await Repository.GetAllByTopicsAsync(topics)).Select(e => Mapper.Map(e));
    }

    public async Task<IEnumerable<Message>> GetAllIncludeAll()
    {
        return (await Repository.GetAllIncludeAll()).Select(e => Mapper.Map(e));
    }

    public async Task<Message?> FirstOrDefaultIncludeAllAsync(Guid id)
    {
        return Mapper.Map(await Repository.FirstOrDefaultIncludeAllAsync(id));
    }

    public async Task<IEnumerable<Message>> GetAllBy1TopicIncludeUserAsync(Guid topicId)
    {
        return (await Repository.GetAllBy1TopicIncludeUserAsync(topicId)).Select(e => Mapper.Map(e));
    }
}