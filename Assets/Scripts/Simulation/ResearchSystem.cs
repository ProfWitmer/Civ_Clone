using CivClone.Infrastructure;

namespace CivClone.Simulation
{
    public sealed class ResearchSystem
    {
        private readonly GameDataCatalog catalog;

        public ResearchSystem(GameDataCatalog catalogRef)
        {
            catalog = catalogRef;
        }

        public void Advance(Player player)
        {
            if (player == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(player.CurrentTechId))
            {
                player.CurrentTechId = GetFirstTechId();
                player.ResearchProgress = 0;
            }

            if (string.IsNullOrWhiteSpace(player.CurrentTechId))
            {
                return;
            }

            int science = 1 + player.Cities.Count;
            player.ResearchProgress += science;

            if (catalog != null && catalog.TryGetTechType(player.CurrentTechId, out var tech))
            {
                if (player.ResearchProgress >= tech.Cost)
                {
                    player.KnownTechs.Add(tech.Id);
                    player.ResearchProgress = 0;
                    player.CurrentTechId = GetFirstUnresearched(player);
                }
            }
        }

        private string GetFirstTechId()
        {
            if (catalog?.TechTypes == null || catalog.TechTypes.Length == 0)
            {
                return string.Empty;
            }

            return catalog.TechTypes[0].Id;
        }

        private string GetFirstUnresearched(Player player)
        {
            if (catalog?.TechTypes == null)
            {
                return string.Empty;
            }

            foreach (var tech in catalog.TechTypes)
            {
                if (tech != null && !player.KnownTechs.Contains(tech.Id))
                {
                    return tech.Id;
                }
            }

            return string.Empty;
        }
    }
}
