using AutoMapper;
using CleanArchitecture.Application;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Mappings;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Shared.Models.Block;
using CleanArchitecture.Shared.Models.ContainerType;
using CleanArchitecture.Shared.Models.Customer;
using CleanArchitecture.Shared.Models.Depot;
using CleanArchitecture.Shared.Models.LineOperator;
using Moq;
using System.Linq.Expressions;
using ValidationException = CleanArchitecture.Application.Common.Exceptions.ValidationException;

namespace CleanArchitecture.Unittest.TestCase;

public class MasterDataServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;

    public MasterDataServiceTests()
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
    public async Task Depot_Get_WhenExists_ShouldReturnResponse()
    {
        var service = new DepotService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DepotRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Depot, bool>>>(),
                It.IsAny<Func<IQueryable<Depot>, IQueryable<Depot>>?>()))
            .ReturnsAsync(CreateDepot());

        var result = await service.Get(1);

        Assert.Equal(1, result.Id);
        Assert.Equal("DEPOT1", result.DepotCode);
        Assert.Equal("Depot 1", result.DepotName);
    }

    [Fact]
    public async Task Depot_Get_WhenNotFound_ShouldThrowUserFriendlyException()
    {
        var service = new DepotService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DepotRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Depot, bool>>>(),
                It.IsAny<Func<IQueryable<Depot>, IQueryable<Depot>>?>()))
            .ReturnsAsync((Depot?)null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() => service.Get(999));

        Assert.Equal("Depot not found", exception.Message);
    }

    [Fact]
    public async Task Depot_Create_WhenValid_ShouldCreateDepot()
    {
        var service = new DepotService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DepotRepository.AnyAsync(It.IsAny<Expression<Func<Depot, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.DepotRepository.AddAsync(It.IsAny<Depot>()))
            .Callback<Depot>(depot => depot.Id = 1)
            .Returns(Task.CompletedTask);

        var result = await service.Create(new CreateDepotRequest
        {
            DepotCode = " depot1 ",
            DepotName = " Depot 1 ",
            Address = " HCM "
        });

        Assert.Equal(1, result);

        _unitOfWorkMock.Verify(x => x.DepotRepository.AddAsync(It.Is<Depot>(
            d => d.DepotCode == "DEPOT1" && d.DepotName == "Depot 1")), Times.Once);
    }

    [Fact]
    public async Task Depot_Create_WhenDuplicateCode_ShouldThrowUserFriendlyException()
    {
        var service = new DepotService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DepotRepository.AnyAsync(It.IsAny<Expression<Func<Depot, bool>>>()))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() =>
            service.Create(new CreateDepotRequest
            {
                DepotCode = "DEPOT1",
                DepotName = "Depot 1",
                Address = "HCM"
            }));

        Assert.Equal("Depot code already exists", exception.Message);
    }

    [Fact]
    public async Task Depot_Update_WhenValid_ShouldUpdateDepot()
    {
        var service = new DepotService(_unitOfWorkMock.Object, _mapper);
        var depot = CreateDepot();

        _unitOfWorkMock
            .Setup(x => x.DepotRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Depot, bool>>>(),
                It.IsAny<Func<IQueryable<Depot>, IQueryable<Depot>>?>()))
            .ReturnsAsync(depot);

        _unitOfWorkMock
            .Setup(x => x.DepotRepository.AnyAsync(It.IsAny<Expression<Func<Depot, bool>>>()))
            .ReturnsAsync(false);

        await service.Update(1, new UpdateDepotRequest
        {
            DepotCode = " depot2 ",
            DepotName = " Depot 2 ",
            Address = " HCM 2 "
        });

        Assert.Equal("DEPOT2", depot.DepotCode);
        Assert.Equal("Depot 2", depot.DepotName);

        _unitOfWorkMock.Verify(x => x.DepotRepository.Update(It.IsAny<Depot>()), Times.Once);
    }

    [Fact]
    public async Task Customer_Get_WhenExists_ShouldReturnResponse()
    {
        var service = new CustomerService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.CustomerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>?>()))
            .ReturnsAsync(CreateCustomer());

        var result = await service.Get(1);

        Assert.Equal(1, result.Id);
        Assert.Equal("CUS1", result.CustomerCode);
        Assert.Equal("Customer 1", result.CustomerName);
    }

    [Fact]
    public async Task Customer_Get_WhenNotFound_ShouldThrowUserFriendlyException()
    {
        var service = new CustomerService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.CustomerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>?>()))
            .ReturnsAsync((Customer?)null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() => service.Get(999));

        Assert.Equal("Customer not found", exception.Message);
    }

    [Fact]
    public async Task Customer_Create_WhenValid_ShouldCreateCustomer()
    {
        var service = new CustomerService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.CustomerRepository.AnyAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.CustomerRepository.AddAsync(It.IsAny<Customer>()))
            .Callback<Customer>(customer => customer.Id = 1)
            .Returns(Task.CompletedTask);

        var result = await service.Create(new CreateCustomerRequest
        {
            CustomerCode = " cus1 ",
            CustomerName = " Customer 1 ",
            CustomerTaxCode = " TAX001 ",
            Address = " HCM "
        });

        Assert.Equal(1, result);

        _unitOfWorkMock.Verify(x => x.CustomerRepository.AddAsync(It.Is<Customer>(
            c => c.CustomerCode == "CUS1" && c.CustomerName == "Customer 1")), Times.Once);
    }

    [Fact]
    public async Task Customer_Create_WhenMissingCode_ShouldThrowUserFriendlyException()
    {
        var service = new CustomerService(_unitOfWorkMock.Object, _mapper);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() =>
            service.Create(new CreateCustomerRequest
            {
                CustomerCode = "",
                CustomerName = "Customer 1",
                Address = "HCM"
            }));

        Assert.Equal("Customer code is required", exception.Message);
    }

    [Fact]
    public async Task Customer_Update_WhenValid_ShouldUpdateCustomer()
    {
        var service = new CustomerService(_unitOfWorkMock.Object, _mapper);
        var customer = CreateCustomer();

        _unitOfWorkMock
            .Setup(x => x.CustomerRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>?>()))
            .ReturnsAsync(customer);

        _unitOfWorkMock
            .Setup(x => x.CustomerRepository.AnyAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(false);

        await service.Update(1, new UpdateCustomerRequest
        {
            CustomerCode = " cus2 ",
            CustomerName = " Customer 2 ",
            CustomerTaxCode = " TAX002 ",
            Address = " HCM 2 "
        });

        Assert.Equal("CUS2", customer.CustomerCode);
        Assert.Equal("Customer 2", customer.CustomerName);

        _unitOfWorkMock.Verify(x => x.CustomerRepository.Update(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task LineOperator_Get_WhenExists_ShouldReturnResponse()
    {
        var service = new LineOperatorService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<LineOperator, bool>>>(),
                It.IsAny<Func<IQueryable<LineOperator>, IQueryable<LineOperator>>?>()))
            .ReturnsAsync(CreateLineOperator());

        var result = await service.Get(1);

        Assert.Equal(1, result.Id);
        Assert.Equal("CMA", result.LineOperatorCode);
        Assert.Equal("CMA CGM", result.LineOperatorName);
    }

    [Fact]
    public async Task LineOperator_Get_WhenNotFound_ShouldThrowUserFriendlyException()
    {
        var service = new LineOperatorService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<LineOperator, bool>>>(),
                It.IsAny<Func<IQueryable<LineOperator>, IQueryable<LineOperator>>?>()))
            .ReturnsAsync((LineOperator?)null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() => service.Get(999));

        Assert.Equal("Line operator not found", exception.Message);
    }

    [Fact]
    public async Task LineOperator_Create_WhenValid_ShouldCreateLineOperator()
    {
        var service = new LineOperatorService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.AnyAsync(It.IsAny<Expression<Func<LineOperator, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.AddAsync(It.IsAny<LineOperator>()))
            .Callback<LineOperator>(line => line.Id = 1)
            .Returns(Task.CompletedTask);

        var result = await service.Create(new CreateLineOperatorRequest
        {
            LineOperatorCode = " cma ",
            LineOperatorName = " CMA CGM "
        });

        Assert.Equal(1, result);

        _unitOfWorkMock.Verify(x => x.LineOperatorRepository.AddAsync(It.Is<LineOperator>(
            l => l.LineOperatorCode == "CMA" && l.LineOperatorName == "CMA CGM")), Times.Once);
    }

    [Fact]
    public async Task LineOperator_Update_WhenValid_ShouldUpdateLineOperator()
    {
        var service = new LineOperatorService(_unitOfWorkMock.Object, _mapper);
        var lineOperator = CreateLineOperator();

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<LineOperator, bool>>>(),
                It.IsAny<Func<IQueryable<LineOperator>, IQueryable<LineOperator>>?>()))
            .ReturnsAsync(lineOperator);

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.AnyAsync(It.IsAny<Expression<Func<LineOperator, bool>>>()))
            .ReturnsAsync(false);

        await service.Update(1, new UpdateLineOperatorRequest
        {
            LineOperatorCode = " msc ",
            LineOperatorName = " MSC "
        });

        Assert.Equal("MSC", lineOperator.LineOperatorCode);
        Assert.Equal("MSC", lineOperator.LineOperatorName);

        _unitOfWorkMock.Verify(x => x.LineOperatorRepository.Update(It.IsAny<LineOperator>()), Times.Once);
    }

    [Fact]
    public async Task ContainerType_Get_WhenExists_ShouldReturnResponse()
    {
        var service = new ContainerTypeService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerType, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerType>, IQueryable<ContainerType>>?>()))
            .ReturnsAsync(CreateContainerType());

        var result = await service.Get(1);

        Assert.Equal(1, result.Id);
        Assert.Equal("20DC", result.ContainerTypeCode);
        Assert.Equal("22G1", result.ISOCode);
        Assert.Equal(20, result.ContainerSize);
    }

    [Fact]
    public async Task ContainerType_Get_WhenNotFound_ShouldThrowUserFriendlyException()
    {
        var service = new ContainerTypeService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerType, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerType>, IQueryable<ContainerType>>?>()))
            .ReturnsAsync((ContainerType?)null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() => service.Get(999));

        Assert.Equal("Container type not found", exception.Message);
    }

    [Fact]
    public async Task ContainerType_Create_WhenValid_ShouldCreateContainerType()
    {
        var service = new ContainerTypeService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.AnyAsync(It.IsAny<Expression<Func<ContainerType, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.AddAsync(It.IsAny<ContainerType>()))
            .Callback<ContainerType>(type => type.Id = 1)
            .Returns(Task.CompletedTask);

        var result = await service.Create(new CreateContainerTypeRequest
        {
            ContainerTypeCode = " 20dc ",
            ContainerTypeName = " 20 Dry Container ",
            ISOCode = " 22g1 ",
            ContainerSize = 20,
            MaximumWeight = 30480,
            TareWeight = 2200
        });

        Assert.Equal(1, result);

        _unitOfWorkMock.Verify(x => x.ContainerTypeRepository.AddAsync(It.Is<ContainerType>(
            t => t.ContainerTypeCode == "20DC" && t.ISOCode == "22G1")), Times.Once);
    }

    [Fact]
    public async Task ContainerType_Create_WhenInvalidSize_ShouldThrowUserFriendlyException()
    {
        var service = new ContainerTypeService(_unitOfWorkMock.Object, _mapper);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() =>
            service.Create(new CreateContainerTypeRequest
            {
                ContainerTypeCode = "20DC",
                ContainerTypeName = "20 Dry Container",
                ISOCode = "22G1",
                ContainerSize = 0,
                MaximumWeight = 30480,
                TareWeight = 2200
            }));

        Assert.Equal("Container size must be greater than 0", exception.Message);
    }

    [Fact]
    public async Task ContainerType_Update_WhenValid_ShouldUpdateContainerType()
    {
        var service = new ContainerTypeService(_unitOfWorkMock.Object, _mapper);
        var containerType = CreateContainerType();

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerType, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerType>, IQueryable<ContainerType>>?>()))
            .ReturnsAsync(containerType);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.AnyAsync(It.IsAny<Expression<Func<ContainerType, bool>>>()))
            .ReturnsAsync(false);

        await service.Update(1, new UpdateContainerTypeRequest
        {
            ContainerTypeCode = " 40hc ",
            ContainerTypeName = " 40 High Cube ",
            ISOCode = " 45g1 ",
            ContainerSize = 40,
            MaximumWeight = 32500,
            TareWeight = 3800
        });

        Assert.Equal("40HC", containerType.ContainerTypeCode);
        Assert.Equal("45G1", containerType.ISOCode);
        Assert.Equal(40, containerType.ContainerSize);

        _unitOfWorkMock.Verify(x => x.ContainerTypeRepository.Update(It.IsAny<ContainerType>()), Times.Once);
    }

    [Fact]
    public async Task Block_Get_WhenExists_ShouldReturnResponse()
    {
        var service = new BlockService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()))
            .ReturnsAsync(CreateBlock());

        var result = await service.Get(1);

        Assert.Equal(1, result.Id);
        Assert.Equal("A", result.BlockCode);
        Assert.Equal("Real", result.BlockType);
    }

    [Fact]
    public async Task Block_Get_WhenNotFound_ShouldThrowUserFriendlyException()
    {
        var service = new BlockService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()))
            .ReturnsAsync((Block?)null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(() => service.Get(999));

        Assert.Equal("Block not found", exception.Message);
    }

    [Fact]
    public async Task Block_Create_WhenValidRealBlock_ShouldCreateBlock()
    {
        var service = new BlockService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DepotRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Depot, bool>>>(),
                It.IsAny<Func<IQueryable<Depot>, IQueryable<Depot>>?>()))
            .ReturnsAsync(CreateDepot());

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.AnyAsync(It.IsAny<Expression<Func<Block, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.AddAsync(It.IsAny<Block>()))
            .Callback<Block>(block => block.Id = 1)
            .Returns(Task.CompletedTask);

        var result = await service.Create(new CreateBlockRequest
        {
            DepotId = 1,
            BlockCode = "A",
            BlockName = "Block A",
            BlockType = "Real",
            MaxBay = 10,
            MaxRow = 10,
            MaxTier = 5
        });

        Assert.Equal(1, result);

        _unitOfWorkMock.Verify(x => x.BlockRepository.AddAsync(It.IsAny<Block>()), Times.Once);
    }

    [Fact]
    public async Task Block_Create_WhenVirtualBlockHasMaxBay_ShouldThrowValidationException()
    {
        var service = new BlockService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DepotRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Depot, bool>>>(),
                It.IsAny<Func<IQueryable<Depot>, IQueryable<Depot>>?>()))
            .ReturnsAsync(CreateDepot());

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.Create(new CreateBlockRequest
            {
                DepotId = 1,
                BlockCode = "V",
                BlockName = "Virtual Block",
                BlockType = "Virtual",
                MaxBay = 1,
                MaxRow = null,
                MaxTier = null
            }));
    }

    [Fact]
    public async Task Block_Update_WhenValid_ShouldUpdateBlock()
    {
        var service = new BlockService(_unitOfWorkMock.Object, _mapper);
        var block = CreateBlock();

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Block, bool>>>(),
                It.IsAny<Func<IQueryable<Block>, IQueryable<Block>>?>()))
            .ReturnsAsync(block);

        _unitOfWorkMock
            .Setup(x => x.DepotRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Depot, bool>>>(),
                It.IsAny<Func<IQueryable<Depot>, IQueryable<Depot>>?>()))
            .ReturnsAsync(CreateDepot());

        _unitOfWorkMock
            .Setup(x => x.BlockRepository.AnyAsync(It.IsAny<Expression<Func<Block, bool>>>()))
            .ReturnsAsync(false);

        await service.Update(1, new UpdateBlockRequest
        {
            DepotId = 1,
            BlockCode = "B",
            BlockName = "Block B",
            BlockType = "Real",
            MaxBay = 20,
            MaxRow = 10,
            MaxTier = 5
        });

        Assert.Equal("B", block.BlockCode);
        Assert.Equal("Block B", block.BlockName);
        Assert.Equal(20, block.MaxBay);

        _unitOfWorkMock.Verify(x => x.BlockRepository.Update(It.IsAny<Block>()), Times.Once);
    }

    [Fact]
    public async Task Depot_Create_WhenDepotCodeIsEmpty_ShouldThrowException()
    {
        var service = new DepotService(_unitOfWorkMock.Object, _mapper);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Create(new CreateDepotRequest
            {
                DepotCode = "",
                DepotName = "Depot 1",
                Address = "HCM"
            }));
    }

    [Fact]
    public async Task Depot_Create_WhenDepotNameIsEmpty_ShouldThrowException()
    {
        var service = new DepotService(_unitOfWorkMock.Object, _mapper);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Create(new CreateDepotRequest
            {
                DepotCode = "DEPOT1",
                DepotName = "",
                Address = "HCM"
            }));
    }

    [Fact]
    public async Task Depot_Update_WhenNotFound_ShouldThrowException()
    {
        var service = new DepotService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.DepotRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Depot, bool>>>(),
                It.IsAny<Func<IQueryable<Depot>, IQueryable<Depot>>?>()))
            .ReturnsAsync((Depot?)null);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Update(999, new UpdateDepotRequest
            {
                DepotCode = "DEPOT2",
                DepotName = "Depot 2",
                Address = "HCM 2"
            }));
    }

    [Fact]
    public async Task LineOperator_Create_WhenCodeIsEmpty_ShouldThrowException()
    {
        var service = new LineOperatorService(_unitOfWorkMock.Object, _mapper);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Create(new CreateLineOperatorRequest
            {
                LineOperatorCode = "",
                LineOperatorName = "CMA CGM"
            }));
    }

    [Fact]
    public async Task LineOperator_Create_WhenNameIsEmpty_ShouldThrowException()
    {
        var service = new LineOperatorService(_unitOfWorkMock.Object, _mapper);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Create(new CreateLineOperatorRequest
            {
                LineOperatorCode = "CMA",
                LineOperatorName = ""
            }));
    }

    [Fact]
    public async Task LineOperator_Create_WhenDuplicateCode_ShouldThrowException()
    {
        var service = new LineOperatorService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.AnyAsync(It.IsAny<Expression<Func<LineOperator, bool>>>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Create(new CreateLineOperatorRequest
            {
                LineOperatorCode = "CMA",
                LineOperatorName = "CMA CGM"
            }));
    }

    [Fact]
    public async Task LineOperator_Update_WhenNotFound_ShouldThrowException()
    {
        var service = new LineOperatorService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.LineOperatorRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<LineOperator, bool>>>(),
                It.IsAny<Func<IQueryable<LineOperator>, IQueryable<LineOperator>>?>()))
            .ReturnsAsync((LineOperator?)null);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Update(999, new UpdateLineOperatorRequest
            {
                LineOperatorCode = "MSC",
                LineOperatorName = "MSC"
            }));
    }

    [Fact]
    public async Task ContainerType_Create_WhenCodeIsEmpty_ShouldThrowException()
    {
        var service = new ContainerTypeService(_unitOfWorkMock.Object, _mapper);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Create(new CreateContainerTypeRequest
            {
                ContainerTypeCode = "",
                ContainerTypeName = "20 Dry Container",
                ISOCode = "22G1",
                ContainerSize = 20,
                MaximumWeight = 30480,
                TareWeight = 2200
            }));
    }

    [Fact]
    public async Task ContainerType_Create_WhenNameIsEmpty_ShouldThrowException()
    {
        var service = new ContainerTypeService(_unitOfWorkMock.Object, _mapper);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Create(new CreateContainerTypeRequest
            {
                ContainerTypeCode = "20DC",
                ContainerTypeName = "",
                ISOCode = "22G1",
                ContainerSize = 20,
                MaximumWeight = 30480,
                TareWeight = 2200
            }));
    }

    [Fact]
    public async Task ContainerType_Create_WhenISOCodeIsEmpty_ShouldThrowException()
    {
        var service = new ContainerTypeService(_unitOfWorkMock.Object, _mapper);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Create(new CreateContainerTypeRequest
            {
                ContainerTypeCode = "20DC",
                ContainerTypeName = "20 Dry Container",
                ISOCode = "",
                ContainerSize = 20,
                MaximumWeight = 30480,
                TareWeight = 2200
            }));
    }

    [Fact]
    public async Task ContainerType_Create_WhenDuplicateCode_ShouldThrowException()
    {
        var service = new ContainerTypeService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.AnyAsync(It.IsAny<Expression<Func<ContainerType, bool>>>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Create(new CreateContainerTypeRequest
            {
                ContainerTypeCode = "20DC",
                ContainerTypeName = "20 Dry Container",
                ISOCode = "22G1",
                ContainerSize = 20,
                MaximumWeight = 30480,
                TareWeight = 2200
            }));
    }

    [Fact]
    public async Task ContainerType_Update_WhenNotFound_ShouldThrowException()
    {
        var service = new ContainerTypeService(_unitOfWorkMock.Object, _mapper);

        _unitOfWorkMock
            .Setup(x => x.ContainerTypeRepository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ContainerType, bool>>>(),
                It.IsAny<Func<IQueryable<ContainerType>, IQueryable<ContainerType>>?>()))
            .ReturnsAsync((ContainerType?)null);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.Update(999, new UpdateContainerTypeRequest
            {
                ContainerTypeCode = "40HC",
                ContainerTypeName = "40 High Cube",
                ISOCode = "45G1",
                ContainerSize = 40,
                MaximumWeight = 32500,
                TareWeight = 3800
            }));
    }

    private static Depot CreateDepot()
    {
        return new Depot
        {
            Id = 1,
            DepotCode = "DEPOT1",
            DepotName = "Depot 1",
            Address = "HCM"
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

    private static Block CreateBlock()
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
}
