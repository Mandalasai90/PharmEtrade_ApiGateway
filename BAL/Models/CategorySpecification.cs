﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.Models
{
    public class CategorySpecification
    {
        public int CategorySpecificationId { get; set; }
        public string? SpecificationName { get; set; }
    }

    public class OrderStatus
    {
        public int StatusId { get; set; }
        public string? StatusDescription { get; set; }
    }
}
