using System;
using System.ComponentModel.DataAnnotations;

namespace HAFD.Enums
{
    public enum EmailTypesEnum
    {
        NewUserWelcomeMessage = 1,
        ConfirmEmail,
        ResetPassword,
        ActivatedPlan,
        PlanCompleted,
        Birthday,
    }
}
