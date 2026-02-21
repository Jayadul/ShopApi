using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Orders.DTOs;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Application.Features.Orders.Commands.Create;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, IActionResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IMapper mapper,
        ILogger<CreateOrderHandler> logger)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate customer exists
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
                return new BadRequestObjectResult(ApiResponse<string>.Fail("Customer not found."));

            // Load all requested products in a single query (no N+1)
            var requestedProductIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _productRepository.GetByIdsAsync(requestedProductIds, cancellationToken);

            // Validate every requested product exists and has sufficient stock
            var productMap = products.ToDictionary(p => p.Id);
            foreach (var item in request.Items)
            {
                if (!productMap.TryGetValue(item.ProductId, out var product))
                    return new BadRequestObjectResult(ApiResponse<string>.Fail($"Product with Id {item.ProductId} not found."));

                if (product.Stock < item.Quantity)
                    return new BadRequestObjectResult(ApiResponse<string>.Fail(
                        $"Insufficient stock for '{product.Name}'. Available: {product.Stock}, Requested: {item.Quantity}."));
            }

            // Build order items, capture price at time of order, deduct stock
            var orderItems = new List<OrderItem>();
            foreach (var item in request.Items)
            {
                var product = productMap[item.ProductId];

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,   // snapshot price
                    CreationDate = DateTime.UtcNow
                });

                product.Stock -= item.Quantity;  // deduct from inventory
                _productRepository.Update(product);
            }

            // Create order with calculated total
            var order = new Order
            {
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                CustomerId = request.CustomerId,
                TotalAmount = orderItems.Sum(i => i.UnitPrice * i.Quantity),
                Status = OrderStatus.Pending,
                Notes = request.Notes,
                CreatedBy = request.CreatedBy,
                CreationDate = DateTime.UtcNow,
                OrderItems = orderItems
            };

            _orderRepository.Add(order);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            // Build response DTO
            var dto = new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = customer.FullName,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Notes = order.Notes,
                CreationDate = order.CreationDate,
                Items = orderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = productMap[oi.ProductId].Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.UnitPrice * oi.Quantity
                }).ToList()
            };

            return new OkObjectResult(ApiResponse<OrderDto>.Ok(dto, "Order created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", request.CustomerId);
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
