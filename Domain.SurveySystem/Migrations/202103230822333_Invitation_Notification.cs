namespace Domain.SurveySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Invitation_Notification : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Survey.Invitations",
                c => new
                    {
                        InvitationId = c.Guid(nullable: false),
                        UserId = c.Guid(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 500),
                        UserEmail = c.String(nullable: false, maxLength: 300),
                        DateEnd = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        IsAccepted = c.Boolean(nullable: false),
                        IsFinished = c.Boolean(nullable: false),
                        SurveyId = c.Guid(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.InvitationId)
                .ForeignKey("Survey.Surveys", t => t.SurveyId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.SurveyId);
            
            CreateTable(
                "Setting.Notifications",
                c => new
                    {
                        NotificationId = c.Guid(nullable: false),
                        EmailTo = c.String(nullable: false),
                        EmailText = c.String(nullable: false),
                        EmailUrl = c.String(maxLength: 1500),
                        DateSend = c.DateTime(precision: 7, storeType: "datetime2"),
                        IsSend = c.Boolean(nullable: false),
                        Id = c.Guid(nullable: false),
                        NotificationTypeId = c.Guid(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("Setting.NotificationTypes", t => t.NotificationTypeId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.NotificationTypeId);
            
            CreateTable(
                "Setting.NotificationTypes",
                c => new
                    {
                        NotificationTypeId = c.Guid(nullable: false),
                        NameRus = c.String(nullable: false, maxLength: 2000),
                        NameEng = c.String(nullable: false, maxLength: 2000),
                        MessageTemplate = c.String(nullable: false),
                        TemplateLink = c.String(maxLength: 500),
                        IsSurvey = c.Boolean(nullable: false),
                        IsAnonymousSurvey = c.Boolean(nullable: false),
                        IsTest = c.Boolean(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.NotificationTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("Setting.Notifications", "NotificationTypeId", "Setting.NotificationTypes");
            DropForeignKey("Survey.Invitations", "SurveyId", "Survey.Surveys");
            DropIndex("Setting.Notifications", new[] { "NotificationTypeId" });
            DropIndex("Setting.Notifications", new[] { "Id" });
            DropIndex("Survey.Invitations", new[] { "SurveyId" });
            DropIndex("Survey.Invitations", new[] { "UserId" });
            DropTable("Setting.NotificationTypes");
            DropTable("Setting.Notifications");
            DropTable("Survey.Invitations");
        }
    }
}
