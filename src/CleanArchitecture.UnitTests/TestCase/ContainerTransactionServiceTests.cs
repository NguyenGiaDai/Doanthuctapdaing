using AutoMapper;
using CleanArchitecture.Application;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Mappings;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Shared.Models.ContainerTransaction;
using Moq;
using System.Linq.Expressions;
using ValidationException = CleanArchitecture.Application.Common.Exceptions.ValidationException;

namespace CleanArchitecture.Unittest.TestCase;

public class ContainerTransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly ContainerTransactionService _service;

    public ContainerTransactionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var mapperConfiguration = new MapperConfiguration(config =>
        {
            config.AddProfile<MapProfile>();
        });

        _mapper = mapperConfiguration.CreateMapper();

        _service = new ContainerTransactionService(
            _unitOfWorkMock.Object,
            _mapper,
            null!
        );
    }

    [Fact]
    public async Task Get_WhenTransactionExists_ShouldReturnResponse()
    {
        // Arrange
        var transaction = CreateTransaction(1);

        _unitOfWorkMock
            .Setup(x => x.ContainerTransactionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerTransaction, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerTransaction>, IQueryable<ContainerTransaction>>?>()
            ))
            .ReturnsAsync(transaction);

        // Act
        var result = await _service.Get(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.ContainerId);
        Assert.Equal("In", result.TransactionType);
        Assert.Equal(1, result.ToBlockId);
        Assert.Equal(1, result.ToBay);
        Assert.Equal("51C-12345", result.VehicleNumber);
    }

    [Fact]
    public async Task Get_WhenTransactionDoesNotExist_ShouldThrowUserFriendlyException()
    {
        // Arrange
        _unitOfWorkMock
            .Setup(x => x.ContainerTransactionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerTransaction, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerTransaction>, IQueryable<ContainerTransaction>>?>()
            ))
            .ReturnsAsync((ContainerTransaction?)null);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _service.Get(999)
        );

        // Assert
        Assert.Equal("Container transaction not found", exception.Message);
    }

    [Fact]
    public async Task Create_WhenContainerDoesNotExist_ShouldThrowValidationException()
    {
        // Arrange
        var request = CreateValidCreateRequest();

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync((Container?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.Create(request)
        );
    }

    [Fact]
    public async Task Create_WhenTransactionTypeIsEmpty_ShouldThrowValidationException()
    {
        // Arrange
        var request = CreateValidCreateRequest();
        request.TransactionType = "";

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(CreateContainer());

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.Create(request)
        );
    }

    [Fact]
    public async Task Create_WhenInTransactionWithoutToBlock_ShouldThrowValidationException()
    {
        // Arrange
        var request = CreateValidCreateRequest();
        request.ToBlockId = null;

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(CreateContainer());

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.Create(request)
        );
    }

    [Fact]
    public async Task Create_WhenToBlockDoesNotExist_ShouldThrowValidationException()
    {
        // Arrange
        var request = CreateValidCreateRequest();

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(CreateContainer());

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()
            ))
            .ReturnsAsync((Block?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.Create(request)
        );
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ShouldCreateTransaction()
    {
        // Arrange
        var request = CreateValidCreateRequest();

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(CreateContainer());

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()
            ))
            .ReturnsAsync(CreateRealBlock());

        _unitOfWorkMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (action, _) => await action());

        _unitOfWorkMock
            .Setup(x => x.ContainerTransactionRepository.AddAsync(It.IsAny<ContainerTransaction>()))
            .Callback<ContainerTransaction>(transaction => transaction.Id = 1)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Create(request);

        // Assert
        Assert.Equal(1, result);

        _unitOfWorkMock.Verify(
            x => x.ContainerTransactionRepository.AddAsync(It.IsAny<ContainerTransaction>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ImportContainer_WhenContainerDoesNotExist_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidImportRequest();

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync((Container?)null);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _service.ImportContainer(request)
        );

        // Assert
        Assert.Equal("Container not found", exception.Message);
    }

    [Fact]
    public async Task ImportContainer_WhenContainerAlreadyInYard_ShouldThrowValidationException()
    {
        // Arrange
        var request = CreateValidImportRequest();
        var container = CreateContainer();
        container.CurrentStatus = "InYard";

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(container);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.ImportContainer(request)
        );
    }

    [Fact]
    public async Task ImportContainer_WhenBlockDoesNotExist_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidImportRequest();

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(CreateContainer());

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()
            ))
            .ReturnsAsync((Block?)null);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _service.ImportContainer(request)
        );

        // Assert
        Assert.Equal("Block not found", exception.Message);
    }

    [Fact]
    public async Task ImportContainer_When20FtContainerPlacedInEvenBay_ShouldThrowValidationException()
    {
        // Arrange
        var request = CreateValidImportRequest();
        request.ToBay = 2;

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(CreateContainer(containerSize: 20));

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()
            ))
            .ReturnsAsync(CreateRealBlock());

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.ImportContainer(request)
        );
    }

    [Fact]
    public async Task ImportContainer_WhenPositionOccupied_ShouldThrowValidationException()
    {
        // Arrange
        var request = CreateValidImportRequest();

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(CreateContainer(containerSize: 20));

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()
            ))
            .ReturnsAsync(CreateRealBlock());

        _unitOfWorkMock
            .Setup(x => x.ContainerPositionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerPosition, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerPosition>, IQueryable<ContainerPosition>>?>()
            ))
            .ReturnsAsync(CreateContainerPosition(containerId: 2));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.ImportContainer(request)
        );
    }

    [Fact]
    public async Task ImportContainer_WhenRequestIsValid_ShouldCreateTransactionAndPosition()
    {
        // Arrange
        var request = CreateValidImportRequest();
        var container = CreateContainer(containerSize: 20);
        container.CurrentStatus = "OutYard";

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(container);

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()
            ))
            .ReturnsAsync(CreateRealBlock());

        _unitOfWorkMock
            .SetupSequence(x => x.ContainerPositionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerPosition, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerPosition>, IQueryable<ContainerPosition>>?>()
            ))
            .ReturnsAsync((ContainerPosition?)null)
            .ReturnsAsync((ContainerPosition?)null);

        _unitOfWorkMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (action, _) => await action());

        _unitOfWorkMock
            .Setup(x => x.ContainerTransactionRepository.AddAsync(It.IsAny<ContainerTransaction>()))
            .Callback<ContainerTransaction>(transaction => transaction.Id = 1)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.ContainerPositionRepository.AddAsync(It.IsAny<ContainerPosition>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ImportContainer(request);

        // Assert
        Assert.Equal(1, result);
        Assert.Equal("InYard", container.CurrentStatus);

        _unitOfWorkMock.Verify(
            x => x.ContainerTransactionRepository.AddAsync(It.IsAny<ContainerTransaction>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.ContainerPositionRepository.AddAsync(It.IsAny<ContainerPosition>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.ContainerRepository.Update(It.IsAny<Container>()),
            Times.Once);
    }

    [Fact]
    public async Task ExportContainer_WhenContainerDoesNotExist_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidExportRequest();

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync((Container?)null);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _service.ExportContainer(request)
        );

        // Assert
        Assert.Equal("Container not found", exception.Message);
    }

    [Fact]
    public async Task ExportContainer_WhenContainerIsNotInYard_ShouldThrowValidationException()
    {
        // Arrange
        var request = CreateValidExportRequest();
        var container = CreateContainer();
        container.CurrentStatus = "OutYard";

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(container);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.ExportContainer(request)
        );
    }

    [Fact]
    public async Task ExportContainer_WhenDeliveryOrderDoesNotExist_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidExportRequest();
        var container = CreateContainer();
        container.CurrentStatus = "InYard";

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(container);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<DeliveryOrder, bool>>>(),
                It.IsAny<Func<IQueryable<DeliveryOrder>, IQueryable<DeliveryOrder>>?>()
            ))
            .ReturnsAsync((DeliveryOrder?)null);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _service.ExportContainer(request)
        );

        // Assert
        Assert.Equal("Delivery order not found", exception.Message);
    }

    [Fact]
    public async Task ExportContainer_WhenDeliveryOrderExpired_ShouldThrowValidationException()
    {
        // Arrange
        var request = CreateValidExportRequest();
        var container = CreateContainer();
        container.CurrentStatus = "InYard";

        var deliveryOrder = CreateDeliveryOrder();
        deliveryOrder.ExpiryDate = DateTime.UtcNow.AddDays(-1);

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(container);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<DeliveryOrder, bool>>>(),
                It.IsAny<Func<IQueryable<DeliveryOrder>, IQueryable<DeliveryOrder>>?>()
            ))
            .ReturnsAsync(deliveryOrder);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.ExportContainer(request)
        );
    }

    [Fact]
    public async Task ExportContainer_WhenLineOperatorDoesNotMatch_ShouldThrowValidationException()
    {
        // Arrange
        var request = CreateValidExportRequest();
        var container = CreateContainer();
        container.CurrentStatus = "InYard";
        container.LineOperatorId = 1;

        var deliveryOrder = CreateDeliveryOrder();
        deliveryOrder.LineOperatorId = 2;

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(container);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<DeliveryOrder, bool>>>(),
                It.IsAny<Func<IQueryable<DeliveryOrder>, IQueryable<DeliveryOrder>>?>()
            ))
            .ReturnsAsync(deliveryOrder);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.ExportContainer(request)
        );
    }

    [Fact]
    public async Task ExportContainer_WhenCurrentPositionDoesNotExist_ShouldThrowValidationException()
    {
        // Arrange
        var request = CreateValidExportRequest();
        var container = CreateContainer();
        container.CurrentStatus = "InYard";

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(container);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<DeliveryOrder, bool>>>(),
                It.IsAny<Func<IQueryable<DeliveryOrder>, IQueryable<DeliveryOrder>>?>()
            ))
            .ReturnsAsync(CreateDeliveryOrder());

        _unitOfWorkMock
            .Setup(x => x.ContainerPositionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerPosition, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerPosition>, IQueryable<ContainerPosition>>?>()
            ))
            .ReturnsAsync((ContainerPosition?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.ExportContainer(request)
        );
    }

    [Fact]
    public async Task ExportContainer_WhenRequestIsValid_ShouldCreateOutTransactionAndDeletePosition()
    {
        // Arrange
        var request = CreateValidExportRequest();
        var container = CreateContainer();
        container.CurrentStatus = "InYard";

        var currentPosition = CreateContainerPosition(container.Id);

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(container);

        _unitOfWorkMock
            .Setup(x => x.DeliveryOrderRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<DeliveryOrder, bool>>>(),
                It.IsAny<Func<IQueryable<DeliveryOrder>, IQueryable<DeliveryOrder>>?>()
            ))
            .ReturnsAsync(CreateDeliveryOrder());

        _unitOfWorkMock
            .Setup(x => x.ContainerPositionRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerPosition, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerPosition>, IQueryable<ContainerPosition>>?>()
            ))
            .ReturnsAsync(currentPosition);

        _unitOfWorkMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (action, _) => await action());

        _unitOfWorkMock
            .Setup(x => x.ContainerTransactionRepository.AddAsync(It.IsAny<ContainerTransaction>()))
            .Callback<ContainerTransaction>(transaction => transaction.Id = 1)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExportContainer(request);

        // Assert
        Assert.Equal(1, result);
        Assert.Equal("OutYard", container.CurrentStatus);

        _unitOfWorkMock.Verify(
            x => x.ContainerTransactionRepository.AddAsync(It.IsAny<ContainerTransaction>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.ContainerPositionRepository.Delete(It.IsAny<ContainerPosition>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.ContainerRepository.Update(It.IsAny<Container>()),
            Times.Once);
    }

    private static CreateContainerTransactionRequest CreateValidCreateRequest()
    {
        return new CreateContainerTransactionRequest
        {
            ContainerId = 1,
            TransactionType = "In",
            FromBlockId = null,
            FromBay = null,
            FromRow = null,
            FromTier = null,
            ToBlockId = 1,
            ToBay = 1,
            ToRow = 1,
            ToTier = 1,
            VehicleNumber = "51C-12345",
            TransactionTime = new DateTime(2026, 1, 1),
            Note = "Import container"
        };
    }

    private static ImportContainerRequest CreateValidImportRequest()
    {
        return new ImportContainerRequest
        {
            ContainerId = 1,
            ToBlockId = 1,
            ToBay = 1,
            ToRow = 1,
            ToTier = 1,
            VehicleNumber = "51C-12345",
            TransactionTime = new DateTime(2026, 1, 1),
            Note = "Import container"
        };
    }

    private static ExportContainerRequest CreateValidExportRequest()
    {
        return new ExportContainerRequest
        {
            ContainerId = 1,
            DeliveryOrderId = 1,
            VehicleNumber = "51C-99999",
            TransactionTime = new DateTime(2026, 1, 2),
            Note = "Export container"
        };
    }

    private static ContainerTransaction CreateTransaction(int id)
    {
        return new ContainerTransaction
        {
            Id = id,
            ContainerId = 1,
            TransactionType = "In",
            FromBlockId = null,
            FromBay = null,
            FromRow = null,
            FromTier = null,
            ToBlockId = 1,
            ToBay = 1,
            ToRow = 1,
            ToTier = 1,
            VehicleNumber = "51C-12345",
            TransactionTime = new DateTime(2026, 1, 1),
            Note = "Import container"
        };
    }

    private static Container CreateContainer(int containerSize = 20)
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
            CurrentStatus = "OutYard",
            DateOfManufacture = new DateTime(2020, 1, 1),
            ContainerTypeNavigation = new ContainerType
            {
                Id = 1,
                ContainerTypeCode = containerSize == 20 ? "20DC" : "40HC",
                ContainerTypeName = containerSize == 20 ? "20 Dry Container" : "40 High Cube",
                ISOCode = containerSize == 20 ? "22G1" : "45G1",
                ContainerSize = containerSize,
                MaximumWeight = 30480,
                TareWeight = containerSize == 20 ? 2200 : 3800
            },
            LineOperator = new LineOperator
            {
                Id = 1,
                LineOperatorCode = "CMA",
                LineOperatorName = "CMA CGM"
            }
        };
    }

    private static Block CreateRealBlock()
    {
        return new Block
        {
            Id = 1,
            DepotId = 1,
            BlockCode = "A",
            BlockName = "Block A",
            BlockType = "Real",
            MaxBay = 10,
            MaxRow = 10,
            MaxTier = 5
        };
    }

    private static ContainerPosition CreateContainerPosition(int containerId)
    {
        return new ContainerPosition
        {
            Id = 1,
            ContainerId = containerId,
            BlockId = 1,
            Bay = 1,
            Row = 1,
            Tier = 1,
            PositionTime = new DateTime(2026, 1, 1)
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
            VesselVoyage = "VESSEL001",
            OrderDate = DateTime.UtcNow,
            OrderStatus = "Active"
        };
    }
}
