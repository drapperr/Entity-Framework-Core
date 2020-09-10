﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace MusicHub.DataProcessor.ImportDtos
{
    [XmlType("Song")]
    public class ImportSongDto
    {
        [XmlElement("Name")]
        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string Name { get; set; }

        [XmlElement("Duration")]
        [Required]
        public string Duration { get; set; }

        [XmlElement("CreatedOn")]
        [Required]
        public string CreatedOn { get; set; }

        [XmlElement("Genre")]
        [Required]
        public string Genre { get; set; }

        [XmlElement("AlbumId")]
        [Required]
        public int? AlbumId { get; set; }

        [XmlElement("WriterId")]
        [Required]
        public int WriterId { get; set; }

        [XmlElement("Price")]
        [Required]
        [Range(typeof(decimal),"0", "79228162514264337593543950335")]
        public decimal Price { get; set; }
    }
}
