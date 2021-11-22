namespace Domain.SurveySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpInstructions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Resource.Instructions",
                c => new
                    {
                        InstructionId = c.Guid(nullable: false),
                        NameRus = c.String(nullable: false, maxLength: 400),
                        NameEng = c.String(nullable: false, maxLength: 400),
                        UploadFileRusId = c.Guid(),
                        UploadFileEngId = c.Guid(),
                        IsAdmin = c.Boolean(nullable: false),
                        Code = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.InstructionId)
                .Index(t => t.UploadFileRusId)
                .Index(t => t.UploadFileEngId)
                .Index(t => t.Code);
            
            CreateTable(
                "Resource.UploadedFiles",
                c => new
                    {
                        UploadedFileId = c.Guid(nullable: false),
                        ApplicationUserId = c.Guid(nullable: false),
                        PathFile = c.String(nullable: false, maxLength: 3000),
                        FileName = c.String(maxLength: 1000),
                        FileSize = c.Int(nullable: false),
                        FileType = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UploadedFileId)
                .Index(t => t.ApplicationUserId);
            
            DropColumn("Survey.Surveys", "WithRecomenadation");
        }
        
        public override void Down()
        {
            AddColumn("Survey.Surveys", "WithRecomenadation", c => c.Boolean(nullable: false));
            DropIndex("Resource.UploadedFiles", new[] { "ApplicationUserId" });
            DropIndex("Resource.Instructions", new[] { "Code" });
            DropIndex("Resource.Instructions", new[] { "UploadFileEngId" });
            DropIndex("Resource.Instructions", new[] { "UploadFileRusId" });
            DropTable("Resource.UploadedFiles");
            DropTable("Resource.Instructions");
        }
    }
}
