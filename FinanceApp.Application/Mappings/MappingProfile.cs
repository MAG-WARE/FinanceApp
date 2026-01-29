using AutoMapper;
using FinanceApp.Application.DTOs;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<RegisterDto, User>();

        // Account mappings
        CreateMap<Account, AccountDto>()
            .ForMember(dest => dest.CurrentBalance, opt => opt.Ignore());
        CreateMap<CreateAccountDto, Account>();
        CreateMap<UpdateAccountDto, Account>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Category mappings
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Transaction mappings
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account.Name))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.DestinationAccountName, opt => opt.MapFrom(src => src.DestinationAccount != null ? src.DestinationAccount.Name : null));
        CreateMap<CreateTransactionDto, Transaction>();
        CreateMap<UpdateTransactionDto, Transaction>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Budget mappings
        CreateMap<Budget, BudgetDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.SpentAmount, opt => opt.Ignore())
            .ForMember(dest => dest.RemainingAmount, opt => opt.Ignore())
            .ForMember(dest => dest.PercentageUsed, opt => opt.Ignore());
        CreateMap<CreateBudgetDto, Budget>();
        CreateMap<UpdateBudgetDto, Budget>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Goal mappings
        CreateMap<Goal, GoalDto>()
            .ForMember(dest => dest.ProgressPercentage, opt => opt.Ignore())
            .ForMember(dest => dest.RemainingAmount, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.IsOwner, opt => opt.Ignore())
            .ForMember(dest => dest.IsShared, opt => opt.MapFrom(src => src.GoalUsers.Count > 1))
            .ForMember(dest => dest.SharedWith, opt => opt.Ignore());
        CreateMap<CreateGoalDto, Goal>();
        CreateMap<UpdateGoalDto, Goal>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // UserGroup mappings
        CreateMap<UserGroup, UserGroupDto>()
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser.Name))
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count))
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members));
        CreateMap<CreateUserGroupDto, UserGroup>();
        CreateMap<UpdateUserGroupDto, UserGroup>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // UserGroupMember mappings
        CreateMap<UserGroupMember, GroupMemberDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

        // GoalUser mappings
        CreateMap<GoalUser, GoalUserDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name));
    }
}
