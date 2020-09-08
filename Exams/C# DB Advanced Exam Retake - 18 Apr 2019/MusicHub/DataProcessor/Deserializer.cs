namespace MusicHub.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using MusicHub.Data.Models;
    using MusicHub.Data.Models.Enums;
    using MusicHub.DataProcessor.ImportDtos;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data";

        private const string SuccessfullyImportedWriter
            = "Imported {0}";
        private const string SuccessfullyImportedProducerWithPhone
            = "Imported {0} with phone: {1} produces {2} albums";
        private const string SuccessfullyImportedProducerWithNoPhone
            = "Imported {0} with no phone number produces {1} albums";
        private const string SuccessfullyImportedSong
            = "Imported {0} ({1} genre) with duration {2}";
        private const string SuccessfullyImportedPerformer
            = "Imported {0} ({1} songs)";

        public static string ImportWriters(MusicHubDbContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var writers = new List<Writer>();

            var writersDtos = JsonConvert.DeserializeObject<ImportWriterDto[]>(jsonString);

            foreach (var writerDto in writersDtos)
            {
                if (!IsValid(writerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var newWriter = new Writer()
                {
                    Name = writerDto.Name,
                    Pseudonym = writerDto.Pseudonym,
                };

                writers.Add(newWriter);
                sb.AppendLine(string.Format(SuccessfullyImportedWriter, newWriter.Name));
            }

            context.Writers.AddRange(writers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProducersAlbums(MusicHubDbContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var producers = new List<Producer>();

            var producersDtos = JsonConvert.DeserializeObject<ImportProducerDto[]>(jsonString);

            foreach (var producerDto in producersDtos)
            {
                if (!IsValid(producerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var albums = new List<Album>();

                foreach (var albumDto in producerDto.Albums)
                {
                    DateTime releaseDate;

                    var isValidDate = DateTime.TryParseExact(albumDto.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDate);

                    if (!isValidDate || !IsValid(albumDto))
                    {
                        continue;
                    }

                    var newAlbum = new Album()
                    {
                        Name = albumDto.Name,
                        ReleaseDate = releaseDate
                    };

                    albums.Add(newAlbum);
                }

                var newProducer = new Producer()
                {
                    Name = producerDto.Name,
                    Pseudonym = producerDto.Pseudonym,
                    PhoneNumber = producerDto.PhoneNumber,
                    Albums = albums
                };

                producers.Add(newProducer);

                if (newProducer.PhoneNumber == null)
                {
                    sb.AppendLine(string.Format(SuccessfullyImportedProducerWithNoPhone, newProducer.Name, albums.Count));
                }
                else
                {
                    sb.AppendLine(string.Format(SuccessfullyImportedProducerWithPhone, newProducer.Name, newProducer.PhoneNumber, albums.Count));
                }
            }

            context.Producers.AddRange(producers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportSongs(MusicHubDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportSongDto[]), new XmlRootAttribute("Songs"));

            using (StringReader streamReader = new StringReader(xmlString))
            {
                ImportSongDto[] songsDtos = (ImportSongDto[])xmlSerializer.Deserialize(streamReader);

                var songs = new List<Song>();

                foreach (var songDto in songsDtos)
                {
                    DateTime createdOn;

                    var isDateValid = DateTime.TryParseExact(songDto.CreatedOn, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out createdOn);

                    TimeSpan duration;

                    var isTimespanValid = TimeSpan.TryParseExact(songDto.Duration, "c", null, out duration);

                    Genre genre;

                    var isValidGenre = Enum.TryParse(songDto.Genre, out genre);

                    if (!IsValid(songDto) || !isDateValid || !isTimespanValid || !isValidGenre)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var albumIds = context.Albums.Select(a => a.Id).ToList();
                    var writerIds = context.Writers.Select(w => w.Id).ToList();

                    if ((songDto.AlbumId != null &&! albumIds.Contains((int)songDto.AlbumId)) || !writerIds.Contains(songDto.WriterId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var song = new Song()
                    {
                        Name = songDto.Name,
                        Duration = duration,
                        CreatedOn = createdOn,
                        Genre = genre,
                        AlbumId = songDto.AlbumId,
                        WriterId = songDto.WriterId,
                        Price = songDto.Price,
                    };

                    songs.Add(song);
                    sb.AppendLine(string.Format(SuccessfullyImportedSong,song.Name,song.Genre,song.Duration));
                }

                context.Songs.AddRange(songs);
                context.SaveChanges();

                return sb.ToString().TrimEnd();
            }
        }

        public static string ImportSongPerformers(MusicHubDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportPerformerDto[]), new XmlRootAttribute("Performers"));

            using (StringReader streamReader = new StringReader(xmlString))
            {
                ImportPerformerDto[] performerDtos = (ImportPerformerDto[])xmlSerializer.Deserialize(streamReader);

                var performers = new List<Performer>();

                var songsIds = context.Songs.Select(x => x.Id).ToList();

                foreach (var performerDto in performerDtos)
                {
                    if (!IsValid(performerDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var performer = new Performer()
                    {
                        FirstName = performerDto.FirstName,
                        LastName = performerDto.LastName,
                        Age = performerDto.Age,
                        NetWorth = performerDto.NetWorth,
                    };

                    bool invalidSong = false;

                    foreach (var performerSongDto in performerDto.PerformersSongs)
                    {
                        if (!songsIds.Contains(performerSongDto.Id))
                        {
                            invalidSong = true;
                            sb.AppendLine(ErrorMessage);
                            break;
                        }

                        performer.PerformerSongs.Add(new SongPerformer() 
                        { 
                            PerformerId = performer.Id, 
                            SongId = performerSongDto.Id 
                        });
                    }

                    if (invalidSong)
                    {
                        continue;
                    }

                    performers.Add(performer);
                    sb.AppendLine(string.Format(SuccessfullyImportedPerformer, performer.FirstName, performer.PerformerSongs.Count));
                }

                context.Performers.AddRange(performers);
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