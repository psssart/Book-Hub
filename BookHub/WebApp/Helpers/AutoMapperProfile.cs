using AutoMapper;

namespace WebApp.Helpers;

public class AutoMapperProfile: Profile
{
    public AutoMapperProfile()
    {
        CreateMap<App.BLL.DTO.Message, App.DTO.v1_0.Message>().ReverseMap();
        CreateMap<App.BLL.DTO.Rating, App.DTO.v1_0.Rating>().ReverseMap();
        CreateMap<App.BLL.DTO.Topic, App.DTO.v1_0.Topic>().ReverseMap();
    }
}