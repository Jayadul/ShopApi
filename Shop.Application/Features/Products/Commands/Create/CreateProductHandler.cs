using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Products.DTOs;
using Shop.Domain.Entities;

namespace Shop.Application.Features.Products.Commands.Create;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, IActionResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(IProductRepository productRepository, IMapper mapper, ILogger<CreateProductHandler> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = _mapper.Map<Product>(request);
            _productRepository.Add(product);
            await _productRepository.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<ProductDto>(product);
            return new OkObjectResult(ApiResponse<ProductDto>.Ok(dto, "Product created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
