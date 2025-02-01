using Bookify.Application.Common.Dto;

namespace Bookify.Core.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Categories
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryFormViewModel, Category>().ReverseMap();
            CreateMap<Category, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.CategoryName));

            //Authors
            CreateMap<Author, AuthorViewModel>();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();
            CreateMap<Author, SelectListItem>()
              .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.id))
              .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //Books
            CreateMap<BookFormViewModel, Book>()
                .ReverseMap()
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore());

            CreateMap<Book, BookViewModel>()
              .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.Name))
              .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => c.Category!.CategoryName).ToList()))
              .ForMember(dest => dest.Copies, opt => opt.MapFrom(src => src.BookCopies)); 
            
            CreateMap<Book, BookRowViewModel>()
              .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.Name));

            CreateMap<BookDto, BookViewModel>();


            //Copy
            CreateMap<BookCopy, BookCopyViewModel>()
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title))
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.Book!.Id))
                .ForMember(dest => dest.BookThumbnailUrl, opt => opt.MapFrom(src => src.Book!.ImageThumbnailUrl))
                .ReverseMap();

            CreateMap<BookCopyFormViewModel, BookCopy>().ReverseMap();

            //Users
            CreateMap<ApplicationUser, UserViewModel>();
            CreateMap<UserFormViewModel, ApplicationUser>()
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()))
                .ReverseMap();

            CreateMap<ResetPasswordFomViewModel, UserViewModel>();

            //subscripers
            CreateMap<Subscriber, SubscriberFormViewModel>()
                .ReverseMap()
                 .ForMember(dest => dest.Area, opt => opt.Ignore())
                .ForMember(dest => dest.Governorate, opt => opt.Ignore());

            CreateMap<Area, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.AreaId.ToString()))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            CreateMap<Governorate, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            CreateMap<Subscriber, SubscriberViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area!.Name))
                .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate!.Name));

            CreateMap<Subscriber, SubscriberSearchResultViewModel>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<Subscription, subscriptionViewModel>();

            //Rentals
            CreateMap<Rental, RentalViewModel>();
            CreateMap<Rental, RentalFormViewModel>();
            CreateMap<RentalCopy, RentalCopyViewModel>();


        }
    }
}
