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

            if (string.IsNullOrWhiteSpace(player.CurrentTechId) || !TryGetValidTech(player, player.CurrentTechId, out _))
            {
                player.CurrentTechId = GetFirstAvailableTech(player);
                player.ResearchProgress = 0;
            }

            if (string.IsNullOrWhiteSpace(player.CurrentTechId))
            {
                return;
            }

            int science = 1 + player.Cities.Count;
            science += GetCivicScienceBonus(player);
            science += GetResourceScienceBonus(player);
            player.ResearchProgress += science;

            if (catalog != null && catalog.TryGetTechType(player.CurrentTechId, out var tech))
            {
                if (player.ResearchProgress >= tech.Cost)
                {
                    if (!player.KnownTechs.Contains(tech.Id))
                    {
                        player.KnownTechs.Add(tech.Id);
                    }
                    player.ResearchProgress = 0;
                    player.CurrentTechId = GetFirstAvailableTech(player);
                }
            }
        }

        private bool TryGetValidTech(Player player, string techId, out TechType tech)
        {
            tech = null;
            if (catalog == null || player == null || string.IsNullOrWhiteSpace(techId))
            {
                return false;
            }

            if (!catalog.TryGetTechType(techId, out tech))
            {
                return false;
            }

            if (player.KnownTechs.Contains(techId))
            {
                return false;
            }

            return AreTechPrereqsMet(player, tech);
        }

        private bool AreTechPrereqsMet(Player player, TechType tech)
        {
            if (player == null || tech == null || string.IsNullOrWhiteSpace(tech.Prerequisites))
            {
                return true;
            }

            var parts = tech.Prerequisites.Split(,);
            foreach (var part in parts)
            {
                var id = part.Trim();
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                if (!player.KnownTechs.Contains(id))
                {
                    return false;
                }
            }

            return true;
        }

        private int GetCivicScienceBonus(Player player)
        {
            if (player?.Civics == null)
            {
                return 0;
            }

            foreach (var civic in player.Civics)
            {
                if (civic != null && civic.CivicId == "republic")
                {
                    return player.Cities.Count;
                }
            }

            return 0;
        }

        private int GetResourceScienceBonus(Player player)
        {
            if (player?.AvailableResources == null || catalog == null)
            {
                return 0;
            }

            int bonus = 0;
            foreach (var resourceId in player.AvailableResources)
            {
                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    continue;
                }

                if (catalog.TryGetResourceType(resourceId, out var resource))
                {
                    bonus += resource.ScienceBonus;
                }
            }

            return bonus;
        }

        private string GetFirstAvailableTech(Player player)
        {
            if (catalog?.TechTypes == null)
            {
                return string.Empty;
            }

            foreach (var tech in catalog.TechTypes)
            {
                if (tech == null || string.IsNullOrWhiteSpace(tech.Id))
                {
                    continue;
                }

                if (player.KnownTechs.Contains(tech.Id))
                {
                    continue;
                }

                if (AreTechPrereqsMet(player, tech))
                {
                    return tech.Id;
                }
            }

            return string.Empty;
        }
    }
}
