namespace MusicHub.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using MusicHub.DataProcessor.ExportDtos;
    using Newtonsoft.Json;

    public class Serializer
    {
        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albums = context.Albums
                .Where(a => a.ProducerId == producerId)
                 .OrderByDescending(x => x.Songs.Sum(s=>s.Price))
                .Select(a => new ExportAlbumDto()
                {
                    AlbumName = a.Name,
                    ReleaseDate = a.ReleaseDate.ToString("MM/dd/yyyy",CultureInfo.InvariantCulture),
                    ProducerName = a.Producer.Name,
                    Songs = a.Songs.Select(s => new ExportAlbumSongDto()
                    {
                        SongName = s.Name,
                        Price = s.Price.ToString("F2"),
                        Writer = s.Writer.Name
                    })
                    .OrderByDescending(x => x.SongName)
                    .ThenBy(x => x.Writer)
                    .ToArray(),
                    AlbumPrice = a.Songs.Sum(s => s.Price).ToString("F2")
                })
                .ToArray();

            var json = JsonConvert.SerializeObject(albums, Formatting.Indented);

            return json;
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songs = context.Songs
                .Where(s => s.Duration.TotalSeconds > duration)
                .Select(s => new ExportSongsDto()
                {
                    SongName= s.Name,
                    Performer = $"{s.SongPerformers.FirstOrDefault().Performer.FirstName} {s.SongPerformers.FirstOrDefault().Performer.LastName}",
                    Writer = s.Writer.Name,
                    AlbumProducer = s.Album.Producer.Name,
                    Duration = s.Duration.ToString("c")
                })
                .OrderBy(x=>x.SongName)
                .ThenBy(x=>x.Writer)
                .ThenBy(x=>x.Performer)
                .ToArray();

            var xmlSerializer = new XmlSerializer(typeof(ExportSongsDto[]), new XmlRootAttribute("Songs"));

            var sb = new StringBuilder();
            var stream = new StringWriter(sb);
            xmlSerializer.Serialize(stream, songs);

            return sb.ToString().TrimEnd();
        }
    }
}