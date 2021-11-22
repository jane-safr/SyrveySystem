using AutoMapper;
using BLL.SurveySystem.DTO;
using Web.SurveySystem.Models.ViewModels;

namespace Web.SurveySystem.Helpers
{
    public class MapperConfigVm
    {
        public static IMapper MapperConfigAll()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<UserAnswerDTO, UserAnswerVM>();
                c.CreateMap<ApplicationUserDTO, ApplicationUserVM>();
                c.CreateMap<AnswerDTO, AnswerVM>();
                c.CreateMap<FixedAnswerDTO, FixedAnswerVM>();
                c.CreateMap<QuestionDTO, QuestionVM>().ForMember(i=>i.Indicator, opt=>opt.NullSubstitute(new IndicatorDTO())); 
                c.CreateMap<QuestionTypeDTO, QuestionTypeVM>();
                c.CreateMap<SurveyTypeDTO, SurveyTypeVM>().ForMember(m => m.FullName, map => map.MapFrom(x => string.Concat(x.NameRus, " / ", x.NameEng)));
                c.CreateMap<SurveyDTO, SurveyVM>().ForMember(m => m.FullName, map => map.MapFrom(x => string.Concat(x.NameRus, " / ", x.NameEng)));
                c.CreateMap<CriterionDTO, CriterionVM>().ForMember(m => m.FullName, map => map.MapFrom(x => string.Concat(x.Order, ". ", x.Name)));
                c.CreateMap<ParameterDTO, ParameterVM>().ForMember(m => m.FullName, map => map.MapFrom(x => string.Concat(x.Criterion.Order, ".", x.Order, " ", x.Name)));
                c.CreateMap<IndicatorDTO, IndicatorVM>().ForMember(m => m.FullName, map => map.MapFrom(x => string.Concat(x.Parameter.Criterion.Order,".",  x.Parameter.Order, ".", x.Order," ", x.Name))).ForMember(m => m.FullNumber, map => map.MapFrom(x => string.Concat(x.Parameter.Criterion.Order, ".", x.Parameter.Order, ".", x.Order))).ForMember(i => i.FullNumber, opt => opt.NullSubstitute("-"));
                c.CreateMap<InvitationDTO, InvitationVM>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            return mapper;
        }

        public static IMapper MapperConfigSettings()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<SettingDTO, SettingVM>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            return mapper;
        }

        public static IMapper MapperConfigNotification()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<NotificationTypeDTO, NotificationTypeVM>();
                c.CreateMap<NotificationDTO, NotificationVM>().ForMember(m => m.EmailText, map => map.MapFrom(x => HelperVm.RemoveHtmlCode(x.EmailText)));

            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            return mapper;
        }

    }
}