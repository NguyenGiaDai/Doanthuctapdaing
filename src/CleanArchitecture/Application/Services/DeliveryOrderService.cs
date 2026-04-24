using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.DeliveryOrder;
using CleanArchitecture.Shared.Models.Errors;

namespace CleanArchitecture.Application.Services;

public class DeliveryOrderService(IUnitOfWork unitOfWork, IMapper mapper) : IDeliveryOrderService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Pagination<DeliveryOrderResponse>> GetAll(int pageNumber, int pageSize)
    {
        var deliveryOrders = await _unitOfWork.DeliveryOrderRepository.ToPagination(
            pageIndex: pageNumber,
            pageSize: pageSize,
            orderBy: x => x.Id,
            ascending: true,
            selector: x => new DeliveryOrderResponse
            {
                Id = x.Id,
                DONumber = x.DONumber,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer != null ? x.Customer.CustomerName : null,
                LineOperatorId = x.LineOperatorId,
                LineOperatorName = x.LineOperator != null ? x.LineOperator.LineOperatorName : null,
                ContainerTypeId = x.ContainerTypeId,
                ContainerTypeName = x.ContainerType != null ? x.ContainerType.ContainerTypeName : null,
                Quantity = x.Quantity,
                ExpiryDate = x.ExpiryDate,
                OrderDate = x.OrderDate,
                VesselVoyage = x.VesselVoyage,
                OrderStatus = x.OrderStatus
            }
        );

        return deliveryOrders;
    }

    public async Task<DeliveryOrderResponse> Get(int id)
    {
        var deliveryOrder = await _unitOfWork.DeliveryOrderRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (deliveryOrder == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Delivery order not found");

        return _mapper.Map<DeliveryOrderResponse>(deliveryOrder);
    }

    public async Task<int> Create(CreateDeliveryOrderRequest request)
    {
        await ValidateRequest(
            request.DONumber,
            request.CustomerId,
            request.LineOperatorId,
            request.ContainerTypeId,
            request.Quantity,
            request.ExpiryDate);

        var deliveryOrder = _mapper.Map<DeliveryOrder>(request);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.DeliveryOrderRepository.AddAsync(deliveryOrder), CancellationToken.None);

        return deliveryOrder.Id;
    }

    public async Task Update(int id, UpdateDeliveryOrderRequest request)
    {
        var deliveryOrder = await _unitOfWork.DeliveryOrderRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (deliveryOrder == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Delivery order not found");

        await ValidateRequest(
            request.DONumber,
            request.CustomerId,
            request.LineOperatorId,
            request.ContainerTypeId,
            request.Quantity,
            request.ExpiryDate,
            id);

        deliveryOrder.DONumber = request.DONumber;
        deliveryOrder.CustomerId = request.CustomerId;
        deliveryOrder.LineOperatorId = request.LineOperatorId;
        deliveryOrder.ContainerTypeId = request.ContainerTypeId;
        deliveryOrder.Quantity = request.Quantity;
        deliveryOrder.ExpiryDate = request.ExpiryDate;
        deliveryOrder.OrderDate = request.OrderDate;
        deliveryOrder.VesselVoyage = request.VesselVoyage;
        deliveryOrder.OrderStatus = request.OrderStatus;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.DeliveryOrderRepository.Update(deliveryOrder);
        }, CancellationToken.None);
    }

    private async Task ValidateRequest(
        string doNumber,
        int customerId,
        int lineOperatorId,
        int containerTypeId,
        int quantity,
        DateTime expiryDate,
        int? currentId = null)
    {
        if (string.IsNullOrWhiteSpace(doNumber))
            throw BuildValidationException("DO number is required");

        var isDuplicate = await _unitOfWork.DeliveryOrderRepository.AnyAsync(x =>
            x.DONumber == doNumber && (!currentId.HasValue || x.Id != currentId.Value));

        if (isDuplicate)
            throw new UserFriendlyException(ErrorCode.Conflict, "DO number already exists");

        var customer = await _unitOfWork.CustomerRepository.FirstOrDefaultAsync(x => x.Id == customerId);
        if (customer == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Customer not found");

        var lineOperator = await _unitOfWork.LineOperatorRepository.FirstOrDefaultAsync(x => x.Id == lineOperatorId);
        if (lineOperator == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Line operator not found");

        var containerType = await _unitOfWork.ContainerTypeRepository.FirstOrDefaultAsync(x => x.Id == containerTypeId);
        if (containerType == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Container type not found");

        if (quantity <= 0)
            throw BuildValidationException("Quantity must be greater than 0");

        if (expiryDate == default)
            throw BuildValidationException("Expiry date is required");
    }

    private static ValidationException BuildValidationException(string message)
    {
        return new ValidationException(
            new ErrorResponse(
                [
                    new Error("Validation.DeliveryOrder", message)
                ]
            )
        );
    }
}
