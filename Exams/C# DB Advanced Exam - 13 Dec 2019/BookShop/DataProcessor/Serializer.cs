namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var craziestAuthors = context.Authors.Select(a => new
            {
                AuthorName = $"{a.FirstName} {a.LastName}",
                Books= a.AuthorsBooks
                .OrderByDescending(x => x.Book.Price)
                .Select(b=> new 
                {
                    BookName = b.Book.Name,
                    BookPrice = b.Book.Price.ToString("F2")
                })
                
            })
            .ToArray()
            .OrderByDescending(x=>x.Books.Count())
            .ThenBy(x=>x.AuthorName);

            var json = JsonConvert.SerializeObject(craziestAuthors,Formatting.Indented);

            return json;
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            var books = context.Books
                .Where(b => b.PublishedOn < date && b.Genre == Genre.Science)
                .OrderByDescending(x => x.Pages)
                .ThenByDescending(x => x.PublishedOn)
                .Take(10)
                .Select(x => new ExportBookDto()
                {
                    Pages= x.Pages,
                    Name = x.Name,
                    Date = x.PublishedOn.ToString("d",CultureInfo.InvariantCulture)
                })
                .ToArray();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var xmlSerializer = new XmlSerializer(typeof(ExportBookDto[]), new XmlRootAttribute("Books"));

            var sb = new StringBuilder();
            var stream = new StringWriter(sb);
            xmlSerializer.Serialize(stream, books,ns);

            return sb.ToString().TrimEnd();
        }
    }
}