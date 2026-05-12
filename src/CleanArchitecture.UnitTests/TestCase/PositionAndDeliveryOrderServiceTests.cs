using AutoMapper;
using CleanArchitecture.Application;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Mappings;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Shared.Models.ContainerPosition;
using CleanArchitecture.Shared.Models.DeliveryOrder;
using Moq;
using System.Linq.Expressions;
using ValidationException = CleanArchitecture.Application.Common.Exceptions.ValidationException;

namespace CleanArchitecture.Unittest.TestCase;

public class PositionAndDeliveryOrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;

    public PositionAndDeliveryOrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var mapperConfiguration = new MapperConfiguration(config =>
        {
            config.AddProfile<MapProfile>();
        });

        _mapper = mapperConfiguration.CreateMapper();

        _unitOfWorkMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (action, _) => await action());

        _unitOfWorkMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<Action>(), It.IsAny<CancellationToken>()))
            .Callback<Action, CancellationToken>((action, _) => action())
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task ContainerPosition_Get_WhenExists_ShouldReturnResponse()
    {
        var service = new ContainerPositionService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerPositionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerPosition, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerPosition>, IQueryable<ContainerPosition>>?>()))
            .ReturnsAsync(CreateContainerPosition());

        var result = await service.Get(1);

        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.ContainerId);
        Assert.Equal(1, result.BlockId);
        Assert.Equal(1, result.Bay);
        Assert.Equal(1, result.Row);
        Assert.Equal(1, result.Tier);
    }

    [Fact]
    public async Task ContainerPosition_Get_WhenNotFound_ShouldThrowValidationException()
    {
        var service = new ContainerPositionService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerPositionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerPosition, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerPosition>, IQueryable<ContainerPosition>>?>()))
            .ReturnsAsync((ContainerPosition?)null);

        await Assert.ThrowsAsync<ValidationException>(() => service.Get(999));
    }

    [Fact]
    public async Task ContainerPosition_Create_WhenContainerNotFound_ShouldThrowValidationException()
    {
        var service = new ContainerPositionService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()))
            .ReturnsAsync((Container?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.Create(CreateValidCreatePositionRequest()));
    }

    [Fact]
    public async Task ContainerPosition_Create_WhenBlockNotFound_ShouldThrowValidationException()
    {
        var service = new ContainerPositionService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()))
            .ReturnsAsync(CreateContainer());

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()))
            .ReturnsAsync((Block?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.Create(CreateValidCreatePositionRequest()));
    }

    [Fact]
    public async Task ContainerPosition_Create_WhenBayOutOfRange_ShouldThrowValidationException()
    {
        var service = new ContainerPositionService(_unitOfWorkMock.Object, _mapper);
        var request = CreateValidCreatePositionRequest();
        request.Bay = 99;

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()))
            .ReturnsAsync(CreateContainer());

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()))
            .ReturnsAsync(CreateNormalBlock());

        await Assert.ThrowsAsync<ValidationException>(() => service.Create(request));
    }

    [Fact]
    public async Task ContainerPosition_Create_WhenValid_ShouldCreatePosition()
    {
        var service = new ContainerPositionService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()))
            .ReturnsAsync(CreateContainer());

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()))
            .ReturnsAsync(CreateNormalBlock());

        _unitOfWorkMock
            .Setup(x => x.ContainerPositionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerPosition, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerPosition>, IQueryable<ContainerPosition>>?>()))
            .ReturnsAsync((ContainerPosition?)null);

        _unitOfWorkMock
            .Setup(x => x.ContainerPositionRepository.AddAsync(It.IsAny<ContainerPosition>()))
            .Callback<ContainerPosition>(position => position.Id = 1)
            .Returns(Task.CompletedTask);

        var result = await service.Create(CreateValidCreatePositionRequest());

        Assert.Equal(1, result);

        _unitOfWorkMock.Verify(
            x => x.ContainerPositionRepository.AddAsync(It.IsAny<ContainerPosition>()),
            Times.Once);
    }

    [Fact]
    public async Task ContainerPosition_Update_WhenPositionNotFound_ShouldThrowValidationException()
    {
        var service = new ContainerPositionService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerPositionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerPosition, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerPosition>, IQueryable<ContainerPosition>>?>()))
            .ReturnsAsync((ContainerPosition?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.Update(999, CreateValidUpdatePositionRequest()));
    }

    [Fact]
    public async Task ContainerPosition_Update_WhenContainerNotFound_ShouldThrowValidationException()
    {
        var service = new ContainerPositionService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerPositionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerPosition, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerPosition>, IQueryable<ContainerPosition>>?>()))
            .ReturnsAsync(CreateContainerPosition());

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()))
            .ReturnsAsync((Container?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.Update(1, CreateValidUpdatePositionRequest()));
    }

    [Fact]
    public async Task ContainerPosition_Update_WhenValid_ShouldUpdatePosition()
    {
        var service = new ContainerPositionService(_unitOfWorkMock.Object, _mapper);
        var position = CreateContainerPosition();

        _unitOfWorkMock
            .SetupSequence(x => x.ContainerPositionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerPosition, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerPosition>, IQueryable<ContainerPosition>>?>()))
            .ReturnsAsync(position)
            .ReturnsAsync((ContainerPosition?)null)
            .ReturnsAsync((ContainerPosition?)null);

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()))
            .ReturnsAsync(CreateContainer());

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()))
            .ReturnsAsync(CreateNormalBlock());

        await service.Update(1, new UpdateContainerPositionRequest
        {
            ContainerId = 1,
            BlockId = 1,
            Bay = 2,
            Row = 3,
            Tier = 1,
            PositionTime = new DateTime(2026, 1, 2)
        });

        Assert.Equal(2, position.Bay);
        Assert.Equal(3, position.Row);
        Assert.Equal(1, position.Tier);

        _unitOfWorkMock.Verify(
            x => x.ContainerPositionRepository.Update(It.IsAny<ContainerPosition>()),
            Times.Once);
    }

    [Fact]
    public async Task DeliveryOrder_Get_WhenExists_ShouldReturnResponse()
    {
        var service = new DeliveryOrderService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<DeliveryOrder, bool>>>(),
                It.IsAny<Func<IQueryable<DeliveryOrder>, IQueryable<DeliveryOrder>>?>()))
            .ReturnsAsync(CreateDeliveryOrder());

        var result = await service.Get(1);

        Assert.Equal(1, result.Id);
        Assert.Equal("DO001", result.DONumber);
        Assert.Equal(1, result.CustomerId);
        Assert.Equal(1, result.LineOperatorId);
        Assert.Equal(1, result.ContainerTypeId);
    }

    [Fact]
    public async Task DeliveryOrder_Get_WhenNotFound_ShouldThrowUserFriendlyException()
    {
        var service = new DeliveryOrderService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<DeliveryOrder, bool>>>(),
                It.IsAny<Func<IQueryable<DeliveryOrder>, IQueryable<DeliveryOrder>>?>()))
            .ReturnsAsync((DeliveryOrder?)null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() => service.Get(999));

        Assert.Equal("Delivery order not found", exception.Message);
    }

    [Fact]
    public async Task DeliveryOrder_Create_WhenDoNumberEmpty_ShouldThrowValidationException()
    {
        var service = new DeliveryOrderService(_unitOfWorkMock.Object, _mapper);
        var request = CreateValidCreateDeliveryOrderRequest();
        request.DONumber = "";

        await Assert.ThrowsAsync<ValidationException>(() => service.Create(request));
    }

    [Fact]
    public async Task DeliveryOrder_Create_WhenDuplicateDoNumber_ShouldThrowUserFriendlyException()
    {
        var service = new DeliveryOrderService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.AnyAsync(It.IsAny<Expression<Func<DeliveryOrder, bool>>>()))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() =>
            service.Create(CreateValidCreateDeliveryOrderRequest()));

        Assert.Equal("DO number already exists", exception.Message);
    }

    [Fact]
    public async Task DeliveryOrder_Create_WhenCustomerNotFound_ShouldThrowUserFriendlyException()
    {
        var service = new DeliveryOrderService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.AnyAsync(It.IsAny<Expression<Func<DeliveryOrder, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.CustomerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>?>()))
            .ReturnsAsync((Customer?)null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() =>
            service.Create(CreateValidCreateDeliveryOrderRequest()));

        Assert.Equal("Customer not found", exception.Message);
    }

    [Fact]
    public async Task DeliveryOrder_Create_WhenLineOperatorNotFound_ShouldThrowUserFriendlyException()
    {
        var service = new DeliveryOrderService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.AnyAsync(It.IsAny<Expression<Func<DeliveryOrder, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.CustomerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>?>()))
            .ReturnsAsync(CreateCustomer());

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<LineOperator, bool>>>(),
                It.IsAny<Func<IQueryable<LineOperator>, IQueryable<LineOperator>>?>()))
            .ReturnsAsync((LineOperator?)null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() =>
            service.Create(CreateValidCreateDeliveryOrderRequest()));

        Assert.Equal("Line operator not found", exception.Message);
    }

    [Fact]
    public async Task DeliveryOrder_Create_WhenContainerTypeNotFound_ShouldThrowUserFriendlyException()
    {
        var service = new DeliveryOrderService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.AnyAsync(It.IsAny<Expression<Func<DeliveryOrder, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.CustomerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>?>()))
            .ReturnsAsync(CreateCustomer());

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<LineOperator, bool>>>(),
                It.IsAny<Func<IQueryable<LineOperator>, IQueryable<LineOperator>>?>()))
            .ReturnsAsync(CreateLineOperator());

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerType, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerType>, IQueryable<ContainerType>>?>()))
            .ReturnsAsync((ContainerType?)null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() =>
            service.Create(CreateValidCreateDeliveryOrderRequest()));

        Assert.Equal("Container type not found", exception.Message);
    }

    [Fact]
    public async Task DeliveryOrder_Create_WhenQuantityInvalid_ShouldThrowValidationException()
    {
        var service = new DeliveryOrderService(_unitOfWorkMock.Object, _mapper);
        var request = CreateValidCreateDeliveryOrderRequest();
        request.Quantity = 0;

        SetupValidDeliveryOrderDependencies();

        await Assert.ThrowsAsync<ValidationException>(() => service.Create(request));
    }

    [Fact]
    public async Task DeliveryOrder_Create_WhenValid_ShouldCreateDeliveryOrder()
    {
        var service = new DeliveryOrderService(_unitOfWorkMock.Object, _mapper);

        SetupValidDeliveryOrderDependencies();

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.AddAsync(It.IsAny<DeliveryOrder>()))
            .Callback<DeliveryOrder>(deliveryOrder => deliveryOrder.Id = 1)
            .Returns(Task.CompletedTask);

        var result = await service.Create(CreateValidCreateDeliveryOrderRequest());

        Assert.Equal(1, result);

        _unitOfWorkMock.Verify(
            x => x.DeliveryOrderRepository.AddAsync(It.IsAny<DeliveryOrder>()),
            Times.Once);
    }

    [Fact]
    public async Task DeliveryOrder_Update_WhenNotFound_ShouldThrowUserFriendlyException()
    {
        var service = new DeliveryOrderService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<DeliveryOrder, bool>>>(),
                It.IsAny<Func<IQueryable<DeliveryOrder>, IQueryable<DeliveryOrder>>?>()))
            .ReturnsAsync((DeliveryOrder?)null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() =>
            service.Update(999, CreateValidUpdateDeliveryOrderRequest()));

        Assert.Equal("Delivery order not found", exception.Message);
    }

    [Fact]
    public async Task DeliveryOrder_Update_WhenValid_ShouldUpdateDeliveryOrder()
    {
        var service = new DeliveryOrderService(_unitOfWorkMock.Object, _mapper);
        var deliveryOrder = CreateDeliveryOrder();

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<DeliveryOrder, bool>>>(),
                It.IsAny<Func<IQueryable<DeliveryOrder>, IQueryable<DeliveryOrder>>?>()))
            .ReturnsAsync(deliveryOrder);

        SetupValidDeliveryOrderDependencies();

        await service.Update(1, CreateValidUpdateDeliveryOrderRequest());

        Assert.Equal("DO002", deliveryOrder.DONumber);
        Assert.Equal(2, deliveryOrder.Quantity);
        Assert.Equal("VESSEL002", deliveryOrder.VesselVoyage);

        _unitOfWorkMock.Verify(
            x => x.DeliveryOrderRepository.Update(It.IsAny<DeliveryOrder>()),
            Times.Once);
    }

    private void SetupValidDeliveryOrderDependencies()
    {
        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.AnyAsync(It.IsAny<Expression<Func<DeliveryOrder, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.CustomerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>?>()))
            .ReturnsAsync(CreateCustomer());

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<LineOperator, bool>>>(),
                It.IsAny<Func<IQueryable<LineOperator>, IQueryable<LineOperator>>?>()))
            .ReturnsAsync(CreateLineOperator());

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerType, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerType>, IQueryable<ContainerType>>?>()))
            .ReturnsAsync(CreateContainerType());
    }

    private static CreateContainerPositionRequest CreateValidCreatePositionRequest()
    {
        return new CreateContainerPositionRequest
        {
            ContainerId = 1,
            BlockId = 1,
            Bay = 1,
            Row = 1,
            Tier = 1,
            PositionTime = new DateTime(2026, 1, 1)
        };
    }

    private static UpdateContainerPositionRequest CreateValidUpdatePositionRequest()
    {
        return new UpdateContainerPositionRequest
        {
            ContainerId = 1,
            BlockId = 1,
            Bay = 1,
            Row = 1,
            Tier = 1,
            PositionTime = new DateTime(2026, 1, 1)
        };
    }

    private static CreateDeliveryOrderRequest CreateValidCreateDeliveryOrderRequest()
    {
        return new CreateDeliveryOrderRequest
        {
            DONumber = "DO001",
            CustomerId = 1,
            LineOperatorId = 1,
            ContainerTypeId = 1,
            Quantity = 1,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            OrderDate = DateTime.UtcNow,
            VesselVoyage = "VESSEL001",
            OrderStatus = "Active"
        };
    }

    private static UpdateDeliveryOrderRequest CreateValidUpdateDeliveryOrderRequest()
    {
        return new UpdateDeliveryOrderRequest
        {
            DONumber = "DO002",
            CustomerId = 1,
            LineOperatorId = 1,
            ContainerTypeId = 1,
            Quantity = 2,
            ExpiryDate = DateTime.UtcNow.AddDays(10),
            OrderDate = DateTime.UtcNow,
            VesselVoyage = "VESSEL002",
            OrderStatus = "Active"
        };
    }

    private static ContainerPosition CreateContainerPosition()
    {
        return new ContainerPosition
        {
            Id = 1,
            ContainerId = 1,
            BlockId = 1,
            Bay = 1,
            Row = 1,
            Tier = 1,
            PositionTime = new DateTime(2026, 1, 1)
        };
    }

    private static Container CreateContainer()
    {
        return new Container
        {
            Id = 1,
            ContainerNumber = "CMAU1234564",
            ContainerTypeId = 1,
            LineOperatorId = 1,
            ContainerOwner = "CMA CGM",
            ContainerCondition = "Normal",
            ContainerClassification = "A",
            CurrentStatus = "InYard",
            DateOfManufacture = new DateTime(2020, 1, 1)
        };
    }

    private static Block CreateNormalBlock()
    {
        return new Block
        {
            Id = 1,
            DepotId = 1,
            BlockCode = "A",
            BlockName = "Block A",
            BlockType = "Normal",
            MaxBay = 10,
            MaxRow = 10,
            MaxTier = 5
        };
    }

    private static DeliveryOrder CreateDeliveryOrder()
    {
        return new DeliveryOrder
        {
            Id = 1,
            DONumber = "DO001",
            CustomerId = 1,
            LineOperatorId = 1,
            ContainerTypeId = 1,
            Quantity = 1,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            OrderDate = DateTime.UtcNow,
            VesselVoyage = "VESSEL001",
            OrderStatus = "Active"
        };
    }

    private static Customer CreateCustomer()
    {
        return new Customer
        {
            Id = 1,
            CustomerCode = "CUS1",
            CustomerName = "Customer 1",
            CustomerTaxCode = "TAX001",
            Address = "HCM"
        };
    }

    private static LineOperator CreateLineOperator()
    {
        return new LineOperator
        {
            Id = 1,
            LineOperatorCode = "CMA",
            LineOperatorName = "CMA CGM"
        };
    }

    private static ContainerType CreateContainerType()
    {
        return new ContainerType
        {
            Id = 1,
            ContainerTypeCode = "20DC",
            ContainerTypeName = "20 Dry Container",
            ISOCode = "22G1",
            ContainerSize = 20,
            MaximumWeight = 30480,
            TareWeight = 2200
        };
    }
}
