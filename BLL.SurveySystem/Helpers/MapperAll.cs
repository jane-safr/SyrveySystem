using AutoMapper;
using BLL.SurveySystem.DTO;
using Domain.SurveySystem.Entity;

namespace BLL.SurveySystem.Helpers
{
    public class MapperAll
    {
        public static IMapper MapperConfigDto()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<ApplicationRole, ApplicationRoleDTO>();
                c.CreateMap<ApplicationUser, ApplicationUserDTO>();
                c.CreateMap<Parameter, ParameterDTO>();
                c.CreateMap<Indicator, IndicatorDTO>();
                c.CreateMap<Criterion, CriterionDTO>();

                c.CreateMap<UserAnswer, UserAnswerDTO>();
                c.CreateMap<Invitation, InvitationDTO>();
                c.CreateMap<Survey, SurveyDTO>();
                c.CreateMap<SurveyType, SurveyTypeDTO>();
                c.CreateMap<Question, QuestionDTO>();
                c.CreateMap<QuestionType, QuestionTypeDTO>();
                c.CreateMap<Answer, AnswerDTO>();
                c.CreateMap<FixedAnswer, FixedAnswerDTO>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            return mapper;
        }
        public static IMapper MapperConfigSettings()
        {
            var config = new MapperConfiguration(c => { c.CreateMap<Setting, SettingDTO>(); });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            return mapper;
        }

        public static IMapper MapperConfigNotification()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<NotificationType, NotificationTypeDTO>();
                c.CreateMap<Notification, NotificationDTO>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            return mapper;
        }
    }
}