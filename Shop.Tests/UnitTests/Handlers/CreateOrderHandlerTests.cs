using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Orders.Commands.Create;
using Shop.Application.Features.Orders.DTOs;
using Shop.Application.Mappings;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Xunit;
using FluentAssertions;

namespace Shop.Tests.UnitTests.Handlers;

public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepo;
    private readonly Mock<ICustomerRepository> _mockCustomerRepo;
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<CreateOrderHandler>> _mockLogger;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _mockOrderRepo = new Mock<IOrderRepository>();
        _mockCustomerRepo = new Mock<ICustomerRepository>();
        _mockProductRepo = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<CreateOrderHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new CreateOrderHandler(
            _mockOrderRepo.Object,
            _mockCustomerRepo.Object,
            _mockProductRepo.Object,
            _mapper,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsOkWithCorrectTotal()
    {
        // Arrange
        var customer = new Customer { Id = 1, FullName = "John Doe", Email = "john@example.com" };
        var products = new List<Product>
        {
            new Product { Id = 10, Name = "Widget A", Price = 25.00m, Stock = 100 },
            new Product { Id = 20, Name = "Gadget B", Price = 50.00m, Stock = 50 }
        };

        var command = new CreateOrderCommand
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 10, Quantity = 2 },  // 2 × 25 = 50
                new CreateOrderItemDto { ProductId = 20, Quantity = 1 }   // 1 × 50 = 50
            },
            CreatedBy = 1
        };

        _mockCustomerRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _mockProductRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(products);
        _mockProductRepo.Setup(r => r.Update(It.IsAny<Product>()));
        _mockOrderRepo.Setup(r => r.Add(It.IsAny<Order>()));
        _mockOrderRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<OrderDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.TotalAmount.Should().Be(100.00m); // 50 + 50
        response.Data.Items.Should().HaveCount(2);
        response.Data.Status.Should().Be(OrderStatus.Pending);

        _mockOrderRepo.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
        // Stock deduction applied for both products
        _mockProductRepo.Verify(r => r.Update(It.IsAny<Product>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = 999,
            Items = new List<CreateOrderItemDto> { new CreateOrderItemDto { ProductId = 1, Quantity = 1 } }
        };

        _mockCustomerRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Customer?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Contain("Customer not found");
        _mockOrderRepo.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InsufficientStock_ReturnsBadRequest()
    {
        // Arrange
        var customer = new Customer { Id = 1, FullName = "Jane Doe", Email = "jane@example.com" };
        var products = new List<Product>
        {
            new Product { Id = 10, Name = "Limited Item", Price = 10.00m, Stock = 1 }
        };

        var command = new CreateOrderCommand
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 10, Quantity = 5 }  // requesting 5 but only 1 in stock
            }
        };

        _mockCustomerRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _mockProductRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Contain("Insufficient stock");
        _mockOrderRepo.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ReturnsBadRequest()
    {
        // Arrange
        var customer = new Customer { Id = 1, FullName = "Bob", Email = "bob@example.com" };
        var command = new CreateOrderCommand
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 999, Quantity = 1 }  // non-existent product
            }
        };

        _mockCustomerRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _mockProductRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<Product>());  // returns empty - product not found

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Contain("not found");
    }
}
