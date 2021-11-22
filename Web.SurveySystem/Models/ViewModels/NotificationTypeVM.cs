using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class NotificationTypeVM : BasePropertiesVM
    {
        public Guid? NotificationTypeId { get; set; }

        [Required(ErrorMessage = "Введите наименование (рус.) / Enter NameRus")]
        [StringLength(1000, ErrorMessage = "NameRus должен быть от {2} до {1} символов", MinimumLength = 1)]
        public string NameRus { get; set; }

        [Required(ErrorMessage = "Введите наименование (англ.) / Enter NameEng")]
        [StringLength(1000, ErrorMessage = "NameEng должен быть от {2} до {1} символов", MinimumLength = 1)]
        public string NameEng { get; set; }

        [Required(ErrorMessage = "Введите шаблон сообщения / Enter message template")]
        [StringLength(1000, ErrorMessage = "MessageTemplate  должен быть от {2} до {1} символов", MinimumLength = 1)]
        public string MessageTemplate { get; set; }
        [StringLength(500, ErrorMessage = "TemplateLink должен быть от {2} до {1} символов", MinimumLength = 0)]
        public string TemplateLink { get; set; }

        public bool IsSurvey { get; set; } // анкета
        public bool IsAnonymousSurvey { get; set; } // анонимный тип
        public bool IsTest { get; set; } // тест как проверка знаний
      
        public virtual ICollection<NotificationVM> Notifications { get; set; }
        public NotificationTypeVM()
        {
            Notifications = new List<NotificationVM>();
        }
    }
}