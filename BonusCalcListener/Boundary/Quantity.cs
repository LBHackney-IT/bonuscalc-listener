using System;
using System.Collections.Generic;
using System.Text;

namespace BonusCalcListener.Boundary
{
    public class Quantity
    {
        public Quantity() { }

        public Quantity(int amount)
        {
            Amount = amount;
        }

        public double Amount { get; set; }
        public int? UnitOfMeasurementCode { get; set; }
    }
}
