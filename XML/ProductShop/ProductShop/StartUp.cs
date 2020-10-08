using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XmlFacade;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();
            var usersXML = File.ReadAllText("../../../Datasets/users.xml");
            var productsXML = File.ReadAllText("../../../Datasets/products.xml");
            var categoriesXML = File.ReadAllText("../../../Datasets/categories.xml");
            var categoryProducts = File.ReadAllText("../../../Datasets/categories-products.xml");

            //ResetDatabase(context);

            //Import
            //Console.WriteLine(ImportUsers(context, usersXML));
            //Console.WriteLine(ImportProducts(context, productsXML));
            //Console.WriteLine(ImportProducts(context, productsXML));
            //Console.WriteLine(ImportCategories(context, categoriesXML));
            //Console.WriteLine(ImportCategoryProducts(context, categoryProducts));

            //Export
            //Console.WriteLine(GetProductsInRange(context));
            //Console.WriteLine(GetSoldProducts(context));
            //Console.WriteLine(GetCategoriesByProductsCount(context));
            Console.WriteLine(GetUsersWithProducts(context));

        }

        private static void ResetDatabase(ProductShopContext context)
        {
            context.Database.EnsureDeleted();
            Console.WriteLine("Database deleted!");
            context.Database.EnsureCreated();
            Console.WriteLine("Database Created!");
        }

        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            const string rootElement = "Users";

            var usersResult = XMLConverter.Deserializer<ImportUserDto>(inputXml, rootElement);

            var users = usersResult
                .Select(u => new User
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age= u.Age
                }).ToArray();

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            const string rootElement = "Products";

            var productsResult = XMLConverter.Deserializer<ImportProductDto>(inputXml, rootElement);

            var products = productsResult
                .Select(p => new Product
                {
                    Name = p.Name,
                    Price= p.Price,
                    SellerId= p.SellerId,
                    BuyerId= p.BuyerId
                }).ToArray();

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            const string rootElement = "Categories";

            var categoriesResult = XMLConverter.Deserializer<ImportCategoriesDto>(inputXml, rootElement);

            var categories = categoriesResult
                .Select(p => new Category
                {
                    Name = p.Name,
                }).ToArray();

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            const string rootElement = "CategoryProducts";

            var categoryProductsResult = XMLConverter.Deserializer<ImportCategoryProductDto>(inputXml, rootElement);

            var categoryIds = context.Categories.Select(c => c.Id);
            var productIds = context.Products.Select(p => p.Id);

            var categoryProducts = categoryProductsResult
                .Where(cp=> categoryIds.Contains(cp.CategoryId) && productIds.Contains(cp.ProductId))
                .Select(cp => new CategoryProduct
                {
                    CategoryId= cp.CategoryId,
                    ProductId= cp.ProductId
                }).ToArray();

            context.CategoryProducts.AddRange(categoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Length}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .Take(10)
                .Select(p=> new ProductsInRangeDto() 
                {
                    Name=p.Name,
                    Price=p.Price,
                    Buyer = p.Buyer.FirstName + " " + p.Buyer.LastName
                })
                .ToArray();

            var result = XMLConverter.Serialize<ProductsInRangeDto[]>(products, "Products");

            return result;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var soldProducts = context.Users
                .Where(u=>u.ProductsSold.Any())
                .OrderBy(u=>u.LastName)
                .ThenBy(u=>u.FirstName)
                .Take(5)
                .Select(u=> new UsersWithSoldProductsDto()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    SoldProducts = u.ProductsSold
                    .Select(ps=> new SoldProductDto() 
                    {
                        Name= ps.Name, 
                        Price=ps.Price 
                    })
                    .ToList()
                })
                .ToArray();

            var result = XMLConverter.Serialize<UsersWithSoldProductsDto[]>(soldProducts, "Users");

            return result;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(c=> new ExportCategory() 
                {
                    Name= c.Name,
                    Count=c.CategoryProducts.Count(),
                    AveragePrice= c.CategoryProducts.Average(cp=>cp.Product.Price),
                    TotalRevenue=c.CategoryProducts.Sum(cp => cp.Product.Price)
                })
                .OrderByDescending(x=>x.Count)
                .ThenBy(x=>x.TotalRevenue)
                .ToArray();

            var result = XMLConverter.Serialize<ExportCategory[]>(categories, "Categories");

            return result;
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .AsEnumerable()
                .Where(u => u.ProductsSold.Any())
                .OrderByDescending(u => u.ProductsSold.Count())
                .Take(10)
                .Select(u => new ExportUser()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProductsWithCount = new SoldProductsWithCountDto()
                    {
                        Count = u.ProductsSold.Count(),
                        SoldProducts = u.ProductsSold.Select(ps=> new SoldProductDto() { 
                            Name= ps.Name,
                            Price = ps.Price
                        })
                        .OrderByDescending(x=>x.Price)
                        .ToArray()
                    }
                })
               .ToArray();

            var resultUsers = new ExportUserWithProducts()
            {
                Count = context.Users
                .Where(u => u.ProductsSold.Any())
                .Count(),
                Users = users
            };

            var result = XMLConverter.Serialize<ExportUserWithProducts>(resultUsers, "Users");

            return result;
        }
    }
}

