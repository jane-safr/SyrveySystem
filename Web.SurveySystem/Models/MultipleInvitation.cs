using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Web.SurveySystem.Helpers;

namespace Web.SurveySystem.Models
{
    public class MultipleInvitation
    {
        [Required]
        public Guid SurveyId { get; set; }
        [Required]
        [DeadlineInvitation(ErrorMessage = "Дата завершения не может быть меньше послезавтра / Date cannot be less than the day after tomorrow")]
        public DateTime DateEnd { get; set; }
        [Required(ErrorMessage = "Пригласите пользователей / Select users")]
        public IEnumerable<InvitedUser> Users { get; set; }
    }

    public class InvitedUser
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
    }
}