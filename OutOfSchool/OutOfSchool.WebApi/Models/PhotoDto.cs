﻿using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class PhotoDto
    {
        public long Id { get; set; }

        public string Path { get; set; }

        [Required]
        public long EntityId { get; set; }

        [Required]
        public EntityType EntityType { get; set; }

        [Required]
        public PhotoExtension PhotoExtension { get; set; }

        public byte[] Photo { get; set; }
    }
}