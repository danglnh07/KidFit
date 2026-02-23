using AutoMapper;
using KidFit.Models;
using KidFit.ViewModels;
using X.PagedList;

namespace KidFit.Profiles
{
    public class PagedListConverter<TSource, TDestination> : ITypeConverter<IPagedList<TSource>, IPagedList<TDestination>>
    {
        public IPagedList<TDestination> Convert(IPagedList<TSource> source,
                                                IPagedList<TDestination> destination,
                                                ResolutionContext context)
        {
            var mappedItems = context.Mapper.Map<List<TDestination>>(source);
            return new StaticPagedList<TDestination>(mappedItems, source.PageNumber, source.PageSize, source.TotalItemCount);
        }
    }

    public class AppProfile : Profile
    {
        public AppProfile()
        {
            CreateMap(typeof(IPagedList<>), typeof(IPagedList<>)).ConvertUsing(typeof(PagedListConverter<,>));

            CreateMap<CreateAccountViewModel, ApplicationUser>();
            CreateMap<ApplicationUser, AccountViewModel>();
            CreateMap<AccountViewModel, ApplicationUser>();
            CreateMap<ApplicationUser, AccountViewModelWithRole>();
        }
    }

}
