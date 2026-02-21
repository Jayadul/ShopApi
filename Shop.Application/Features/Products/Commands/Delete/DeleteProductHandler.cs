using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;

namespace Shop.Application.Features.Products.Commands.Delete;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, IActionResult>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<DeleteProductHandler> _logger;

    public DeleteProductHandler(IProductRepository productRepository, ILogger<DeleteProductHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
                return new NotFoundObjectResult(ApiResponse<string>.Fail("Product not found."));

            product.IsArchived = true;
            product.UpdatedDate = DateTime.UtcNow;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync(cancellationToken);

            return new OkObjectResult(ApiResponse<bool>.Ok(true, "Product archived successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", request.Id);
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
