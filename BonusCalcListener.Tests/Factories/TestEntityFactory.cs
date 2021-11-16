using AutoFixture;
using BonusCalcListener.Boundary;
using System;
using System.Collections.Generic;
using System.Text;

namespace BonusCalcListener.Tests.Factories
{
    public class TestEntityFactory
    {
        private readonly Fixture _fixture = new Fixture();
        public EntityEventSns ConstructEvent(string evtType, string version = RepairsEventVersions.V1)
        {
            return _fixture.Build<EntityEventSns>()
                .With(e => e.EventType, evtType)
                .With(e => e.Version, version)
                .Create();
        }

    }
}
