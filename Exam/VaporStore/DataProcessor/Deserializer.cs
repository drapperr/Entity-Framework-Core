namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
	{
		const string ErrorMessage = "Invalid Data";

        public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			var gamesDtos = JsonConvert.DeserializeObject<ImportGameDto[]>(jsonString);

			var sb = new StringBuilder();

			var validGames = new List<Game>();

			var localDevelopers = context.Developers.ToList();

			var localGenres = context.Genres.ToList();

			var localTags = context.Tags.ToList();


			foreach (var gameDto in gamesDtos)
            {
                if (!IsValid(gameDto))
                {
					sb.AppendLine(ErrorMessage);
					continue;
                }

				DateTime releaseDate;

				var isDateValid = DateTime.TryParseExact(gameDto.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
					DateTimeStyles.None, out releaseDate);

                if (!isDateValid)
                {
					sb.AppendLine(ErrorMessage);
					continue;
				}

				var developer = localDevelopers.FirstOrDefault(d => d.Name == gameDto.Developer);

                if (developer == null)
                {
					developer = new Developer()
					{
						Name = gameDto.Developer
					};

					localDevelopers.Add(developer);
                }

				var genre = localGenres.FirstOrDefault(g => g.Name == gameDto.Genre);

                if (genre == null)
                {
					genre = new Genre()
					{
						Name = gameDto.Genre
					};

					localGenres.Add(genre);
                }

                if (gameDto.Tags.Length==0)
                {
					sb.AppendLine(ErrorMessage);
					continue;
				}

				var game = new Game()
				{
					Name = gameDto.Name,
					Price = gameDto.Price,
					Developer = developer,
					Genre = genre
				};

				foreach (var tagName in gameDto.Tags)
                {
					var tag = localTags.FirstOrDefault(t => t.Name == tagName);

                    if (tag == null)
                    {
						tag = new Tag()
						{
							Name = tagName
						};

						localTags.Add(tag);
                    }

					game.GameTags.Add(new GameTag() 
					{
						Game = game, 
						Tag = tag 
					});
                }
				validGames.Add(game);
				sb.AppendLine($"Added {game.Name} ({game.Genre.Name}) with {game.GameTags.Count} tags");
			}

			context.Games.AddRange(validGames);
			context.SaveChanges();

			return sb.ToString().TrimEnd();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
			var usersDtos = JsonConvert.DeserializeObject<ImportUserDto[]>(jsonString);

			var sb = new StringBuilder();

			var validUsers = new List<User>();

			var localCards = context.Cards.ToList();


            foreach (var userDto in usersDtos)
            {
                if (!IsValid(userDto))
                {
					sb.AppendLine(ErrorMessage);
					continue;
                }

				var user = new User()
				{
					FullName = userDto.FullName,
					Username = userDto.UserName,
					Email = userDto.Email,
					Age = userDto.Age
				};

                foreach (var cardDto in userDto.Cards)
                {
                    if (!IsValid(cardDto))
                    {
						sb.AppendLine(ErrorMessage);
						continue;
					}

					CardType cardType;

					var isValidType = Enum.TryParse(cardDto.Type, out cardType);

                    if (!isValidType)
                    {
						sb.AppendLine(ErrorMessage);
						continue;
					}

					var card = localCards.FirstOrDefault(c => c.Number == cardDto.Number && c.Cvc == cardDto.CVC && c.Type == cardType);

					if (card == null)
                    {
						card = new Card()
						{
							Number = cardDto.Number,
							Cvc = cardDto.CVC,
							Type = cardType
						};

						localCards.Add(card);
                    }

					user.Cards.Add(card);
                }

				validUsers.Add(user);

				sb.AppendLine($"Imported {user.Username} with {user.Cards.Count} cards");
            }

			context.Users.AddRange(validUsers);
			context.SaveChanges();

			return sb.ToString().TrimEnd();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			var sb = new StringBuilder();

			var localCards = context.Cards.ToList();

			var localGames = context.Games.ToList();

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportPurchaseDto[]), new XmlRootAttribute("Purchases"));

			using (StringReader streamReader = new StringReader(xmlString))
			{
				ImportPurchaseDto[] purchasesDtos = (ImportPurchaseDto[])xmlSerializer.Deserialize(streamReader);

				var validPurchases = new List<Purchase>();

				

				foreach (var purchaseDto in purchasesDtos)
				{
					if (!IsValid(purchaseDto))
					{
						sb.AppendLine(ErrorMessage);
						continue;
					}

					DateTime date;

					var isDateValid = DateTime.TryParseExact(purchaseDto.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture,
						DateTimeStyles.None, out date);

					if (!isDateValid)
					{
						sb.AppendLine(ErrorMessage);
						continue;
					}

					PurchaseType purchaseType;

					var isValidType = Enum.TryParse(purchaseDto.Type,out purchaseType);

                    if (!isValidType)
                    {
						sb.AppendLine(ErrorMessage);
						continue;
					}

					var card = localCards.FirstOrDefault(c => c.Number == purchaseDto.Card);
					var game = localGames.FirstOrDefault(g => g.Name == purchaseDto.Title);

                    if (card == null || game == null)
                    {
						sb.AppendLine(ErrorMessage);
						continue;
					}

					var purchase = new Purchase()
					{
						Type = purchaseType,
						ProductKey =purchaseDto.Key,
						Card = card,
						Date = date,
						Game = game
					};

					validPurchases.Add(purchase);
					sb.AppendLine($"Imported {game.Name} for {card.User.Username}");
				}

				context.Purchases.AddRange(validPurchases);
				context.SaveChanges();

				return sb.ToString().TrimEnd();
			}
		}

		private static bool IsValid(object dto)
		{
			var validationContext = new ValidationContext(dto);
			var validationResult = new List<ValidationResult>();

			return Validator.TryValidateObject(dto, validationContext, validationResult, true);
		}
	}
}