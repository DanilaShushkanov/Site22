namespace Site22.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Adding123 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Subjects", "Coef", c => c.Single());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Subjects", "Coef", c => c.Int());
        }
    }
}
