using AutoMapper;
using yhctapp.Model.DTO;
using yhctapp.Model.DTO.UserDTO;
using yhctapp.Model.Enitity;

namespace yhctapp.Helpper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Catogerymenu, CategorymenuVM>().ReverseMap();
            CreateMap<CategorymenuVM, Catogerymenu>().ForMember(dest => dest.Id, opt => opt.Ignore());
            // menu 
            CreateMap<Menu, MenuVM>().ReverseMap();
            CreateMap<MenuVM, Menu>().ForMember(dest => dest.Id, opt => opt.Ignore());
            // permison
            CreateMap<RolePermission, RolePermissonVM>().ReverseMap();
            CreateMap<RolePermissonVM, RolePermission>().ForMember(dest => dest.Id_RolePermission, opt => opt.Ignore());
            //user
            CreateMap<ApplicationUser,ListuserVM>().ReverseMap();
            CreateMap<ApplicationUser,UserVM>().ReverseMap();
            CreateMap<UserVM, ApplicationUser>().ForMember(dest => dest.Id, opt => opt.Ignore());
            // DepartmentRoom
            CreateMap<DepartmentRoom, DepartmentRoomVM>().ReverseMap();
            CreateMap<DepartmentRoomVM, DepartmentRoom>();
            // document group
            CreateMap<DocumentGroup, DocumentVM>().ReverseMap();
            CreateMap<DocumentVM, DocumentGroup>().ForMember(dest => dest.Id, opt => opt.Ignore());
            // document record
            CreateMap<DocumentRecord, DocumentRecordVM>().ReverseMap();
            CreateMap<DocumentRecordVM, DocumentRecord>().ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
