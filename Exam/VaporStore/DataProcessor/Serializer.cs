namespace VaporStore.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Export;

    public static class Serializer
    {
        public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
        {
            var games = context.Genres
                .AsEnumerable()
                .Where(g => genreNames.Contains(g.Name))
                .Select(g => new ExportGenreDto
                {
                    Id = g.Id,
                    Genre = g.Name,
                    Games = g.Games
                    .Where(ga => ga.Purchases.Count != 0)
                    .Select(ga => new ExportGameDto
                    {
                        Id = ga.Id,
                        Title = ga.Name,
                        Developer = ga.Developer.Name,
                        Tags = string.Join(", ", ga.GameTags.Select(t => t.Tag.Name)),
                        Players = ga.Purchases.Count()
                    })
                    .OrderByDescending(x => x.Players)
                    .ThenBy(x => x.Id)
                    .ToArray(),
                })
                .ToArray();

            var result = games.Select(g => new
            {
                Id = g.Id,
                Genre = g.Genre,
                Games = g.Games,
                TotalPlayers = g.Games.Select(ga => ga.Players).Sum()
            })
                .OrderByDescending(x => x.TotalPlayers)
                .ThenBy(x => x.Id)
                .ToArray();

            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            return json;
        }

        public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
        {
            var users = context.Cards
                .Where(c => c.Purchases.Count != 0)
                .Select(c => new ExportUserDto()
                {
                    Username = c.User.Username,
                    Purchases = c.Purchases
                    .Where(p => Enum.Parse<PurchaseType>(storeType) == p.Type)
                    .OrderBy(x=>x.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture))
                    .Select(p => new ExportUserPurchaseDto()
                    {
                        Card = c.Number,
                        Cvc = c.Cvc,
                        Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                        Game = new ExportPurchaseGameDto()
                        {
                            Title = p.Game.Name,
                            Genre = p.Game.Genre.Name,
                            Price = p.Game.Price
                        }
                    })
                    .ToArray(),
                    TotalSpent = c.Purchases.Where(p => Enum.Parse<PurchaseType>(storeType) == p.Type).Sum(p => p.Game.Price)
                })
                .ToArray()
                .OrderByDescending(x=>x.Purchases.Sum(p=>p.Game.Price))
                .ThenBy(x=>x.Username)
               .ToArray();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var xmlSerializer = new XmlSerializer(typeof(ExportUserDto[]), new XmlRootAttribute("Users"));

            var sb = new StringBuilder();
            var stream = new StringWriter(sb);
            xmlSerializer.Serialize(stream, users,ns);

            return sb.ToString().TrimEnd();
        }
    }
}