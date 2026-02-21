using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Customers.DTOs;

namespace Shop.Application.Features.Customers.Queries.GetAll;

public class GetAllCustomersHandler : IRequestHandler<GetAllCustomersQuery, IActionResult>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllCustomersHandler> _logger;

    public GetAllCustomersHandler(ICustomerRepository customerRepository, IMapper mapper, ILogger<GetAllCustomersHandler> logger)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pagedCustomers = await _customerRepository.GetAllAsync(request, cancellationToken);

            var result = new PagedResult<CustomerDto>
            {
                Items = _mapper.Map<List<CustomerDto>>(pagedCustomers.Items),
                TotalCount = pagedCustomers.TotalCount,
                PageNumber = pagedCustomers.PageNumber,
                PageSize = pagedCustomers.PageSize
            };

            return new OkObjectResult(ApiResponse<PagedResult<CustomerDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all customers");
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
