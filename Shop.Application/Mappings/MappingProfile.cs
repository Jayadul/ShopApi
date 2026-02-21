using AutoMapper;
using Shop.Application.Features.Customers.Commands.Create;
using Shop.Application.Features.Customers.DTOs;
using Shop.Application.Features.Orders.DTOs;
using Shop.Application.Features.Products.Commands.Create;
using Shop.Application.Features.Products.DTOs;
using Shop.Domain.Entities;

namespace Shop.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Customer
        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateCustomerCommand, Customer>();

        // Product
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductCommand, Product>();

        // OrderItem
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.UnitPrice * src.Quantity));

        // Order
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : string.Empty))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));
    }
}
