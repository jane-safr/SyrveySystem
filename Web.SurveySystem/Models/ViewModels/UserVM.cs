using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.SurveySystem.Models.ViewModels
{
    public class UserVM
    {
        [Required]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Требуется имя / Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Требуется email / Email address is required")]
        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail is invalid")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        public List<RoleVM> Roles { get; set; }
        public bool IsExternal { get; set; }
        public bool IsTwoFactor { get; set; }
        public bool IsLockoutEnabled { get; set; }
    }
}