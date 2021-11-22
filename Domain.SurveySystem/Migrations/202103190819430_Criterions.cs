namespace Domain.SurveySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Criterions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Criterion.Criterions",
                c => new
                    {
                        CriterionId = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 2000),
                        Order = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CriterionId);
            
            CreateTable(
                "Criterion.Parameters",
                c => new
                    {
                        ParameterId = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 2000),
                        Order = c.Int(nullable: false),
                        CriterionId = c.Guid(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ParameterId)
                .ForeignKey("Criterion.Criterions", t => t.CriterionId, cascadeDelete: true)
                .Index(t => t.CriterionId);
            
            CreateTable(
                "Criterion.Indicators",
                c => new
                    {
                        IndicatorId = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 3000),
                        Order = c.Int(nullable: false),
                        ParameterId = c.Guid(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IndicatorId)
                .ForeignKey("Criterion.Parameters", t => t.ParameterId, cascadeDelete: true)
                .Index(t => t.ParameterId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("Criterion.Parameters", "CriterionId", "Criterion.Criterions");
            DropForeignKey("Criterion.Indicators", "ParameterId", "Criterion.Parameters");
            DropIndex("Criterion.Indicators", new[] { "ParameterId" });
            DropIndex("Criterion.Parameters", new[] { "CriterionId" });
            DropTable("Criterion.Indicators");
            DropTable("Criterion.Parameters");
            DropTable("Criterion.Criterions");
        }
    }
}
