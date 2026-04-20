using AutoMapper;
using CleanArchitecture.Shared.Models.User;
using CleanArchitecture.Shared.Models.Container;
using CleanArchitecture.Shared.Models.Depot;
using CleanArchitecture.Shared.Models.Block;
using CleanArchitecture.Shared.Models.ContainerPosition;
using CleanArchitecture.Shared.Models.ContainerTransaction;
namespace CleanArchitecture.Application.Common.Mappings;

public class MapProfile : Profile
{
    public MapProfile()
    {
        //map container
        CreateMap<Container, ContainerResponse>().ReverseMap();
        CreateMap<Container, CreateContainerRequest>().ReverseMap();
        CreateMap<Container, UpdateContainerRequest>().ReverseMap();
        //map depot
        CreateMap<Depot, DepotResponse>();
        CreateMap<CreateDepotRequest, Depot>();
        CreateMap<UpdateDepotRequest, Depot>();
        //map block
        CreateMap<Block, BlockResponse>();
        CreateMap<CreateBlockRequest, Block>();
        CreateMap<UpdateBlockRequest, Block>();
        //map containerPosition
        CreateMap<CreateContainerPositionRequest, ContainerPosition>();
        CreateMap<UpdateContainerPositionRequest, ContainerPosition>();
        CreateMap<ContainerPosition, ContainerPositionResponse>();
        //map containerTransaction
        CreateMap<CreateContainerTransactionRequest, ContainerTransaction>();
        CreateMap<UpdateContainerTransactionRequest, ContainerTransaction>();
        CreateMap<ContainerTransaction, ContainerTransactionResponse>();
        //map user
        CreateMap<User, UserSignInRequest>().ReverseMap();
        CreateMap<User, UserSignInResponse>().ReverseMap();
        CreateMap<User, UserSignUpRequest>().ReverseMap();
        CreateMap<User, UserSignUpResponse>().ReverseMap();
        CreateMap<User, UserProfileResponse>().ReverseMap();
    }
}
