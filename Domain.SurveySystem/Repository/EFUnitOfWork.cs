using System;
using System.Threading.Tasks;
using Domain.SurveySystem.Context;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Identity;
using Domain.SurveySystem.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Domain.SurveySystem.Repository
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private SurveySystemContext db;

        private InstructionRepository instructionRepository;
        private UploadedFileRepository uploadedFileRepository;
        private UserAnswerRepository userAnswerRepository;
        private AnswerRepository answerRepository;
        private FixedAnswerRepository fixedAnswerRepository;
        private QuestionRepository questionRepository;
        private QuestionTypeRepository questionTypeRepository;
        private NotificationTypeRepository notificationTypeRepository;
        private NotificationRepository notificationRepository;
        private InvitationRepository invitationRepository;
        private IndicatorRepository indicatorRepository;
        private ParameterRepository parameterRepository;
        private CriterionRepository criterionRepository;
        private SurveyTypeRepository surveyTypeRepository;
        private SurveyRepository surveyRepository;
        private SettingRepository settingRepository;

        private ApplicationUserManager userManager;
        private ApplicationRoleManager roleManager;

        public EFUnitOfWork(string connectionString)
        {
            db = new SurveySystemContext(connectionString);
            userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
            roleManager = new ApplicationRoleManager(new RoleStore<ApplicationRole>(db));
        }

        public IRepository<Instruction, Guid> Instructions
        {
            get
            {
                if (instructionRepository == null)
                    instructionRepository = new InstructionRepository(db);
                return instructionRepository;
            }
        }
        public IRepository<UploadedFile, Guid> UploadedFiles
        {
            get
            {
                if (uploadedFileRepository == null)
                    uploadedFileRepository = new UploadedFileRepository(db);
                return uploadedFileRepository;
            }
        }
        public IRepository<UserAnswer, Guid> UserAnswers
        {
            get
            {
                if (userAnswerRepository == null)
                    userAnswerRepository = new UserAnswerRepository(db);
                return userAnswerRepository;
            }
        }
       public IRepository<FixedAnswer, Guid> FixedAnswers
        {
            get
            {
                if (fixedAnswerRepository == null)
                    fixedAnswerRepository = new FixedAnswerRepository(db);
                return fixedAnswerRepository;
            }
        }
        public IRepository<Answer, Guid> Answers
        {
            get
            {
                if (answerRepository == null)
                    answerRepository = new AnswerRepository(db);
                return answerRepository;
            }
        }
        public IRepository<QuestionType, Guid> QuestionTypes
        {
            get
            {
                if (questionTypeRepository == null)
                    questionTypeRepository = new QuestionTypeRepository(db);
                return questionTypeRepository;
            }
        }
        public IRepository<Question, Guid> Questions
        {
            get
            {
                if (questionRepository == null)
                    questionRepository = new QuestionRepository(db);
                return questionRepository;
            }
        }

        public IRepository<NotificationType, Guid> NotificationTypes
        {
            get
            {
                if (notificationTypeRepository == null)
                    notificationTypeRepository = new NotificationTypeRepository(db);
                return notificationTypeRepository;
            }
        }

        public IRepository<Notification, Guid> Notifications
        {
            get
            {
                if (notificationRepository == null)
                    notificationRepository = new NotificationRepository(db);
                return notificationRepository;
            }
        }

        public IRepository<Invitation, Guid> Invitations
        {
            get
            {
                if (invitationRepository == null)
                    invitationRepository = new InvitationRepository(db);
                return invitationRepository;
            }
        }
        public IRepository<Indicator, Guid> Indicators
        {
            get
            {
                if (indicatorRepository == null)
                    indicatorRepository = new IndicatorRepository(db);
                return indicatorRepository;
            }
        }
        public IRepository<Parameter, Guid> Parameters
        {
            get
            {
                if (parameterRepository == null)
                    parameterRepository = new ParameterRepository(db);
                return parameterRepository;
            }
        }

        public IRepository<Criterion, Guid> Criterions
        {
            get
            {
                if (criterionRepository == null)
                    criterionRepository = new CriterionRepository(db);
                return criterionRepository;
            }
        }

        public IRepository<SurveyType, Guid> SurveyTypes
        {
            get
            {
                if (surveyTypeRepository == null)
                    surveyTypeRepository = new SurveyTypeRepository(db);
                return surveyTypeRepository;
            }
        }
        public IRepository<Survey, Guid> Surveys
        {
            get
            {
                if (surveyRepository == null)
                    surveyRepository = new SurveyRepository(db);
                return surveyRepository;
            }
        }

        public IRepository<Setting, Guid> Settings
        {
            get
            {
                if (settingRepository == null)
                    settingRepository = new SettingRepository(db);
                return settingRepository;
            }
        }

        public ApplicationUserManager UserManager => userManager;
        public ApplicationRoleManager RoleManager => roleManager;

        public async Task<int> Save()
        {
            return await db.SaveChangesAsync();
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                    userManager.Dispose();
                    roleManager.Dispose();
                }
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}