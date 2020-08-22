namespace VaporStore.DataProcessor.Dto.Export
{
    public class ExportGenreDto
    {
        public int Id { get; set; }

        public string Genre { get; set; }

        public ExportGameDto[] Games { get; set; }

        public int TotalPlayers { get; set; }
    }
}



                    //Id = g.Id,
                    //Genre = g.Name,
                    //Games = g.Games
                    //.Where(ga => ga.Purchases.Count != 0)
                    //.Select(ga => new
                    //{
                    //    Id = ga.Id,
                    //    Title = ga.Name,
                    //    Developer = ga.Developer.Name,
                    //    Tags = ga.GameTags.Select(t => string.Join(", ", t.Tag.Name)),
                    //    Players = ga.Purchases.Count()
                    //})
                    //.ToArray()
                    //.OrderByDescending(x=>x.Players)
                    //.ThenBy(x=>x.Id),
                    //TotalPlayers = g.Games.Sum(ga => ga.Purchases.Count)