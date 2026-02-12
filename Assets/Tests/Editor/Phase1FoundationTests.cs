using CivClone.Simulation;
using NUnit.Framework;

namespace CivClone.Tests
{
    public class Phase1FoundationTests
    {
        [Test]
        public void MapGenerator_CreatesExpectedDimensions()
        {
            var config = new MapConfig
            {
                Width = 5,
                Height = 4,
                Seed = 123,
                DefaultTerrainId = "plains"
            };

            var generator = new MapGenerator(config);
            var map = generator.Generate();

            Assert.AreEqual(5, map.Width);
            Assert.AreEqual(4, map.Height);
            Assert.AreEqual(20, map.Tiles.Count);
        }
    }
}
