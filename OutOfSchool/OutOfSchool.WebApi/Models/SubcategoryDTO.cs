﻿using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class SubcategoryDTO
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [DataType(DataType.Text)]
        [MaxLength(60)]
        [MinLength(1)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public long CategoryId { get; set; }

        public virtual CategoryDTO Category { get; set; }
    }
}