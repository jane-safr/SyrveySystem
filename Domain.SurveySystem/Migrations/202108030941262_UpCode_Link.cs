namespace Domain.SurveySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpCode_Link : DbMigration
    {
        public override void Up()
        {
            AddColumn("Survey.Surveys", "ShortLink", c => c.String(maxLength: 500));
            AddColumn("Survey.Invitations", "InvitationCode", c => c.Int(nullable: false));
            CreateIndex("Survey.Invitations", "InvitationCode");
            DropColumn("Survey.Invitations", "LastQuestionId");
        }
        
        public override void Down()
        {
            AddColumn("Survey.Invitations", "LastQuestionId", c => c.Guid());
            DropIndex("Survey.Invitations", new[] { "InvitationCode" });
            DropColumn("Survey.Invitations", "InvitationCode");
            DropColumn("Survey.Surveys", "ShortLink");
        }
    }
}
