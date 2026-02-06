using System.Collections.Generic;

namespace CivClone.Simulation
{
    public sealed class FogOfWarSystem
    {
        private readonly int visionRadius;

        public FogOfWarSystem(int visionRadius = 2)
        {
            this.visionRadius = visionRadius;
        }

        public void Apply(GameState state)
        {
            if (state == null || state.Map == null)
            {
                return;
            }

            foreach (var tile in state.Map.Tiles)
            {
                tile.Visible = false;
            }

            var activePlayer = state.ActivePlayer;
            if (activePlayer == null)
            {
                return;
            }

            foreach (var unit in activePlayer.Units)
            {
                RevealAround(state.Map, unit.Position, visionRadius);
            }

            foreach (var city in activePlayer.Cities)
            {
                RevealAround(state.Map, city.Position, visionRadius);
            }
        }

        private static void RevealAround(WorldMap map, GridPosition center, int radius)
        {
            for (int y = center.Y - radius; y <= center.Y + radius; y++)
            {
                for (int x = center.X - radius; x <= center.X + radius; x++)
                {
                    var tile = map.GetTile(x, y);
                    if (tile == null)
                    {
                        continue;
                    }

                    tile.Visible = true;
                    tile.Explored = true;
                }
            }
        }
    }
}
