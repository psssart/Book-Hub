using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using AutoMapper;
using Base.BLL;
using Rating = App.BLL.DTO.Rating;

namespace App.BLL.Services;

public class RatingService :
    BaseEntityService<App.DAL.DTO.Rating, App.BLL.DTO.Rating, IRatingRepository>,
    IRatingService
{
    public RatingService(IAppUnitOfWork uoW, IRatingRepository repository, IMapper mapper) : base(uoW,
        repository, new BllDalMapper<App.DAL.DTO.Rating, App.BLL.DTO.Rating>(mapper))
    {
    }
    
    
    public async Task<IEnumerable<Rating>> GetAllSortedAsync(Guid userId)
    {
        return (await Repository.GetAllSortedAsync(userId)).Select(e => Mapper.Map(e));
    }

    public async Task<IEnumerable<Rating>> GetAllByBookIdAsync(Guid id)
    {
        return (await Repository.GetAllByBookIdAsync(id)).Select(e => Mapper.Map(e));
    }

    public async Task<Rating?> FirstOrDefaultIncludeAllAsync(Guid id)
    {
        return Mapper.Map(await Repository.FirstOrDefaultIncludeAllAsync(id));
    }
}