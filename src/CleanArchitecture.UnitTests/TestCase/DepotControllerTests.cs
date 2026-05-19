using CleanArchitecture.Application;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Block;
using CleanArchitecture.Shared.Models.Container;
using CleanArchitecture.Shared.Models.ContainerPosition;
using CleanArchitecture.Shared.Models.ContainerTransaction;
using CleanArchitecture.Shared.Models.ContainerType;
using CleanArchitecture.Shared.Models.Customer;
using CleanArchitecture.Shared.Models.DeliveryOrder;
using CleanArchitecture.Shared.Models.Depot;
using CleanArchitecture.Shared.Models.LineOperator;
using CleanArchitecture.Web.Controller;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CleanArchitecture.Unittest.TestCase;

public class DepotControllerTests
{
    [Fact]
    public async Task ContainerController_GetById_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerService>();
        var controller = new ContainerController(serviceMock.Object);

        serviceMock
            .Setup(x => x.Get(1))
            .ReturnsAsync(new ContainerResponse { Id = 1, ContainerNumber = "CMAU1234564" });

        var result = await controller.Get(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<ContainerResponse>(okResult.Value);
        Assert.Equal(1, value.Id);

        serviceMock.Verify(x => x.Get(1), Times.Once);
    }

    [Fact]
    public async Task ContainerController_GetList_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerService>();
        var controller = new ContainerController(serviceMock.Object);

        var pagination = new Pagination<ContainerResponse>(
            new List<ContainerResponse>
            {
                new ContainerResponse { Id = 1, ContainerNumber = "CMAU1234564" }
            },
            1,
            1,
            10
        );

        serviceMock
            .Setup(x => x.Get(1, 10))
            .ReturnsAsync(pagination);

        var result = await controller.Get(1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Pagination<ContainerResponse>>(okResult.Value);

        serviceMock.Verify(x => x.Get(1, 10), Times.Once);
    }

    [Fact]
    public async Task ContainerController_Add_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerService>();
        var controller = new ContainerController(serviceMock.Object);
        var request = new CreateContainerRequest { ContainerNumber = "CMAU1234564" };

        serviceMock
            .Setup(x => x.Add(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContainerResponse { Id = 1, ContainerNumber = "CMAU1234564" });

        var result = await controller.Add(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<ContainerResponse>(okResult.Value);
        Assert.Equal(1, value.Id);

        serviceMock.Verify(x => x.Add(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ContainerController_Update_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerService>();
        var controller = new ContainerController(serviceMock.Object);
        var request = new UpdateContainerRequest { Id = 1, ContainerNumber = "CMAU1234564" };

        serviceMock
            .Setup(x => x.Update(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContainerResponse { Id = 1, ContainerNumber = "CMAU1234564" });

        var result = await controller.Update(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<ContainerResponse>(okResult.Value);
        Assert.Equal(1, value.Id);

        serviceMock.Verify(x => x.Update(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ContainerTransactionController_GetAll_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTransactionService>();
        var controller = new ContainerTransactionController(serviceMock.Object);

        var pagination = new Pagination<ContainerTransactionResponse>(
            new List<ContainerTransactionResponse>
            {
                new ContainerTransactionResponse { Id = 1, ContainerId = 1 }
            },
            1,
            1,
            10
        );

        serviceMock
            .Setup(x => x.GetAll(1, 10))
            .ReturnsAsync(pagination);

        var result = await controller.GetAll(1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Pagination<ContainerTransactionResponse>>(okResult.Value);

        serviceMock.Verify(x => x.GetAll(1, 10), Times.Once);
    }

    [Fact]
    public async Task ContainerTransactionController_GetById_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTransactionService>();
        var controller = new ContainerTransactionController(serviceMock.Object);

        serviceMock
            .Setup(x => x.Get(1))
            .ReturnsAsync(new ContainerTransactionResponse { Id = 1, ContainerId = 1 });

        var result = await controller.Get(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<ContainerTransactionResponse>(okResult.Value);
        Assert.Equal(1, value.Id);

        serviceMock.Verify(x => x.Get(1), Times.Once);
    }

    [Fact]
    public async Task ContainerTransactionController_Create_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTransactionService>();
        var controller = new ContainerTransactionController(serviceMock.Object);
        var request = new CreateContainerTransactionRequest { ContainerId = 1 };

        serviceMock
            .Setup(x => x.Create(request))
            .ReturnsAsync(1);

        var result = await controller.Create(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, okResult.Value);

        serviceMock.Verify(x => x.Create(request), Times.Once);
    }

    [Fact]
    public async Task ContainerTransactionController_ImportContainer_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTransactionService>();
        var controller = new ContainerTransactionController(serviceMock.Object);
        var request = new ImportContainerRequest { ContainerId = 1 };

        serviceMock
            .Setup(x => x.ImportContainer(request))
            .ReturnsAsync(1);

        var result = await controller.ImportContainer(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, okResult.Value);

        serviceMock.Verify(x => x.ImportContainer(request), Times.Once);
    }

    [Fact]
    public async Task ContainerTransactionController_ExportContainer_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTransactionService>();
        var controller = new ContainerTransactionController(serviceMock.Object);
        var request = new ExportContainerRequest { ContainerId = 1, DeliveryOrderId = 1 };

        serviceMock
            .Setup(x => x.ExportContainer(request))
            .ReturnsAsync(1);

        var result = await controller.ExportContainer(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, okResult.Value);

        serviceMock.Verify(x => x.ExportContainer(request), Times.Once);
    }

    [Fact]
    public async Task ContainerTransactionController_GetThroughputReport_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTransactionService>();
        var controller = new ContainerTransactionController(serviceMock.Object);
        var request = new ContainerThroughputReportRequest();

        var response = new List<ContainerThroughputReportResponse>
        {
            new ContainerThroughputReportResponse()
        };

        serviceMock
            .Setup(x => x.GetContainerThroughputReport(request))
            .ReturnsAsync(response);

        var result = await controller.GetContainerThroughputReport(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<List<ContainerThroughputReportResponse>>(okResult.Value);

        serviceMock.Verify(x => x.GetContainerThroughputReport(request), Times.Once);
    }

    [Fact]
    public async Task ContainerTransactionController_GetYardInventoryReport_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTransactionService>();
        var controller = new ContainerTransactionController(serviceMock.Object);
        var request = new ContainerYardInventoryReportRequest();

        var response = new List<ContainerYardInventoryReportResponse>
        {
            new ContainerYardInventoryReportResponse()
        };

        serviceMock
            .Setup(x => x.GetContainerYardInventoryReport(request))
            .ReturnsAsync(response);

        var result = await controller.GetContainerYardInventoryReport(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<List<ContainerYardInventoryReportResponse>>(okResult.Value);

        serviceMock.Verify(x => x.GetContainerYardInventoryReport(request), Times.Once);
    }

    [Fact]
    public async Task ContainerTransactionController_Update_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTransactionService>();
        var controller = new ContainerTransactionController(serviceMock.Object);
        var request = new UpdateContainerTransactionRequest { ContainerId = 1 };

        serviceMock
            .Setup(x => x.Update(1, request))
            .Returns(Task.CompletedTask);

        var result = await controller.Update(1, request);

        Assert.IsType<OkResult>(result);

        serviceMock.Verify(x => x.Update(1, request), Times.Once);
    }

    [Fact]
    public async Task DepotController_GetAll_ShouldReturnOk()
    {
        var serviceMock = new Mock<IDepotService>();
        var controller = new DepotController(serviceMock.Object);

        var pagination = new Pagination<DepotResponse>(
            new List<DepotResponse> { new DepotResponse { Id = 1, DepotCode = "DEPOT1" } },
            1,
            1,
            10
        );

        serviceMock
            .Setup(x => x.GetAll(1, 10))
            .ReturnsAsync(pagination);

        var result = await controller.Get(1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Pagination<DepotResponse>>(okResult.Value);

        serviceMock.Verify(x => x.GetAll(1, 10), Times.Once);
    }

    [Fact]
    public async Task DepotController_GetById_ShouldReturnOk()
    {
        var serviceMock = new Mock<IDepotService>();
        var controller = new DepotController(serviceMock.Object);

        serviceMock
            .Setup(x => x.Get(1))
            .ReturnsAsync(new DepotResponse { Id = 1, DepotCode = "DEPOT1" });

        var result = await controller.Get(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<DepotResponse>(okResult.Value);
        Assert.Equal(1, value.Id);

        serviceMock.Verify(x => x.Get(1), Times.Once);
    }

    [Fact]
    public async Task DepotController_Post_ShouldReturnOk()
    {
        var serviceMock = new Mock<IDepotService>();
        var controller = new DepotController(serviceMock.Object);
        var request = new CreateDepotRequest { DepotCode = "DEPOT1" };

        serviceMock
            .Setup(x => x.Create(request))
            .ReturnsAsync(1);

        var result = await controller.Post(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, okResult.Value);

        serviceMock.Verify(x => x.Create(request), Times.Once);
    }

    [Fact]
    public async Task DepotController_Put_ShouldReturnOk()
    {
        var serviceMock = new Mock<IDepotService>();
        var controller = new DepotController(serviceMock.Object);
        var request = new UpdateDepotRequest { DepotCode = "DEPOT1" };

        serviceMock
            .Setup(x => x.Update(1, request))
            .Returns(Task.CompletedTask);

        var result = await controller.Put(1, request);

        Assert.IsType<OkResult>(result);

        serviceMock.Verify(x => x.Update(1, request), Times.Once);
    }

    [Fact]
    public async Task BlockController_GetAll_ShouldReturnOk()
    {
        var serviceMock = new Mock<IBlockService>();
        var controller = new BlockController(serviceMock.Object);

        var pagination = new Pagination<BlockResponse>(
            new List<BlockResponse> { new BlockResponse { Id = 1, BlockCode = "A" } },
            1,
            1,
            10
        );

        serviceMock
            .Setup(x => x.GetAll(1, 10))
            .ReturnsAsync(pagination);

        var result = await controller.GetAll(1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Pagination<BlockResponse>>(okResult.Value);

        serviceMock.Verify(x => x.GetAll(1, 10), Times.Once);
    }

    [Fact]
    public async Task BlockController_GetById_ShouldReturnOk()
    {
        var serviceMock = new Mock<IBlockService>();
        var controller = new BlockController(serviceMock.Object);

        serviceMock
            .Setup(x => x.Get(1))
            .ReturnsAsync(new BlockResponse { Id = 1, BlockCode = "A" });

        var result = await controller.Get(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<BlockResponse>(okResult.Value);
        Assert.Equal(1, value.Id);

        serviceMock.Verify(x => x.Get(1), Times.Once);
    }

    [Fact]
    public async Task BlockController_Create_ShouldReturnOk()
    {
        var serviceMock = new Mock<IBlockService>();
        var controller = new BlockController(serviceMock.Object);
        var request = new CreateBlockRequest { BlockCode = "A" };

        serviceMock
            .Setup(x => x.Create(request))
            .ReturnsAsync(1);

        var result = await controller.Create(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, okResult.Value);

        serviceMock.Verify(x => x.Create(request), Times.Once);
    }

    [Fact]
    public async Task BlockController_Update_ShouldReturnOk()
    {
        var serviceMock = new Mock<IBlockService>();
        var controller = new BlockController(serviceMock.Object);
        var request = new UpdateBlockRequest { BlockCode = "A" };

        serviceMock
            .Setup(x => x.Update(1, request))
            .Returns(Task.CompletedTask);

        var result = await controller.Update(1, request);

        Assert.IsType<OkResult>(result);

        serviceMock.Verify(x => x.Update(1, request), Times.Once);
    }

    [Fact]
    public async Task CustomerController_GetAll_ShouldReturnOk()
    {
        var serviceMock = new Mock<ICustomerService>();
        var controller = new CustomerController(serviceMock.Object);

        var pagination = new Pagination<CustomerResponse>(
            new List<CustomerResponse> { new CustomerResponse { Id = 1, CustomerCode = "CUS1" } },
            1,
            1,
            10
        );

        serviceMock
            .Setup(x => x.GetAll(1, 10))
            .ReturnsAsync(pagination);

        var result = await controller.GetAll(1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Pagination<CustomerResponse>>(okResult.Value);

        serviceMock.Verify(x => x.GetAll(1, 10), Times.Once);
    }

    [Fact]
    public async Task CustomerController_GetById_ShouldReturnOk()
    {
        var serviceMock = new Mock<ICustomerService>();
        var controller = new CustomerController(serviceMock.Object);

        serviceMock
            .Setup(x => x.Get(1))
            .ReturnsAsync(new CustomerResponse { Id = 1, CustomerCode = "CUS1" });

        var result = await controller.Get(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<CustomerResponse>(okResult.Value);
        Assert.Equal(1, value.Id);

        serviceMock.Verify(x => x.Get(1), Times.Once);
    }

    [Fact]
    public async Task CustomerController_Create_ShouldReturnOk()
    {
        var serviceMock = new Mock<ICustomerService>();
        var controller = new CustomerController(serviceMock.Object);
        var request = new CreateCustomerRequest { CustomerCode = "CUS1" };

        serviceMock
            .Setup(x => x.Create(request))
            .ReturnsAsync(1);

        var result = await controller.Create(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, okResult.Value);

        serviceMock.Verify(x => x.Create(request), Times.Once);
    }

    [Fact]
    public async Task CustomerController_Update_ShouldReturnOk()
    {
        var serviceMock = new Mock<ICustomerService>();
        var controller = new CustomerController(serviceMock.Object);
        var request = new UpdateCustomerRequest { CustomerCode = "CUS1" };

        serviceMock
            .Setup(x => x.Update(1, request))
            .Returns(Task.CompletedTask);

        var result = await controller.Update(1, request);

        Assert.IsType<OkResult>(result);

        serviceMock.Verify(x => x.Update(1, request), Times.Once);
    }

    [Fact]
    public async Task LineOperatorController_GetAll_ShouldReturnOk()
    {
        var serviceMock = new Mock<ILineOperatorService>();
        var controller = new LineOperatorController(serviceMock.Object);

        var pagination = new Pagination<LineOperatorResponse>(
            new List<LineOperatorResponse> { new LineOperatorResponse { Id = 1, LineOperatorCode = "CMA" } },
            1,
            1,
            10
        );

        serviceMock
            .Setup(x => x.GetAll(1, 10))
            .ReturnsAsync(pagination);

        var result = await controller.GetAll(1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Pagination<LineOperatorResponse>>(okResult.Value);

        serviceMock.Verify(x => x.GetAll(1, 10), Times.Once);
    }

    [Fact]
    public async Task LineOperatorController_GetById_ShouldReturnOk()
    {
        var serviceMock = new Mock<ILineOperatorService>();
        var controller = new LineOperatorController(serviceMock.Object);

        serviceMock
            .Setup(x => x.Get(1))
            .ReturnsAsync(new LineOperatorResponse { Id = 1, LineOperatorCode = "CMA" });

        var result = await controller.Get(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<LineOperatorResponse>(okResult.Value);
        Assert.Equal(1, value.Id);

        serviceMock.Verify(x => x.Get(1), Times.Once);
    }

    [Fact]
    public async Task LineOperatorController_Create_ShouldReturnOk()
    {
        var serviceMock = new Mock<ILineOperatorService>();
        var controller = new LineOperatorController(serviceMock.Object);
        var request = new CreateLineOperatorRequest { LineOperatorCode = "CMA" };

        serviceMock
            .Setup(x => x.Create(request))
            .ReturnsAsync(1);

        var result = await controller.Create(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, okResult.Value);

        serviceMock.Verify(x => x.Create(request), Times.Once);
    }

    [Fact]
    public async Task LineOperatorController_Update_ShouldReturnOk()
    {
        var serviceMock = new Mock<ILineOperatorService>();
        var controller = new LineOperatorController(serviceMock.Object);
        var request = new UpdateLineOperatorRequest { LineOperatorCode = "CMA" };

        serviceMock
            .Setup(x => x.Update(1, request))
            .Returns(Task.CompletedTask);

        var result = await controller.Update(1, request);

        Assert.IsType<OkResult>(result);

        serviceMock.Verify(x => x.Update(1, request), Times.Once);
    }

    [Fact]
    public async Task ContainerTypeController_GetAll_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTypeService>();
        var controller = new ContainerTypeController(serviceMock.Object);

        var pagination = new Pagination<ContainerTypeResponse>(
            new List<ContainerTypeResponse> { new ContainerTypeResponse { Id = 1, ContainerTypeCode = "20DC" } },
            1,
            1,
            10
        );

        serviceMock
            .Setup(x => x.GetAll(1, 10))
            .ReturnsAsync(pagination);

        var result = await controller.GetAll(1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Pagination<ContainerTypeResponse>>(okResult.Value);

        serviceMock.Verify(x => x.GetAll(1, 10), Times.Once);
    }

    [Fact]
    public async Task ContainerTypeController_GetById_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTypeService>();
        var controller = new ContainerTypeController(serviceMock.Object);

        serviceMock
            .Setup(x => x.Get(1))
            .ReturnsAsync(new ContainerTypeResponse { Id = 1, ContainerTypeCode = "20DC" });

        var result = await controller.Get(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<ContainerTypeResponse>(okResult.Value);
        Assert.Equal(1, value.Id);

        serviceMock.Verify(x => x.Get(1), Times.Once);
    }

    [Fact]
    public async Task ContainerTypeController_Create_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTypeService>();
        var controller = new ContainerTypeController(serviceMock.Object);
        var request = new CreateContainerTypeRequest { ContainerTypeCode = "20DC" };

        serviceMock
            .Setup(x => x.Create(request))
            .ReturnsAsync(1);

        var result = await controller.Create(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, okResult.Value);

        serviceMock.Verify(x => x.Create(request), Times.Once);
    }

    [Fact]
    public async Task ContainerTypeController_Update_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerTypeService>();
        var controller = new ContainerTypeController(serviceMock.Object);
        var request = new UpdateContainerTypeRequest { ContainerTypeCode = "20DC" };

        serviceMock
            .Setup(x => x.Update(1, request))
            .Returns(Task.CompletedTask);

        var result = await controller.Update(1, request);

        Assert.IsType<OkResult>(result);

        serviceMock.Verify(x => x.Update(1, request), Times.Once);
    }

    [Fact]
    public async Task DeliveryOrderController_GetAll_ShouldReturnOk()
    {
        var serviceMock = new Mock<IDeliveryOrderService>();
        var controller = new DeliveryOrderController(serviceMock.Object);

        var pagination = new Pagination<DeliveryOrderResponse>(
            new List<DeliveryOrderResponse> { new DeliveryOrderResponse { Id = 1, DONumber = "DO001" } },
            1,
            1,
            10
        );

        serviceMock
            .Setup(x => x.GetAll(1, 10))
            .ReturnsAsync(pagination);

        var result = await controller.GetAll(1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Pagination<DeliveryOrderResponse>>(okResult.Value);

        serviceMock.Verify(x => x.GetAll(1, 10), Times.Once);
    }

    [Fact]
    public async Task DeliveryOrderController_GetById_ShouldReturnOk()
    {
        var serviceMock = new Mock<IDeliveryOrderService>();
        var controller = new DeliveryOrderController(serviceMock.Object);

        serviceMock
            .Setup(x => x.Get(1))
            .ReturnsAsync(new DeliveryOrderResponse { Id = 1, DONumber = "DO001" });

        var result = await controller.Get(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<DeliveryOrderResponse>(okResult.Value);
        Assert.Equal(1, value.Id);

        serviceMock.Verify(x => x.Get(1), Times.Once);
    }

    [Fact]
    public async Task DeliveryOrderController_Create_ShouldReturnOk()
    {
        var serviceMock = new Mock<IDeliveryOrderService>();
        var controller = new DeliveryOrderController(serviceMock.Object);
        var request = new CreateDeliveryOrderRequest { DONumber = "DO001" };

        serviceMock
            .Setup(x => x.Create(request))
            .ReturnsAsync(1);

        var result = await controller.Create(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, okResult.Value);

        serviceMock.Verify(x => x.Create(request), Times.Once);
    }

    [Fact]
    public async Task DeliveryOrderController_Update_ShouldReturnOk()
    {
        var serviceMock = new Mock<IDeliveryOrderService>();
        var controller = new DeliveryOrderController(serviceMock.Object);
        var request = new UpdateDeliveryOrderRequest { DONumber = "DO001" };

        serviceMock
            .Setup(x => x.Update(1, request))
            .Returns(Task.CompletedTask);

        var result = await controller.Update(1, request);

        Assert.IsType<OkResult>(result);

        serviceMock.Verify(x => x.Update(1, request), Times.Once);
    }

    [Fact]
    public async Task ContainerPositionController_GetAll_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerPositionService>();
        var controller = new ContainerPositionController(serviceMock.Object);

        var pagination = new Pagination<ContainerPositionResponse>(
            new List<ContainerPositionResponse> { new ContainerPositionResponse { Id = 1, ContainerId = 1 } },
            1,
            1,
            10
        );

        serviceMock
            .Setup(x => x.GetAll(1, 10))
            .ReturnsAsync(pagination);

        var result = await controller.GetAll(1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Pagination<ContainerPositionResponse>>(okResult.Value);

        serviceMock.Verify(x => x.GetAll(1, 10), Times.Once);
    }

    [Fact]
    public async Task ContainerPositionController_GetById_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerPositionService>();
        var controller = new ContainerPositionController(serviceMock.Object);

        serviceMock
            .Setup(x => x.Get(1))
            .ReturnsAsync(new ContainerPositionResponse { Id = 1, ContainerId = 1 });

        var result = await controller.Get(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<ContainerPositionResponse>(okResult.Value);
        Assert.Equal(1, value.Id);

        serviceMock.Verify(x => x.Get(1), Times.Once);
    }

    [Fact]
    public async Task ContainerPositionController_Create_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerPositionService>();
        var controller = new ContainerPositionController(serviceMock.Object);
        var request = new CreateContainerPositionRequest { ContainerId = 1, BlockId = 1 };

        serviceMock
            .Setup(x => x.Create(request))
            .ReturnsAsync(1);

        var result = await controller.Create(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, okResult.Value);

        serviceMock.Verify(x => x.Create(request), Times.Once);
    }

    [Fact]
    public async Task ContainerPositionController_Update_ShouldReturnOk()
    {
        var serviceMock = new Mock<IContainerPositionService>();
        var controller = new ContainerPositionController(serviceMock.Object);
        var request = new UpdateContainerPositionRequest { ContainerId = 1, BlockId = 1 };

        serviceMock
            .Setup(x => x.Update(1, request))
            .Returns(Task.CompletedTask);

        var result = await controller.Update(1, request);

        Assert.IsType<OkResult>(result);

        serviceMock.Verify(x => x.Update(1, request), Times.Once);
    }
}
