using System;

namespace BonusCalcListener.Domain
{
    public static class ContractorReference
    {
        public const string Repairs = "H01";
        public const string Voids = "H02";
        public const string OutOfHours = "H03";
        public const string GasBreakdown = "H04";
        public const string BoilerHouseHeating = "H05";
        public const string ElectricalPlanned = "H06";
        public const string DrainsAndPaths = "H07";
        public const string GasServicing = "H08";
        public const string Painting = "H09";
        public const string TankMaintenance = "H10";
        public const string LegalActionWorks = "H11";
        public const string Surveying = "H12";
        public const string SignServices = "H13";
        public const string EstateCleaning = "H14";
        public const string GroundsMaintenance = "H15";

        public static string Map(string contractorReference)
        {
            switch (contractorReference)
            {

                case Repairs:
                    return "H3009";

                case Voids:
                    return "H3007";

                case OutOfHours:
                    return "H3015";

                case GasBreakdown:
                case BoilerHouseHeating:
                case GasServicing:
                    return "H3002";

                case ElectricalPlanned:
                    return "H3003";

                case DrainsAndPaths:
                case TankMaintenance:
                    return "H3010";

                case Painting:
                    return "H3005";

                case LegalActionWorks:
                    return "H3016";

                case Surveying:
                    return "H3004";

                case SignServices:
                    return "H3008";

                case EstateCleaning:
                    return "H1040";

                case GroundsMaintenance:
                    return "H1039";

                case null:
                    return null;

                default:
                    throw new ArgumentException($"Unknown contractor reference: {contractorReference}");
            }
        }
    }
}
