using App.Contracts.DAL.Repositories;
using AutoMapper;
using DALDTO = App.DAL.DTO;
using APPDomain = App.Domain.Entities;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class MessageRepository : BaseEntityRepository<APPDomain.Message, DALDTO.Message, AppDbContext>, IMessageRepository
{
    public MessageRepository(AppDbContext dbContext, IMapper mapper) : 
        base(dbContext, new DalDomainMapper<APPDomain.Message, DALDTO.Message>(mapper))
    {
    }
    
    // NB !!!!!!!!!!
    public async Task<IEnumerable<DALDTO.Message>> GetAllSortedAsync(Guid userId)
    {
        var query = CreateQuery(userId);
        query = query.OrderBy(m => m.Content);
        var res = await query.ToListAsync();
        return res.Select(e => Mapper.Map(e));
    }
    
    // implement your custom methods here
    public virtual async Task<IEnumerable<DALDTO.Message>> GetAllByTopicsAsync(IEnumerable<DALDTO.Topic>? topics)
    {
        if (topics == null || !topics.Any())
            return Enumerable.Empty<DALDTO.Message>();

        var topicIds = topics.Select(t => t.Id).ToList();

        return await CreateQuery()
            .Where(m => topicIds.Contains(m.TopicId))
            .Include(m => m.AppUser)
            .Select(e => Mapper.Map(e))
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<DALDTO.Message>> GetAllBy1TopicIncludeUserAsync(Guid topicId)
    {
        return await CreateQuery()
            .Where(m => m.TopicId.Equals(topicId))
            .Include(m => m.AppUser)
            .Select(e => Mapper.Map(e))
            .ToListAsync();
    }
    
    public virtual async Task<IEnumerable<DALDTO.Message>> GetAllIncludeAll()
    {
        return await CreateQuery()
            .Include(m => m.AppUser)
            .Include(m => m.Topic)
            .Select(e => Mapper.Map(e))
            .ToListAsync();
    }
    
    public virtual async Task<DALDTO.Message?> FirstOrDefaultIncludeAllAsync(Guid id)
    {
        return Mapper.Map( await CreateQuery()
            .Include(m => m.AppUser)
            .Include(m => m.Topic)
            .FirstOrDefaultAsync(m => m.Id.Equals(id)));
    }
}