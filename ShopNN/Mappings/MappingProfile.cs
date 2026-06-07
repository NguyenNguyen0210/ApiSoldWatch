using AutoMapper;
using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
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
            CreateMap<Category, CategoryResponseDTO>();
            CreateMap<CategoryRequestDTO, Category>();

            // Cart Mappings
            CreateMap<Cart, CartResponseDTO>();
            CreateMap<CartItem, CartItemResponseDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl));
            
            // Order Mappings
            CreateMap<Order, OrderResponseDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()));
                
            CreateMap<OrderItem, OrderItemResponseDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));
        }
    }
}

