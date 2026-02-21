using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Products.DTOs;

namespace Shop.Application.Features.Products.Queries.GetById;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, IActionResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductByIdHandler> _logger;

    public GetProductByIdHandler(IProductRepository productRepository, IMapper mapper, ILogger<GetProductByIdHandler> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
                return new NotFoundObjectResult(ApiResponse<string>.Fail("Product not found."));

            var dto = _mapper.Map<ProductDto>(product);
            return new OkObjectResult(ApiResponse<ProductDto>.Ok(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {Id}", request.Id);
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
