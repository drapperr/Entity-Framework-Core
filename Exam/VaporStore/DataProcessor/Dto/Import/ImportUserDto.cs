﻿using System;
using System.ComponentModel.DataAnnotations;
namespace VaporStore.DataProcessor.Dto.Import
{
    public class ImportUserDto
    {
        [Required]
        [RegularExpression("^[A-Z][a-z]{2,} [A-Z][a-z]{2,}$")]
        public string FullName { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Range(3,103)]
        public int Age { get; set; }

        public ImportUserCardDto[] Cards { get; set; }
    }
}
