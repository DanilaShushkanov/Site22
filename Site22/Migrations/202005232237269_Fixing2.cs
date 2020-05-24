namespace Site22.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fixing2 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Subjects");
            AlterColumn("dbo.Subjects", "ID", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Subjects", "ID");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Subjects");
            AlterColumn("dbo.Subjects", "ID", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Subjects", "ID");
        }
    }
}
