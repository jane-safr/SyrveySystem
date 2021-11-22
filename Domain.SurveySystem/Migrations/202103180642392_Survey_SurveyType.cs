namespace Domain.SurveySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Survey_SurveyType : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Survey.Surveys",
                c => new
                    {
                        SurveyId = c.Guid(nullable: false),
                        SurveyCode = c.Int(nullable: false),
                        SurveyTypeId = c.Guid(nullable: false),
                        NameRus = c.String(nullable: false, maxLength: 1000),
                        NameEng = c.String(nullable: false, maxLength: 1000),
                        PurposeRus = c.String(nullable: false, maxLength: 2000),
                        PurposeEng = c.String(nullable: false, maxLength: 2000),
                        TimeEstimateMin = c.Int(nullable: false),
                        IsRandomQuestions = c.Boolean(nullable: false),
                        IsAnonymous = c.Boolean(nullable: false),
                        WithRecomenadation = c.Boolean(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SurveyId)
                .ForeignKey("Survey.SurveyTypes", t => t.SurveyTypeId, cascadeDelete: true)
                .Index(t => t.SurveyCode)
                .Index(t => t.SurveyTypeId);
            
            CreateTable(
                "Survey.SurveyTypes",
                c => new
                    {
                        SurveyTypeId = c.Guid(nullable: false),
                        NameRus = c.String(nullable: false, maxLength: 1000),
                        NameEng = c.String(nullable: false, maxLength: 1000),
                        DescriptionRus = c.String(nullable: false, maxLength: 1000),
                        DescriptionEng = c.String(nullable: false, maxLength: 1000),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SurveyTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("Survey.Surveys", "SurveyTypeId", "Survey.SurveyTypes");
            DropIndex("Survey.Surveys", new[] { "SurveyTypeId" });
            DropIndex("Survey.Surveys", new[] { "SurveyCode" });
            DropTable("Survey.SurveyTypes");
            DropTable("Survey.Surveys");
        }
    }
}
