﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.ViewModels
{
    public class loginViewModel
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public string LoginStatus { get; set; }

      
        public int UserId { get; set; }

       
        public string Username { get; set; }

     
        public string UserEmail { get; set; }

      
        public string Role { get; set; }
        
        public string token { get; set; }

       
      
    }
}