 using System.Data.Entity;
 using System.Data.Entity.Infrastructure;
 using Domain.SurveySystem.Entity;
 using Microsoft.AspNet.Identity.EntityFramework;

namespace Domain.SurveySystem.Context
{
    public class SurveySystemContext : IdentityDbContext<ApplicationUser>
    {
        public SurveySystemContext(string connectionString) : base(connectionString)
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = true;
        }

#if DEBUG
       public class MigrationsContextFactory : IDbContextFactory<SurveySystemContext> // MIGRATIONS
        {
            public SurveySystemContext Create()
            {
                return new SurveySystemContext("SurveySystemConnection");
            }
        }
#endif
        // Intructions + UploadedFiles
        public DbSet<Instruction> Instructions { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        // UserAnswers
        public DbSet<UserAnswer> UserAnswers { get; set; }
        
        // Questions + Types + Answers
        public DbSet<Answer> Answers { get; set; }
        public DbSet<FixedAnswer> FixedAnswers { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionType> QuestionTypes { get; set; }

        // NotificationType + Notification
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }

        // Invitations
        public DbSet<Invitation> Invitations { get; set; }

        // Criterions
        public DbSet<Criterion> Criterions { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<Indicator> Indicators { get; set; }

        // Surveys
        public DbSet<SurveyType> SurveyTypes { get; set; }
        public DbSet<Survey> Surveys { get; set; }

        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //User
            var user = modelBuilder.Entity<ApplicationUser>().HasKey(l => l.Id).ToTable("ApplicationUsers", "Identity");
            user.HasMany(u => u.Roles).WithRequired().HasForeignKey(ur => ur.UserId);
            user.HasMany(u => u.Claims).WithRequired().HasForeignKey(uc => uc.UserId);
            user.HasMany(u => u.Logins).WithRequired().HasForeignKey(ul => ul.UserId);
            user.Property(u => u.UserName).IsRequired();
            user.Property(u => u.Email).IsRequired();
            //UserRoles
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new {r.UserId, r.RoleId})
                .ToTable("IdentityUserRoles", "Identity");
            //UserLogin
            modelBuilder.Entity<IdentityUserLogin>().HasKey(l => new {l.UserId, l.LoginProvider, l.ProviderKey})
                .ToTable("IdentityUserLogins", "Identity");
            //UserClaim
            modelBuilder.Entity<IdentityUserClaim>().ToTable("IdentityUserClaims", "Identity");
            //UserRole
            var role = modelBuilder.Entity<IdentityRole>().ToTable("IdentityRoles", "Identity");
            role.Property(r => r.Name).IsRequired();
            role.HasMany(r => r.Users).WithRequired().HasForeignKey(ur => ur.RoleId);

            // Settings - (Name, Value) nonclustered index
            modelBuilder.Entity<Setting>()
                .HasIndex(p => new { p.Name, p.Value })
                .IsUnique();

            // Survey - SurveyType: oo - 1
            modelBuilder.Entity<SurveyType>()
                .HasMany(r => r.Surveys)
                .WithRequired(p => p.SurveyType)
                .HasForeignKey(fk => fk.SurveyTypeId);

            // Criterion - Parameter: 1 - oo
            modelBuilder.Entity<Criterion>()
                .HasMany(r => r.Parameters)
                .WithRequired(p => p.Criterion)
                .HasForeignKey(fk => fk.CriterionId);

            // Parameter - Indicator: 1 - oo
            modelBuilder.Entity<Parameter>()
                .HasMany(r => r.Indicators)
                .WithRequired(p => p.Parameter)
                .HasForeignKey(fk => fk.ParameterId);

            // Survey - Invitation: 1 - oo
            modelBuilder.Entity<Survey>()
                .HasMany(r => r.Invitations)
                .WithRequired(p => p.Survey)
                .HasForeignKey(fk => fk.SurveyId);

            // NotificationType - Notification: 1 - oo
            modelBuilder.Entity<NotificationType>()
                .HasMany(r => r.Notifications)
                .WithRequired(p => p.NotificationType)
                .HasForeignKey(fk => fk.NotificationTypeId);

            // Survey - Question: 1 - oo
            modelBuilder.Entity<Survey>()
                .HasMany(r => r.Questions)
                .WithRequired(p => p.Survey)
                .HasForeignKey(fk => fk.SurveyId);

            // QuestionType - Question: 1 - oo
            modelBuilder.Entity<QuestionType>()
                .HasMany(r => r.Questions)
                .WithRequired(p => p.QuestionType)
                .HasForeignKey(fk => fk.QuestionTypeId);

            // Question - Answer: 1 - oo
            modelBuilder.Entity<Question>()
                .HasMany(r => r.Answers)
                .WithRequired(p => p.Question)
                .HasForeignKey(fk => fk.QuestionId);

            // QuestionType - FixedAnswer: 1 - oo
            modelBuilder.Entity<QuestionType>()
                .HasMany(r => r.FixedAnswers)
                .WithRequired(p => p.QuestionType)
                .HasForeignKey(fk => fk.QuestionTypeId);

            // Invitation - UserAnswer: 1 - oo
            modelBuilder.Entity<Invitation>()
                .HasMany(r => r.UserAnswers)
                .WithRequired(p => p.Invitation)
                .HasForeignKey(fk => fk.InvitationId);
        }
    }
}