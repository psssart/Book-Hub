using AutoMapper;

namespace App.BLL;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<App.DAL.DTO.Message, App.BLL.DTO.Message>().ReverseMap();
        CreateMap<App.DAL.DTO.Rating, App.BLL.DTO.Rating>().ReverseMap();
        CreateMap<App.DAL.DTO.Topic, App.BLL.DTO.Topic>().ReverseMap();
    }
}