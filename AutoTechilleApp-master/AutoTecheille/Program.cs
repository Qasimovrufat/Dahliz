using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoTecheille.Data;
using AutoTecheille.Models;
using AutoTecheille.Models.MyClass;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoTecheille
{
    public class Program
    {
        public static void Main(string[] args)
        {
           IWebHost webHost =  CreateWebHostBuilder(args).Build();

            using(IServiceScope service = webHost.Services.CreateScope())
            {
                using(AutoEntity db = service.ServiceProvider.GetRequiredService<AutoEntity>())
                {

                    //if (!db.Languages.Any())
                    //{
                    //    Language language = new Language()
                    //    {
                    //        Key = "en",
                    //        Value = "1"
                    //    };
                    //    Language language2 = new Language()
                    //    {
                    //        Key = "ru",
                    //        Value = "2"
                    //    };
                    //    Language language3 = new Language()
                    //    {
                    //        Key = "ger",
                    //        Value = "3"
                    //    };

                    //    db.Languages.AddRange(language, language2, language3);
                    //    db.SaveChanges();
                    //}
                    //if (!db.Categories.Any())
                    //{
                    //    var category = new Category()
                    //    {
                    //        Name = "Citroen",
                    //    };

                    //    db.Categories.Add(category);
                    //    db.SaveChanges();
                    //}
                    //if (!db.CategoryLanguages.Any())
                    //{
                    //    var categoryLanguegae = new CategoryLanguage()
                    //    {
                    //        CategoryId = 1,
                    //        LanguageId = 1
                    //    };
                    //    db.CategoryLanguages.Add(categoryLanguegae);
                    //    db.SaveChanges();
                    //}

                    //if (!db.SubCategories.Any())
                    //{
                    //    SubCategory subCategory = new SubCategory()
                    //    {
                    //        Name = "Engine Lubrication",
                    //    };

                    //    db.SubCategories.Add(subCategory);
                    //    db.SaveChanges();
                    //}
                    //if (!db.SubCategoryLanguages.Any())
                    //{
                    //    SubCategoryLanguage subCategoryLanguage = new SubCategoryLanguage()
                    //    {
                    //        SubCategoryId = 1,
                    //        LanguageId = 1
                    //    };
                    //    db.SubCategoryLanguages.Add(subCategoryLanguage);
                    //    db.SaveChanges();
                    //}
                    //if (!db.Products.Any())
                    //{
                    //    Product product = new Product()
                    //    {
                    //        SubCategoryId = 1,
                    //    };


                    //    db.Products.Add(product);
                    //    db.SaveChanges();
                    //}
                    //if (!db.ProductCategories.Any())
                    //{
                    //    ProductCategory pr = new ProductCategory()
                    //    {
                    //        CategoryId = 1,
                    //        ProductId = 1
                    //    };

                    //    db.ProductCategories.Add(pr);
                    //    db.SaveChanges();
                    //}
                    //if (!db.ProductLanguages.Any())
                    //{
                    //    ProductLanguage productLanguage = new ProductLanguage()
                    //    {
                    //        ProductId = 1,
                    //        LanguageId = 1
                    //    };
                    //    db.ProductLanguages.Add(productLanguage);
                    //}
                    //if (!db.RealPartNos.Any())
                    //{
                    //    RealPartNo realPartNo = new RealPartNo()
                    //    {
                    //        Name = "000 018 07 02",
                    //        ProductId = 1
                    //    };
                    //    db.RealPartNos.Add(realPartNo);
                    //    db.SaveChanges();
                    //}


                    AdminCreater.CreatAsync(service, db).Wait();
                }
            }
            webHost.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
