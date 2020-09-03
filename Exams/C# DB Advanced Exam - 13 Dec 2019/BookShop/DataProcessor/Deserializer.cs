namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            var sb = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportBookDto[]), new XmlRootAttribute("Books"));

            using (StringReader streamReader = new StringReader(xmlString))
            {
                ImportBookDto[] bookDtos = (ImportBookDto[])xmlSerializer.Deserialize(streamReader);

                var validBooks = new List<Book>();

                foreach (var bookDto in bookDtos)
                {
                    if (!IsValid(bookDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime publishedOn;

                    var isDateValid = DateTime.TryParseExact(bookDto.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out publishedOn);

                    if (!isDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var book = new Book()
                    {
                        Name = bookDto.Name,
                        Genre = (Genre)bookDto.Genre,
                        Price = (decimal)bookDto.Price,
                        Pages = bookDto.Pages,
                        PublishedOn = publishedOn
                    };

                    validBooks.Add(book);
                    sb.AppendLine(string.Format(SuccessfullyImportedBook, book.Name, book.Price));
                }

                context.AddRange(validBooks);
                context.SaveChanges();

                return sb.ToString().TrimEnd();
            }
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var validAuthors = new List<Author>();

            var emailsDb = context.Authors.Select(a => a.Email);
            var booksIds = context.Books.Select(b => b.Id);

            var authorsDtos = JsonConvert.DeserializeObject<ImportAuthorDto[]>(jsonString);

            foreach (var authorDto in authorsDtos)
            {
                if (!IsValid(authorDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (validAuthors.Any(a => a.Email == authorDto.Email) ||
                    emailsDb.Any(x => x == authorDto.Email))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var author = new Author()
                {
                    FirstName = authorDto.FirstName,
                    LastName = authorDto.LastName,
                    Email = authorDto.Email,
                    Phone = authorDto.Phone,
                    //AuthorsBooks = new List<AuthorBook>()
                };

                var validAuthorBooks = new List<AuthorBook>();

                if (!(authorDto.Books == null))
                {
                    foreach (var authorBook in authorDto.Books)
                    {
                        if (authorBook.Id == null)
                        {
                            continue;
                        }

                        if (!booksIds.Contains((int)authorBook.Id))
                        {
                            continue;
                        }

                        validAuthorBooks.Add(new AuthorBook() { AuthorId = author.Id, BookId = (int)authorBook.Id });
                    }
                }

                

                if (validAuthorBooks.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                foreach (var authorBook in validAuthorBooks)
                {
                    author.AuthorsBooks.Add(authorBook);
                }

                validAuthors.Add(author);
                sb.AppendLine(string.Format(SuccessfullyImportedAuthor, $"{author.FirstName} {author.LastName}", validAuthorBooks.Count));
            }

            context.AddRange(validAuthors);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}