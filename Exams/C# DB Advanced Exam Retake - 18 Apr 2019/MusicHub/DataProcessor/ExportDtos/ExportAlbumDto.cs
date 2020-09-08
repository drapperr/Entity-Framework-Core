using System;
using System.Collections.Generic;
using System.Text;

namespace MusicHub.DataProcessor.ExportDtos
{
    public class ExportAlbumDto
    {
        public string AlbumName { get; set; }

        public string ReleaseDate { get; set; }

        public string ProducerName { get; set; }

        public ExportAlbumSongDto[] Songs { get; set; }

        public string AlbumPrice { get; set; }
    }
}


//"AlbumName": "Devil's advocate",
//    "ReleaseDate": "07/21/2018",
//    "ProducerName": "Evgeni Dimitrov",
//    "Songs": [
//      {
//        "SongName": "Numb",
//        "Price": "13.99",
//        "Writer": "Kara-lynn Sharpous"
//      },
