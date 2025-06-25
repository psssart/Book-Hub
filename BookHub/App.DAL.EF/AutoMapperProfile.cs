using AutoMapper;

namespace App.DAL.EF;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<App.Domain.Entities.Message, App.DAL.DTO.Message>().ReverseMap();
        CreateMap<App.Domain.Entities.Topic, App.DAL.DTO.Topic>().ReverseMap();
        CreateMap<App.Domain.Entities.Rating, App.DAL.DTO.Rating>().ReverseMap();
    }
}