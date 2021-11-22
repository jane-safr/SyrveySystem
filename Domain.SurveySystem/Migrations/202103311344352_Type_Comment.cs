namespace Domain.SurveySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Type_Comment : DbMigration
    {
        public override void Up()
        {
            AddColumn("QA.QuestionTypes", "Comment", c => c.String());
            DropColumn("QA.Answers", "Comment");
            DropColumn("QA.FixedAnswers", "Comment");
        }
        
        public override void Down()
        {
            AddColumn("QA.FixedAnswers", "Comment", c => c.String());
            AddColumn("QA.Answers", "Comment", c => c.String());
            DropColumn("QA.QuestionTypes", "Comment");
        }
    }
}
