using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Products.DTOs;

namespace Shop.Application.Features.Products.Queries.GetAll;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, IActionResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllProductsHandler> _logger;

    public GetAllProductsHandler(IProductRepository productRepository, IMapper mapper, ILogger<GetAllProductsHandler> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var paged = await _productRepository.GetAllAsync(request, cancellationToken);

            var result = new PagedResult<ProductDto>
            {
                Items = _mapper.Map<List<ProductDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize
            };

            return new OkObjectResult(ApiResponse<PagedResult<ProductDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
