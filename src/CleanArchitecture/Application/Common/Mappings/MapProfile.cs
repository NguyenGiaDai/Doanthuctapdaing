using AutoMapper;
using CleanArchitecture.Shared.Models.User;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Shared.Models.Container;

namespace CleanArchitecture.Application.Common.Mappings;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<Container, ContainerResponse>().ReverseMap();
        CreateMap<Container, CreateContainerRequest>().ReverseMap();
        CreateMap<Container, UpdateContainerRequest>().ReverseMap();

        CreateMap<User, UserSignInRequest>().ReverseMap();
        CreateMap<User, UserSignInResponse>().ReverseMap();
        CreateMap<User, UserSignUpRequest>().ReverseMap();
        CreateMap<User, UserSignUpResponse>().ReverseMap();
        CreateMap<User, UserProfileResponse>().ReverseMap();
    }
}
