using AutoMapper;
using CleanArchitecture.Application;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Mappings;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Shared.Models.Container;
using Moq;
using System.Linq.Expressions;

namespace CleanArchitecture.Unittest.TestCase;

public class ContainerServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly ContainerService _containerService;

    public ContainerServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var mapperConfiguration = new MapperConfiguration(config =>
        {
            config.AddProfile<MapProfile>();
        });

        _mapper = mapperConfiguration.CreateMapper();

        _containerService = new ContainerService(
            _unitOfWorkMock.Object,
            _mapper
        );
    }

    [Fact]
    public async Task Get_WhenContainerExists_ShouldReturnContainerResponse()
    {
        // Arrange
        var containerId = 1;
        var container = CreateContainer(containerId, "CMAU1234564");

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(container);

        // Act
        var result = await _containerService.Get(containerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(containerId, result.Id);
        Assert.Equal("CMAU1234564", result.ContainerNumber);
        Assert.Equal("A", result.ContainerClassification);
        Assert.Equal("Normal", result.ContainerCondition);
        Assert.Equal("InYard", result.CurrentStatus);

        Assert.Equal(1, result.ContainerTypeId);
        Assert.Equal("20DC", result.ContainerTypeCode);
        Assert.Equal("20 Dry Container", result.ContainerTypeName);
        Assert.Equal("22G1", result.ISOCode);
        Assert.Equal(20, result.ContainerSize);

        Assert.Equal(1, result.LineOperatorId);
        Assert.Equal("CMA", result.LineOperatorCode);
        Assert.Equal("CMA CGM", result.LineOperatorName);
    }

    [Fact]
    public async Task Get_WhenContainerDoesNotExist_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var containerId = 999;

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync((Container?)null);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Get(containerId)
        );

        // Assert
        Assert.Equal("Container not found", exception.Message);
    }

    [Fact]
    public async Task Add_WhenContainerNumberIsEmpty_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = new CreateContainerRequest
        {
            ContainerNumber = "",
            ContainerTypeId = 1,
            LineOperatorId = 1,
            ContainerOwner = "CMA CGM",
            ContainerCondition = "Normal",
            ContainerClassification = "A",
            CurrentStatus = "OutYard"
        };

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Add(request, CancellationToken.None)
        );

        // Assert
        Assert.Equal("Container number is required", exception.Message);
    }

    [Fact]
    public async Task Add_WhenContainerNumberFormatIsInvalid_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = new CreateContainerRequest
        {
            ContainerNumber = "ABC123",
            ContainerTypeId = 1,
            LineOperatorId = 1,
            ContainerOwner = "CMA CGM",
            ContainerCondition = "Normal",
            ContainerClassification = "A",
            CurrentStatus = "OutYard"
        };

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Add(request, CancellationToken.None)
        );

        // Assert
        Assert.Contains("Container number is invalid", exception.Message);
    }

    [Fact]
    public async Task Add_WhenContainerNumberAlreadyExists_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidCreateRequest("CMAU1234564");

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.AnyAsync(It.IsAny<Expression<Func<Container, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Add(request, CancellationToken.None)
        );

        // Assert
        Assert.Equal("Container number already exists", exception.Message);
    }

    [Fact]
    public async Task Add_WhenContainerTypeDoesNotExist_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidCreateRequest("CMAU1234564");

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.AnyAsync(It.IsAny<Expression<Func<Container, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.AnyAsync(It.IsAny<Expression<Func<ContainerType, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Add(request, CancellationToken.None)
        );

        // Assert
        Assert.Equal("Container type not found", exception.Message);
    }

    [Fact]
    public async Task Add_WhenLineOperatorDoesNotExist_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidCreateRequest("CMAU1234564");

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.AnyAsync(It.IsAny<Expression<Func<Container, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.AnyAsync(It.IsAny<Expression<Func<ContainerType, bool>>>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.AnyAsync(It.IsAny<Expression<Func<LineOperator, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Add(request, CancellationToken.None)
        );

        // Assert
        Assert.Equal("Line operator not found", exception.Message);
    }

    [Fact]
    public async Task Add_WhenRequestIsValid_ShouldCreateContainer()
    {
        // Arrange
        var request = CreateValidCreateRequest("CMAU1234564");
        var createdContainer = CreateContainer(1, "CMAU1234564");

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.AnyAsync(It.IsAny<Expression<Func<Container, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.AnyAsync(It.IsAny<Expression<Func<ContainerType, bool>>>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.AnyAsync(It.IsAny<Expression<Func<LineOperator, bool>>>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (action, _) => await action());

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.AddAsync(It.IsAny<Container>()))
            .Callback<Container>(container =>
            {
                container.Id = 1;
                container.ContainerTypeNavigation = createdContainer.ContainerTypeNavigation;
                container.LineOperator = createdContainer.LineOperator;
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(createdContainer);

        // Act
        var result = await _containerService.Add(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("CMAU1234564", result.ContainerNumber);
        Assert.Equal(1, result.ContainerTypeId);
        Assert.Equal(1, result.LineOperatorId);

        _unitOfWorkMock.Verify(
            x => x.ContainerRepository.AddAsync(It.IsAny<Container>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenContainerNumberIsEmpty_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1, "");

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Update(request, CancellationToken.None)
        );

        // Assert
        Assert.Equal("Container number is required", exception.Message);
    }

    [Fact]
    public async Task Update_WhenContainerNumberFormatIsInvalid_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1, "ABC123");

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Update(request, CancellationToken.None)
        );

        // Assert
        Assert.Contains("Container number is invalid", exception.Message);
    }

    [Fact]
    public async Task Update_WhenContainerDoesNotExist_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidUpdateRequest(999, "CMAU1234564");

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync((Container?)null);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Update(request, CancellationToken.None)
        );

        // Assert
        Assert.Equal("Container not found", exception.Message);
    }

    [Fact]
    public async Task Update_WhenContainerNumberAlreadyExists_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1, "CMAU1234564");
        var existingContainer = CreateContainer(1, "MSCU1234566");

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(existingContainer);

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.AnyAsync(It.IsAny<Expression<Func<Container, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Update(request, CancellationToken.None)
        );

        // Assert
        Assert.Equal("Container number already exists", exception.Message);
    }

    [Fact]
    public async Task Update_WhenContainerTypeDoesNotExist_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1, "CMAU1234564");
        var existingContainer = CreateContainer(1, "CMAU1234564");

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(existingContainer);

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.AnyAsync(It.IsAny<Expression<Func<Container, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.AnyAsync(It.IsAny<Expression<Func<ContainerType, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Update(request, CancellationToken.None)
        );

        // Assert
        Assert.Equal("Container type not found", exception.Message);
    }

    [Fact]
    public async Task Update_WhenLineOperatorDoesNotExist_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1, "CMAU1234564");
        var existingContainer = CreateContainer(1, "CMAU1234564");

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(existingContainer);

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.AnyAsync(It.IsAny<Expression<Func<Container, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.AnyAsync(It.IsAny<Expression<Func<ContainerType, bool>>>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.AnyAsync(It.IsAny<Expression<Func<LineOperator, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _containerService.Update(request, CancellationToken.None)
        );

        // Assert
        Assert.Equal("Line operator not found", exception.Message);
    }

    [Fact]
    public async Task Update_WhenRequestIsValid_ShouldUpdateContainer()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1, "CMAU1234564");
        var existingContainer = CreateContainer(1, "MSCU1234566");
        var updatedContainer = CreateContainer(1, "CMAU1234564");

        _unitOfWorkMock
            .SetupSequence(x => x.ContainerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Container, bool>>>(),
                It.IsAny<Func<IQueryable<Container>, IQueryable<Container>>?>()
            ))
            .ReturnsAsync(existingContainer)
            .ReturnsAsync(updatedContainer);

        _unitOfWorkMock
            .Setup(x => x.ContainerRepository.AnyAsync(It.IsAny<Expression<Func<Container, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.AnyAsync(It.IsAny<Expression<Func<ContainerType, bool>>>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.AnyAsync(It.IsAny<Expression<Func<LineOperator, bool>>>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<Action>(), It.IsAny<CancellationToken>()))
            .Callback<Action, CancellationToken>((action, _) => action())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _containerService.Update(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("CMAU1234564", result.ContainerNumber);
        Assert.Equal("A", result.ContainerClassification);
        Assert.Equal("Normal", result.ContainerCondition);
        Assert.Equal("InYard", result.CurrentStatus);

        _unitOfWorkMock.Verify(
            x => x.ContainerRepository.Update(It.IsAny<Container>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.ExecuteTransactionAsync(It.IsAny<Action>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CreateContainerRequest CreateValidCreateRequest(string containerNumber)
    {
        return new CreateContainerRequest
        {
            ContainerNumber = containerNumber,
            ContainerTypeId = 1,
            LineOperatorId = 1,
            DateOfManufacture = new DateTime(2020, 1, 1),
            ContainerOwner = "CMA CGM",
            ContainerCondition = "Normal",
            ContainerClassification = "A",
            CurrentStatus = "OutYard"
        };
    }

    private static UpdateContainerRequest CreateValidUpdateRequest(int id, string containerNumber)
    {
        return new UpdateContainerRequest
        {
            Id = id,
            ContainerNumber = containerNumber,
            ContainerTypeId = 1,
            LineOperatorId = 1,
            DateOfManufacture = new DateTime(2020, 1, 1),
            ContainerOwner = "CMA CGM",
            ContainerCondition = "Normal",
            ContainerClassification = "A",
            CurrentStatus = "InYard"
        };
    }

    private static Container CreateContainer(int id, string containerNumber)
    {
        return new Container
        {
            Id = id,
            ContainerNumber = containerNumber,
            ContainerTypeId = 1,
            LineOperatorId = 1,
            ContainerOwner = "CMA CGM",
            ContainerCondition = "Normal",
            ContainerClassification = "A",
            CurrentStatus = "InYard",
            DateOfManufacture = new DateTime(2020, 1, 1),
            ContainerTypeNavigation = new ContainerType
            {
                Id = 1,
                ContainerTypeCode = "20DC",
                ContainerTypeName = "20 Dry Container",
                ISOCode = "22G1",
                ContainerSize = 20,
                MaximumWeight = 30480,
                TareWeight = 2200
            },
            LineOperator = new LineOperator
            {
                Id = 1,
                LineOperatorCode = "CMA",
                LineOperatorName = "CMA CGM"
            }
        };
    }
}
