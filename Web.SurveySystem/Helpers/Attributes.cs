using System;
using System.ComponentModel.DataAnnotations;

namespace Web.SurveySystem.Helpers
{
    public class DeadlineInvitationAttribute : RangeAttribute
    {
        public DeadlineInvitationAttribute() : base(typeof(DateTime),
                DateTime.Now.AddDays(2).ToShortDateString(),
            DateTime.Now.AddYears(5).ToShortDateString())
        { }
    }
}