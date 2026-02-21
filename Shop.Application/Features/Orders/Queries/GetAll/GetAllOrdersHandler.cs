using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Orders.DTOs;

namespace Shop.Application.Features.Orders.Queries.GetAll;

public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, IActionResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllOrdersHandler> _logger;

    public GetAllOrdersHandler(IOrderRepository orderRepository, IMapper mapper, ILogger<GetAllOrdersHandler> logger)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pagedOrders = await _orderRepository.GetAllAsync(
                request,
                request.CustomerId,
                request.FromDate,
                request.ToDate,
                request.Status,
                cancellationToken);

            var result = new PagedResult<OrderDto>
            {
                Items = _mapper.Map<List<OrderDto>>(pagedOrders.Items),
                TotalCount = pagedOrders.TotalCount,
                PageNumber = pagedOrders.PageNumber,
                PageSize = pagedOrders.PageSize
            };

            return new OkObjectResult(ApiResponse<PagedResult<OrderDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders");
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
