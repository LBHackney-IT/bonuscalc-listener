using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BonusCalcListener.Domain
{
    public enum PaymentType
    {
        [Description("Overtime")] Overtime = 1,
        [Description("Bonus")] Bonus = 2,
        [Description("CloseToBase")] CloseToBase = 3,
    }
}
