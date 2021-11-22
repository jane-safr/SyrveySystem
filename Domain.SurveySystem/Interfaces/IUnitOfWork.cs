using System;
using System.Threading.Tasks;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Identity;

namespace Domain.SurveySystem.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<UploadedFile, Guid> UploadedFiles { get; }
        IRepository<Instruction, Guid> Instructions { get; }
        IRepository<UserAnswer, Guid> UserAnswers { get; }
        IRepository<Answer, Guid> Answers { get; }
        IRepository<FixedAnswer, Guid> FixedAnswers { get; }
        IRepository<QuestionType, Guid> QuestionTypes { get; }
        IRepository<Question, Guid> Questions { get; }
        IRepository<Notification, Guid> Notifications { get; }
        IRepository<NotificationType, Guid> NotificationTypes { get; }
        IRepository<Invitation, Guid> Invitations { get; }
        IRepository<Indicator, Guid> Indicators { get; }
        IRepository<Parameter, Guid> Parameters { get; }
        IRepository<Criterion, Guid> Criterions { get; }
        IRepository<SurveyType, Guid> SurveyTypes { get; }
        IRepository<Survey, Guid> Surveys { get; }
        IRepository<Setting, Guid> Settings { get; }
        ApplicationUserManager UserManager { get; }
        ApplicationRoleManager RoleManager { get; }
        Task<int> Save();
    }
}
