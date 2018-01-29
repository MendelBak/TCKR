using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace tckr.Models
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }
        public string Homepage { get; set; }
    }
}
