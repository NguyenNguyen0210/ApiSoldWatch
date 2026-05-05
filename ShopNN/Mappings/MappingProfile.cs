using AutoMapper;
using ShopNN.DTOs;
using ShopNN.Entities;

namespace ShopNN.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product Mappings
            CreateMap<Product, ProductResponseDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));
            CreateMap<ProductRequestDTO, Product>();

            // Category Mappings
            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryRequestDTO, Category>();

            // Cart Mappings
            CreateMap<Cart, CartDTO>();
            CreateMap<CartItem, CartItemDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.Price));
            
            // Order Mappings
            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()));
            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));
        }
    }
}
