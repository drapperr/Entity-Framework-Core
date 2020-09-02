using BookShop.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Data.Models
{
    public class Book
    {
        public Book()
        {
            this.AuthorsBooks = new List<AuthorBook>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        [Required]
        public Genre Genre { get; set; }

        public decimal Price { get; set; }

        public int Pages { get; set; }

        [Required]
        public DateTime PublishedOn { get; set; }

        [ForeignKey("BookId")]
        public ICollection<AuthorBook> AuthorsBooks { get; set; }
    }
}
