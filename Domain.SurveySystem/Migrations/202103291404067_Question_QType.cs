namespace Domain.SurveySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Question_QType : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "QA.Questions",
                c => new
                    {
                        QuestionId = c.Guid(nullable: false),
                        QuestionRus = c.String(nullable: false, maxLength: 3000),
                        QuestionEng = c.String(nullable: false, maxLength: 3000),
                        IsCriterion = c.Boolean(nullable: false),
                        IndicatorId = c.Guid(),
                        IsInReport = c.Boolean(nullable: false),
                        SurveyId = c.Guid(nullable: false),
                        QuestionTypeId = c.Guid(nullable: false),
                        Group = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.QuestionId)
                .ForeignKey("Criterion.Indicators", t => t.IndicatorId)
                .ForeignKey("QA.QuestionTypes", t => t.QuestionTypeId, cascadeDelete: true)
                .ForeignKey("Survey.Surveys", t => t.SurveyId, cascadeDelete: true)
                .Index(t => t.IndicatorId)
                .Index(t => t.SurveyId)
                .Index(t => t.QuestionTypeId);
            
            CreateTable(
                "QA.QuestionTypes",
                c => new
                    {
                        QuestionTypeId = c.Guid(nullable: false),
                        TypeName = c.String(nullable: false, maxLength: 2000),
                        IsFixedAnswer = c.Boolean(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.QuestionTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("QA.Questions", "SurveyId", "Survey.Surveys");
            DropForeignKey("QA.Questions", "QuestionTypeId", "QA.QuestionTypes");
            DropForeignKey("QA.Questions", "IndicatorId", "Criterion.Indicators");
            DropIndex("QA.Questions", new[] { "QuestionTypeId" });
            DropIndex("QA.Questions", new[] { "SurveyId" });
            DropIndex("QA.Questions", new[] { "IndicatorId" });
            DropTable("QA.QuestionTypes");
            DropTable("QA.Questions");
        }
    }
}
