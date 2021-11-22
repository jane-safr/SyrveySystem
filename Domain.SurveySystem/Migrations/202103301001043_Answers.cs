namespace Domain.SurveySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Answers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "QA.Answers",
                c => new
                    {
                        AnswerId = c.Guid(nullable: false),
                        AnswerRus = c.String(nullable: false, maxLength: 3000),
                        AnswerEng = c.String(nullable: false, maxLength: 3000),
                        IsValid = c.Boolean(nullable: false),
                        Credit = c.Int(nullable: false),
                        QuestionId = c.Guid(nullable: false),
                        Comment = c.String(),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.AnswerId)
                .ForeignKey("QA.Questions", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "QA.FixedAnswers",
                c => new
                    {
                        FixedAnswerId = c.Guid(nullable: false),
                        FixAnswerRus = c.String(nullable: false, maxLength: 3000),
                        FixAnswerEng = c.String(nullable: false, maxLength: 3000),
                        Credit = c.Int(nullable: false),
                        QuestionTypeId = c.Guid(nullable: false),
                        Comment = c.String(),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.FixedAnswerId)
                .ForeignKey("QA.QuestionTypes", t => t.QuestionTypeId, cascadeDelete: true)
                .Index(t => t.QuestionTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("QA.FixedAnswers", "QuestionTypeId", "QA.QuestionTypes");
            DropForeignKey("QA.Answers", "QuestionId", "QA.Questions");
            DropIndex("QA.FixedAnswers", new[] { "QuestionTypeId" });
            DropIndex("QA.Answers", new[] { "QuestionId" });
            DropTable("QA.FixedAnswers");
            DropTable("QA.Answers");
        }
    }
}
