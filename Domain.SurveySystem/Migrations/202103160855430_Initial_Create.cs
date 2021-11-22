namespace Domain.SurveySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial_Create : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Identity.IdentityRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false),
                        Description = c.String(maxLength: 1000),
                        IsShown = c.Boolean(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "Identity.IdentityUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("Identity.IdentityRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("Identity.ApplicationUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "Setting.Settings",
                c => new
                    {
                        SettingId = c.Guid(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 400),
                        Value = c.String(nullable: false, maxLength: 400),
                        Description = c.String(nullable: false, maxLength: 1000),
                    })
                .PrimaryKey(t => t.SettingId)
                .Index(t => new { t.Name, t.Value }, unique: true);
            
            CreateTable(
                "Identity.ApplicationUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        IsExternal = c.Boolean(nullable: false),
                        DisplayName = c.String(),
                        Email = c.String(nullable: false),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "Identity.IdentityUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("Identity.ApplicationUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "Identity.IdentityUserLogins",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.LoginProvider, t.ProviderKey })
                .ForeignKey("Identity.ApplicationUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("Identity.IdentityUserRoles", "UserId", "Identity.ApplicationUsers");
            DropForeignKey("Identity.IdentityUserLogins", "UserId", "Identity.ApplicationUsers");
            DropForeignKey("Identity.IdentityUserClaims", "UserId", "Identity.ApplicationUsers");
            DropForeignKey("Identity.IdentityUserRoles", "RoleId", "Identity.IdentityRoles");
            DropIndex("Identity.IdentityUserLogins", new[] { "UserId" });
            DropIndex("Identity.IdentityUserClaims", new[] { "UserId" });
            DropIndex("Setting.Settings", new[] { "Name", "Value" });
            DropIndex("Identity.IdentityUserRoles", new[] { "RoleId" });
            DropIndex("Identity.IdentityUserRoles", new[] { "UserId" });
            DropTable("Identity.IdentityUserLogins");
            DropTable("Identity.IdentityUserClaims");
            DropTable("Identity.ApplicationUsers");
            DropTable("Setting.Settings");
            DropTable("Identity.IdentityUserRoles");
            DropTable("Identity.IdentityRoles");
        }
    }
}
