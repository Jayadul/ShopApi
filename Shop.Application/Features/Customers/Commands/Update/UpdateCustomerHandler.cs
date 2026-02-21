using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Customers.DTOs;

namespace Shop.Application.Features.Customers.Commands.Update;

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, IActionResult>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCustomerHandler> _logger;

    public UpdateCustomerHandler(ICustomerRepository customerRepository, IMapper mapper, ILogger<UpdateCustomerHandler> logger)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (customer == null)
                return new NotFoundObjectResult(ApiResponse<string>.Fail("Customer not found."));

            var emailExists = await _customerRepository.EmailExistsAsync(request.Email, request.Id, cancellationToken);
            if (emailExists)
                return new BadRequestObjectResult(ApiResponse<string>.Fail("Email already in use."));

            customer.FullName = request.FullName;
            customer.Email = request.Email;
            customer.Phone = request.Phone;
            customer.Address = request.Address;
            customer.UpdatedBy = request.UpdatedBy;
            customer.UpdatedDate = DateTime.UtcNow;

            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<CustomerDto>(customer);
            return new OkObjectResult(ApiResponse<CustomerDto>.Ok(dto, "Customer updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {Id}", request.Id);
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
