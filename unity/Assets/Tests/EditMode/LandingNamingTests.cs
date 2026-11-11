using NUnit.Framework;
using PhoenicianJourney.Landing;

namespace PhoenicianJourney.Tests
{
    public sealed class LandingNamingTests
    {
        [TestCase("COL_wall", MeshRole.Collider)]
        [TestCase("col_Barrier.001", MeshRole.Collider)]
        [TestCase("NOCOL_rope", MeshRole.Visual)]
        [TestCase("Water_Surface", MeshRole.Visual)]
        [TestCase("Shore_Ground", MeshRole.Solid)]
        [TestCase("Cargo_Footprint", MeshRole.Solid)]
        public void ClassifiesMeshesByName(string name, MeshRole expected)
        {
            Assert.That(LandingNaming.RoleOf(name), Is.EqualTo(expected));
        }

        [TestCase("Cargo_Footprint", StationId.Cargo)]
        [TestCase("Dye_Stall_Footprint", StationId.Dye)]
        [TestCase("Scribe_Table_Proxy", StationId.Scribe)]
        public void FindsStationsByName(string name, StationId expected)
        {
            Assert.That(LandingNaming.StationOf(name), Is.EqualTo(expected));
        }

        [Test]
        public void IgnoresOtherNames()
        {
            Assert.That(LandingNaming.StationOf("Shallow_Boat_Proxy"), Is.Null);
            Assert.That(LandingNaming.IsSpawn("SPAWN"), Is.True);
            Assert.That(LandingNaming.IsSpawn("Spawn_Reference"), Is.False);
        }
    }
}
