using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Products.DTOs;

namespace Shop.Application.Features.Products.Commands.Update;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, IActionResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProductHandler> _logger;

    public UpdateProductHandler(IProductRepository productRepository, IMapper mapper, ILogger<UpdateProductHandler> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
                return new NotFoundObjectResult(ApiResponse<string>.Fail("Product not found."));

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Stock = request.Stock;
            product.UpdatedBy = request.UpdatedBy;
            product.UpdatedDate = DateTime.UtcNow;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<ProductDto>(product);
            return new OkObjectResult(ApiResponse<ProductDto>.Ok(dto, "Product updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", request.Id);
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
