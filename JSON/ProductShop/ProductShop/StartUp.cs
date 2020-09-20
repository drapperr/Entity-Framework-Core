using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();
            var usersJson = File.ReadAllText("../../../Datasets/users.json");
            var productsJson = File.ReadAllText("../../../Datasets/products.json");
            var categoriesJson = File.ReadAllText("../../../Datasets/categories.json");
            var categoriesProductsJson = File.ReadAllText("../../../Datasets/categories-products.json");

            //ResetDatabase(context);

            //Import
            //Console.WriteLine(ImportUsers(context, usersJson));
            //Console.WriteLine(ImportProducts(context, productsJson));
            //Console.WriteLine(ImportCategories(context, categoriesJson));
            //Console.WriteLine(ImportCategoryProducts(context, categoriesProductsJson));

            //Export

            //Console.WriteLine(GetProductsInRange(context));
            //Console.WriteLine(GetSoldProducts(context));
            //Console.WriteLine(GetCategoriesByProductsCount(context));
            Console.WriteLine(GetUsersWithProducts(context));
        }

        private static void ResetDatabase(ProductShopContext db)
        {
            db.Database.EnsureDeleted();
            Console.WriteLine("Databese was successfully deleted!");
            db.Database.EnsureCreated();
            Console.WriteLine("Databese was successfully created!");
        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var users = JsonConvert.DeserializeObject<User[]>(inputJson);

            context.Users.AddRange(users);

            var count = context.SaveChanges();

            return $"Successfully imported {count}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<Product[]>(inputJson);

            context.Products.AddRange(products);
            var count = context.SaveChanges();

            return $"Successfully imported {count}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var categories = JsonConvert.DeserializeObject<Category[]>(inputJson)
                .Where(x => x.Name != null);

            context.Categories.AddRange(categories);
            var count = context.SaveChanges();

            return $"Successfully imported {count}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var categoryProducts = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);

            context.CategoryProducts.AddRange(categoryProducts);
            var count = context.SaveChanges();

            return $"Successfully imported {count}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .OrderBy(x => x.Price)
                .Select(x => new
                {
                    x.Name,
                    x.Price,
                    Seller = x.Seller.FirstName + " " + x.Seller.LastName
                });

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            };

            return JsonConvert.SerializeObject(products, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var products = context.Users
                .Where(x => x.ProductsSold.Count > 0 && x.ProductsSold.Any(y => y.Buyer != null))
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    SoldProducts = x.ProductsSold
                    .Where(y => y.Buyer != null)
                    .Select(y => new
                    {
                        y.Name,
                        y.Price,
                        BuyerFirstName = y.Buyer.FirstName,
                        BuyerLastName = y.Buyer.LastName
                    })
                });

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            return JsonConvert.SerializeObject(products, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(x => new
                {
                    Category = x.Name,
                    ProductsCount = x.CategoryProducts.Count(),
                    AveragePrice = $"{x.CategoryProducts.Average(y => y.Product.Price):F2}",
                    TotalRevenue = $"{x.CategoryProducts.Sum(y => y.Product.Price):F2}"
                })
                .OrderByDescending(x=>x.ProductsCount);

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            };

            return JsonConvert.SerializeObject(categories, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var usersWithProducts = context.Users
                .AsEnumerable()
                .Where(u => u.ProductsSold.Any(p => p.Buyer != null))
                .OrderByDescending(x => x.ProductsSold.Where(p => p.Buyer != null).Count())
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.Age,
                    SoldProducts = new
                    {
                        Count = x.ProductsSold.Where(p => p.Buyer != null).Count(),
                        Products = x.ProductsSold.Where(p => p.Buyer != null).Select(y => new
                        {
                            y.Name,
                            y.Price
                        })
                    }
                }).ToList();

            var result = new
            {
                UsersCount = usersWithProducts.Count(),
                Users = usersWithProducts
            };

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            };

            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
        }
    }
}