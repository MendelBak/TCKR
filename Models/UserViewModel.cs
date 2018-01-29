using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tckr.Models
{
    public class RegisterViewModel : BaseEntity
    {
        [Required]
        [MinLength(2, ErrorMessage = "First Name must be at least 2 characters")]
        public string FirstName { get; set; }


        [Required]
        [MinLength(2, ErrorMessage = "Last Name must be at least 2 characters")]
        public string LastName { get; set; }


        [Required]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        [Required]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match!")]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel : BaseEntity
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

}