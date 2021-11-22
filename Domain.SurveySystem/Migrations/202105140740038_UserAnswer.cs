namespace Domain.SurveySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserAnswer : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Respondent.UserAnswers",
                c => new
                    {
                        UserAnswerId = c.Guid(nullable: false),
                        InvitationId = c.Guid(nullable: false),
                        QuestionId = c.Guid(nullable: false),
                        AnswerId = c.Guid(),
                        IsValid = c.Boolean(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UserAnswerId)
                .ForeignKey("Survey.Invitations", t => t.InvitationId, cascadeDelete: true)
                .Index(t => t.InvitationId)
                .Index(t => t.QuestionId);
            
            AddColumn("QA.QuestionTypes", "IsOpenAnswer", c => c.Boolean(nullable: false));
            AddColumn("Survey.Invitations", "Percent", c => c.Int(nullable: false));
            AddColumn("Survey.Invitations", "DateStart", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("Survey.Invitations", "LastQuestionId", c => c.Guid());
        }
        
        public override void Down()
        {
            DropForeignKey("Respondent.UserAnswers", "InvitationId", "Survey.Invitations");
            DropIndex("Respondent.UserAnswers", new[] { "QuestionId" });
            DropIndex("Respondent.UserAnswers", new[] { "InvitationId" });
            DropColumn("Survey.Invitations", "LastQuestionId");
            DropColumn("Survey.Invitations", "DateStart");
            DropColumn("Survey.Invitations", "Percent");
            DropColumn("QA.QuestionTypes", "IsOpenAnswer");
            DropTable("Respondent.UserAnswers");
        }
    }
}
