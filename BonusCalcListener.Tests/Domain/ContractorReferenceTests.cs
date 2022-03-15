using BonusCalcListener.Domain;
using FluentAssertions;
using NUnit.Framework;

namespace BonusCalcListener.Tests.Domain
{
    public class ContractorReferenceTests
    {
        public ContractorReferenceTests()
        {
        }

        [Test]
        public void ContractorReferencesAreMappedCorrectly()
        {
            string result;

            // Repairs
            result = ContractorReference.Map("H01");
            result.Should().Be("H3009");

            // Voids
            result = ContractorReference.Map("H02");
            result.Should().Be("H3007");

            // Out of hours
            result = ContractorReference.Map("H03");
            result.Should().Be("H3015");

            // Gas breakdown
            result = ContractorReference.Map("H04");
            result.Should().Be("H3002");

            // Boiler house heating
            result = ContractorReference.Map("H05");
            result.Should().Be("H3002");

            // Electrical (Planned)
            result = ContractorReference.Map("H06");
            result.Should().Be("H3003");

            // Drains & paths
            result = ContractorReference.Map("H07");
            result.Should().Be("H3010");

            // Gas servicing
            result = ContractorReference.Map("H08");
            result.Should().Be("H3002");

            // Painting
            result = ContractorReference.Map("H09");
            result.Should().Be("H3005");

            // Tank maintenance
            result = ContractorReference.Map("H10");
            result.Should().Be("H3010");

            // Legal action works
            result = ContractorReference.Map("H11");
            result.Should().Be("H3016");

            // Surveyors/Inspectors
            result = ContractorReference.Map("H12");
            result.Should().Be("H3004");

            // Sign services
            result = ContractorReference.Map("H13");
            result.Should().Be("H3008");

            // Estate cleaning
            result = ContractorReference.Map("H14");
            result.Should().Be("H1040");

            // Grounds maintenance
            result = ContractorReference.Map("H15");
            result.Should().Be("H1039");

            // Missing reference
            result = ContractorReference.Map(null);
            result.Should().Be(null);
        }
    }
}
