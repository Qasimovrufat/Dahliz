using AutoTecheille.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Data
{
    public class AutoEntity:IdentityDbContext<User>
    {
        public AutoEntity(DbContextOptions<AutoEntity> dbContextOptions) :base(dbContextOptions){ }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductCategory> ProductCategories { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<SubCategory> SubCategories { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<ProductLanguage> ProductLanguages { get; set; }
        public virtual DbSet<CategoryLanguage> CategoryLanguages { get; set; }
        public virtual DbSet<SubCategoryLanguage> SubCategoryLanguages { get; set; }
        public virtual DbSet<About> Abouts { get; set; }
        public virtual DbSet<AboutLanguage> AboutLanguages { get; set; }
        public virtual DbSet<RealPartNo> RealPartNos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //    builder.Entity<ProductCategory>().HasKey(pr => new { pr.CategoryId, pr.ProductId });
            //    builder.Entity<ProductCategory>()
            //         .HasOne(pr => pr.Product)
            //         .WithMany(p => p.ProductCategories)
            //         .HasForeignKey(pr => pr.ProductId);

            //    builder.Entity<ProductCategory>()
            //        .HasOne(pr => pr.Category)
            //        .WithMany(c => c.ProductCategories)
            //        .HasForeignKey(pr => pr.CategoryId);

            builder.Entity<Category>().HasMany(pr => pr.ProductCategories).WithOne(pr => pr.Category);
            base.OnModelCreating(builder);
        }
    }


    

    //public class AutoEntityFactory : IDesignTimeDbContextFactory<AutoEntity>
    //{
    //    AutoEntity IDesignTimeDbContextFactory<AutoEntity>.CreateDbContext(string[] args)
    //    {
    //        var optionsBuilder = new DbContextOptionsBuilder<AutoEntity>();
    //        optionsBuilder.UseSqlServer<AutoEntity>("Data Source =DESKTOP-C7AK884\\ACERSQL ;Initial Catalog = AutoEntity;Integrated Security = SSPI;");

    //        return new AutoEntity(optionsBuilder.Options);
    //    }
    //}
}
