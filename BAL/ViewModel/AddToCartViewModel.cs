﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.ViewModel
{
   public  class AddToCartViewModel
    {

        public int AddtoCartId { get; set; }
        public int Userid { get; set; }
        public int Imageid { get; set; }
        public int ProductId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}