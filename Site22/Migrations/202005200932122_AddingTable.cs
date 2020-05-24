namespace Site22.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Subjects",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        Coef = c.Int(),
                        ID_employee = c.Int(),
                        Name = c.String(maxLength: 50),
                        Employee_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Employees", t => t.Employee_ID)
                .Index(t => t.Employee_ID);
            
            DropColumn("dbo.Employees", "WorkingTime123");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Employees", "WorkingTime123", c => c.Int());
            DropForeignKey("dbo.Subjects", "Employee_ID", "dbo.Employees");
            DropIndex("dbo.Subjects", new[] { "Employee_ID" });
            DropTable("dbo.Subjects");
        }
    }
}
