using AutoMapper;
using CleanArchitecture.Shared.Models.User;
using CleanArchitecture.Shared.Models.Container;
using CleanArchitecture.Shared.Models.Depot;
using CleanArchitecture.Shared.Models.Block;
using CleanArchitecture.Shared.Models.ContainerPosition;
using CleanArchitecture.Shared.Models.ContainerTransaction;
using CleanArchitecture.Shared.Models.Customer;
using CleanArchitecture.Shared.Models.LineOperator;
using CleanArchitecture.Shared.Models.ContainerType;
using CleanArchitecture.Shared.Models.DeliveryOrder;
namespace CleanArchitecture.Application.Common.Mappings;

public class MapProfile : Profile
{
    public MapProfile()
    {
        //map container
        CreateMap<CreateContainerRequest, Container>();
        CreateMap<UpdateContainerRequest, Container>();

        CreateMap<Container, ContainerResponse>()
            .ForMember(dest => dest.ContainerTypeCode,
                opt => opt.MapFrom(src => src.ContainerTypeNavigation != null ? src.ContainerTypeNavigation.ContainerTypeCode : string.Empty))
            .ForMember(dest => dest.ContainerTypeName,
                opt => opt.MapFrom(src => src.ContainerTypeNavigation != null ? src.ContainerTypeNavigation.ContainerTypeName : string.Empty))
            .ForMember(dest => dest.ISOCode,
                opt => opt.MapFrom(src => src.ContainerTypeNavigation != null ? src.ContainerTypeNavigation.ISOCode : string.Empty))
            .ForMember(dest => dest.ContainerSize,
                opt => opt.MapFrom(src => src.ContainerTypeNavigation != null ? src.ContainerTypeNavigation.ContainerSize : 0))
            .ForMember(dest => dest.MaximumWeight,
                opt => opt.MapFrom(src => src.ContainerTypeNavigation != null ? src.ContainerTypeNavigation.MaximumWeight : null))
            .ForMember(dest => dest.TareWeight,
                opt => opt.MapFrom(src => src.ContainerTypeNavigation != null ? src.ContainerTypeNavigation.TareWeight : null))
            .ForMember(dest => dest.LineOperatorCode,
                opt => opt.MapFrom(src => src.LineOperator != null ? src.LineOperator.LineOperatorCode : string.Empty))
            .ForMember(dest => dest.LineOperatorName,
                opt => opt.MapFrom(src => src.LineOperator != null ? src.LineOperator.LineOperatorName : string.Empty));
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
        //map Customer
        CreateMap<CreateCustomerRequest, Customer>();
        CreateMap<Customer, CustomerResponse>();
        //map LineOperator
        CreateMap<CreateLineOperatorRequest, LineOperator>();
        CreateMap<LineOperator, LineOperatorResponse>();
        //map ContainerType
        CreateMap<CreateContainerTypeRequest, Domain.Entities.ContainerType>();
        CreateMap<Domain.Entities.ContainerType, ContainerTypeResponse>();
        //map DeliveryOrder
        CreateMap<CreateDeliveryOrderRequest, DeliveryOrder>();
        CreateMap<UpdateDeliveryOrderRequest, DeliveryOrder>();

        CreateMap<DeliveryOrder, DeliveryOrderResponse>()
            .ForMember(dest => dest.CustomerName,
                opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustomerName : null))
            .ForMember(dest => dest.LineOperatorName,
                opt => opt.MapFrom(src => src.LineOperator != null ? src.LineOperator.LineOperatorName : null))
            .ForMember(dest => dest.ContainerTypeName,
                opt => opt.MapFrom(src => src.ContainerType != null ? src.ContainerType.ContainerTypeName : null));
        //map user
        CreateMap<User, UserSignInRequest>().ReverseMap();
        CreateMap<User, UserSignInResponse>().ReverseMap();
        CreateMap<User, UserSignUpRequest>().ReverseMap();
        CreateMap<User, UserSignUpResponse>().ReverseMap();
        CreateMap<User, UserProfileResponse>().ReverseMap();
    }
}
