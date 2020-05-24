namespace Site22.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fixing : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Subjects", "ID_employee");
            RenameColumn(table: "dbo.Subjects", name: "Employee_ID", newName: "ID_employee");
            RenameIndex(table: "dbo.Subjects", name: "IX_Employee_ID", newName: "IX_ID_employee");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Subjects", name: "IX_ID_employee", newName: "IX_Employee_ID");
            RenameColumn(table: "dbo.Subjects", name: "ID_employee", newName: "Employee_ID");
            AddColumn("dbo.Subjects", "ID_employee", c => c.Int());
        }
    }
}
