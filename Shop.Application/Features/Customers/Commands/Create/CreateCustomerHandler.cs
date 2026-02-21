using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Customers.DTOs;
using Shop.Domain.Entities;

namespace Shop.Application.Features.Customers.Commands.Create;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, IActionResult>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCustomerHandler> _logger;

    public CreateCustomerHandler(ICustomerRepository customerRepository, IMapper mapper, ILogger<CreateCustomerHandler> logger)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailExists = await _customerRepository.EmailExistsAsync(request.Email, null, cancellationToken);
            if (emailExists)
                return new BadRequestObjectResult(ApiResponse<string>.Fail("Email already in use."));

            var customer = _mapper.Map<Customer>(request);
            _customerRepository.Add(customer);
            await _customerRepository.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<CustomerDto>(customer);
            return new OkObjectResult(ApiResponse<CustomerDto>.Ok(dto, "Customer created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
