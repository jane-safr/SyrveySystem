namespace Domain.SurveySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Additional_UserAnswer : DbMigration
    {
        public override void Up()
        {
            AddColumn("Survey.Invitations", "ActualCompleteDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("Respondent.UserAnswers", "Order", c => c.Int(nullable: false));
            AddColumn("Respondent.UserAnswers", "UserAnswerText", c => c.String(maxLength: 1500));
        }
        
        public override void Down()
        {
            DropColumn("Respondent.UserAnswers", "UserAnswerText");
            DropColumn("Respondent.UserAnswers", "Order");
            DropColumn("Survey.Invitations", "ActualCompleteDate");
        }
    }
}
