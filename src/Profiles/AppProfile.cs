using AutoMapper;
using KidFit.Dtos;
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
            CreateMap<UpdateCardCategoryDto, CardCategory>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                {
                    if (srcMember is null) return false;
                    if (srcMember is string str && string.IsNullOrWhiteSpace(str)) return false;
                    return true;
                }));

            CreateMap<CreateCardDto, Card>();
            CreateMap<Card, ViewCardDto>();
            CreateMap<UpdateCardDto, Card>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                {
                    if (srcMember is null) return false;
                    if (srcMember is string str && string.IsNullOrWhiteSpace(str)) return false;
                    return true;

                }));

            CreateMap<CreateLessonDto, Lesson>();
            CreateMap<Lesson, ViewLessonDto>();
            CreateMap<UpdateLessonDto, Lesson>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                {
                    if (srcMember is null) return false;
                    if (srcMember is null) return false;
                    return true;
                }));

            CreateMap<CreateModuleDto, Module>();
            CreateMap<Module, ViewModuleDto>();
            CreateMap<UpdateModuleDto, Module>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                {
                    if (srcMember is null) return false;
                    if (srcMember is string str && string.IsNullOrWhiteSpace(str)) return false;
                    if (srcMember is int num && num == 0) return false;
                    return true;
                }));

            // Paged list mappings: AutoMapper don't know how to map IPagedList
            // since they also contain paging metadata, so we need to manually instruct it
            CreateMap(typeof(IPagedList<>), typeof(IPagedList<>)).ConvertUsing(typeof(PagedListConverter<,>));
        }
    }

    public class PagedListConverter<TSource, TDestination> : ITypeConverter<IPagedList<TSource>, IPagedList<TDestination>>
    {
        public IPagedList<TDestination> Convert(IPagedList<TSource> source,
                                                IPagedList<TDestination> destination,
                                                ResolutionContext context)
        {
            var mappedItems = context.Mapper.Map<List<TDestination>>(source);
            return new StaticPagedList<TDestination>(mappedItems,
                                                     source.PageNumber,
                                                     source.PageSize,
                                                     source.TotalItemCount);
        }
    }
}
