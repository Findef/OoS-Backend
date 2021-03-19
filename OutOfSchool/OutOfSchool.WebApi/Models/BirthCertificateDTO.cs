﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class BirthCertificateDTO
    {
        public long Id { get; set; }

        public string SvidSer { get; set; } = string.Empty;

        public string SvidNum { get; set; } = string.Empty;

        public string SvidNumMD5 { get; set; } = string.Empty;

        public string SvidWho { get; set; } = string.Empty;

        public DateTime SvidDate { get; set; } = default;

        public long ChildId { get; set; }
    }
}
