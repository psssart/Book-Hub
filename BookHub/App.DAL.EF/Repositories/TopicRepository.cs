using App.Contracts.DAL.Repositories;
using AutoMapper;
using Base.DAL.EF;
using DALDTO = App.DAL.DTO;
using APPDomain = App.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class TopicRepository : BaseEntityRepository<APPDomain.Topic, DALDTO.Topic, AppDbContext>, ITopicRepository
{
    public TopicRepository(AppDbContext dbContext, IMapper mapper) : 
        base(dbContext, new DalDomainMapper<APPDomain.Topic, DALDTO.Topic>(mapper))
    {
    }
    
    public async Task<IEnumerable<DALDTO.Topic>> GetAllSortedAsync(Guid userId)
    {
        var query = CreateQuery(userId);
        query = query.OrderBy(m => m.Content);
        var res = await query.ToListAsync();
        return res.Select(e => Mapper.Map(e));
    }
    
    // implement your custom methods here
    public virtual async Task<IEnumerable<DALDTO.Topic>> GetAllByDiscussionIdAsync(Guid id)
    {
        return await CreateQuery()
            .Where(t => t.DiscussionId.Equals(id))
            .Include(t => t.AppUser)
            .Select(e => Mapper.Map(e))
            .ToListAsync();
    }
    public virtual async Task<IEnumerable<DALDTO.Topic>> GetAllIncludeAll()
    {
        return await CreateQuery()
            .Include(t => t.AppUser)
            .Include(t => t.Discussion)
            .Select(e => Mapper.Map(e))
            .ToListAsync();
    }
    public virtual async Task<DALDTO.Topic?> FirstOrDefaultIncludeAllAsync(Guid id)
    {
        return Mapper.Map(await CreateQuery()
            .Include(t => t.AppUser)
            .Include(t => t.Discussion)
            .FirstOrDefaultAsync(t => t.Id.Equals(id)));
    }
}