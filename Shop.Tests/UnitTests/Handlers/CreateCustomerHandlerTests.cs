using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Customers.Commands.Create;
using Shop.Application.Features.Customers.DTOs;
using Shop.Application.Mappings;
using Shop.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace Shop.Tests.UnitTests.Handlers;

public class CreateCustomerHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockRepo;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<CreateCustomerHandler>> _mockLogger;
    private readonly CreateCustomerHandler _handler;

    public CreateCustomerHandlerTests()
    {
        _mockRepo = new Mock<ICustomerRepository>();
        _mockLogger = new Mock<ILogger<CreateCustomerHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new CreateCustomerHandler(_mockRepo.Object, _mapper, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsOkWithCustomerDto()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Phone = "123456789",
            Address = "123 Main St",
            CreatedBy = 1
        };

        _mockRepo.Setup(r => r.EmailExistsAsync(command.Email, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);
        _mockRepo.Setup(r => r.Add(It.IsAny<Customer>()));
        _mockRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Email.Should().Be(command.Email);

        _mockRepo.Verify(r => r.Add(It.IsAny<Customer>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FullName = "Jane Doe",
            Email = "duplicate@example.com",
            Phone = "111222333",
            Address = "456 Elm St"
        };

        _mockRepo.Setup(r => r.EmailExistsAsync(command.Email, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeFalse();

        _mockRepo.Verify(r => r.Add(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryThrows_Returns500()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FullName = "Test User",
            Email = "test@example.com",
            Phone = "000111222",
            Address = "789 Oak Ave"
        };

        _mockRepo.Setup(r => r.EmailExistsAsync(command.Email, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);
        _mockRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }
}
