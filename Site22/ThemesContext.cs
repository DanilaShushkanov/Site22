namespace Site22
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ThemesContext : DbContext
    {
        public ThemesContext()
            : base("name=ThemesContext")
        {
        }

        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<Scientific_works> Scientific_works { get; set; }
        public virtual DbSet<Them> Thems { get; set; }
        public virtual DbSet<Subjects> Subjects { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Position)
                .IsUnicode(false);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Academic_degree)
                .IsUnicode(false);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Scientific_works)
                .WithOptional(e => e.Employee)
                .HasForeignKey(e => e.ID_employee);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Thems)
                .WithOptional(e => e.Employee)
                .HasForeignKey(e => e.ID_Employee);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Subjects)
                .WithOptional(e => e.Employee)
                .HasForeignKey(e => e.ID_employee);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.News)
                .WithOptional(e => e.Employee)
                .HasForeignKey(e => e.ID_Author);

            modelBuilder.Entity<News>()
                .Property(e => e.Text)
                .IsUnicode(false);

            modelBuilder.Entity<Scientific_works>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Them>()
                .Property(e => e.Name)
                .IsUnicode(false);
        }
    }
}
