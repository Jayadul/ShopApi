using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Customers.DTOs;

namespace Shop.Application.Features.Customers.Queries.GetById;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, IActionResult>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCustomerByIdHandler> _logger;

    public GetCustomerByIdHandler(ICustomerRepository customerRepository, IMapper mapper, ILogger<GetCustomerByIdHandler> logger)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (customer == null)
                return new NotFoundObjectResult(ApiResponse<string>.Fail("Customer not found."));

            var dto = _mapper.Map<CustomerDto>(customer);
            return new OkObjectResult(ApiResponse<CustomerDto>.Ok(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {Id}", request.Id);
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
