using App.Contracts.DAL.Repositories;
using AutoMapper;
using DALDTO = App.DAL.DTO;
using APPDomain = App.Domain.Entities;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class RatingRepository : BaseEntityRepository<APPDomain.Rating, DALDTO.Rating, AppDbContext>,  IRatingRepository
{
    public RatingRepository(AppDbContext dbContext, IMapper mapper) : 
        base(dbContext, new DalDomainMapper<APPDomain.Rating, DALDTO.Rating>(mapper))
    {
    }
    
    public async Task<IEnumerable<DALDTO.Rating>> GetAllSortedAsync(Guid userId)
    {
        var query = CreateQuery(userId);
        query = query.OrderBy(m => m.Comment);
        var res = await query.ToListAsync();
        return res.Select(e => Mapper.Map(e));
    }
    
    // implement your custom methods here
    public virtual async Task<IEnumerable<DALDTO.Rating>> GetAllByBookIdAsync(Guid id)
    {
        return await CreateQuery()
            .Where(r => r.BookId.Equals(id))
            .Include(r => r.AppUser)
            .Select(e => Mapper.Map(e))
            .ToListAsync();
    }
    public virtual async Task<DALDTO.Rating?> FirstOrDefaultIncludeAllAsync(Guid id)
    {
        return Mapper.Map(await CreateQuery()
            .Include(r => r.AppUser)
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id.Equals(id)));
    }
}