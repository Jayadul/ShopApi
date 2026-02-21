using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;

namespace Shop.Application.Features.Customers.Commands.Delete;

public class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, IActionResult>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<DeleteCustomerHandler> _logger;

    public DeleteCustomerHandler(ICustomerRepository customerRepository, ILogger<DeleteCustomerHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (customer == null)
                return new NotFoundObjectResult(ApiResponse<string>.Fail("Customer not found."));

            // Soft delete - archive instead of hard delete (GDPR: data retention)
            customer.IsArchived = true;
            customer.UpdatedDate = DateTime.UtcNow;

            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync(cancellationToken);

            return new OkObjectResult(ApiResponse<bool>.Ok(true, "Customer archived successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {Id}", request.Id);
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
