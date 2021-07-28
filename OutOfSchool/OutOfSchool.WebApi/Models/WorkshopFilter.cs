﻿using System.Collections.Generic;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class WorkshopFilter : OffsetFilter
    {
        public List<long> Ids { get; set; } = null;

        public string SearchText { get; set; } = string.Empty;

        public string OrderByField { get; set; } = OrderBy.Rating.ToString();

        public int MinAge { get; set; } = 0;

        public int MaxAge { get; set; } = 100;

        public bool IsFree { get; set; } = false;

        public int MinPrice { get; set; } = 0;

        public int MaxPrice { get; set; } = int.MaxValue;

        public List<long> DirectionIds { get; set; } = new List<long> { 0 };

        public string City { get; set; } = string.Empty;

        public bool WithDisabilityOptions { get; set; } = false;
    }
}