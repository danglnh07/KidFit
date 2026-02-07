using AutoMapper;
using KidFit.Dtos.Requests;
using KidFit.Dtos.Responses;
using KidFit.Models;
using X.PagedList;

namespace KidFit.Profiles
{
    public class AppProfile : Profile
    {
        public AppProfile()
        {
            CreateMap<CreateCardCategoryDto, CardCategory>();
            CreateMap<CardCategory, ViewCardCategoryDto>();
            CreateMap<UpdateCardCategoryDto, CardCategory>();

            CreateMap<CreateCardDto, Card>();
            CreateMap<Card, ViewCardDto>();
            CreateMap<UpdateCardDto, Card>();

            CreateMap<CreateLessonDto, Lesson>();
            CreateMap<UpdateLessonDto, Lesson>();
            CreateMap<Lesson, ViewLessonDto>();

            CreateMap<CreateModuleDto, Module>();
            CreateMap<UpdateModuleDto, Module>();
            CreateMap<Module, ViewModuleDto>();

            // Paged list mappings: AutoMapper don't know how to map IPagedList
            // since they also contain paging metadata, so we need to manually instruct it
            CreateMap(typeof(IPagedList<>), typeof(IPagedList<>)).ConvertUsing(typeof(PagedListConverter<,>));
        }
    }

    public class PagedListConverter<TSource, TDestination> : ITypeConverter<IPagedList<TSource>, IPagedList<TDestination>>
    {
        public IPagedList<TDestination> Convert(IPagedList<TSource> source, IPagedList<TDestination> destination, ResolutionContext context)
        {
            var mappedItems = context.Mapper.Map<List<TDestination>>(source);
            return new StaticPagedList<TDestination>(mappedItems, source.PageNumber, source.PageSize, source.TotalItemCount);
        }
    }
}
