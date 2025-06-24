using Keepercraft.RimKeeperAnimals.Models;
using UnityEngine;
using Verse;

namespace Keepercraft.RimKeeperAnimals
{
    public class RimKeeperAnimalsHelperMod : Mod
    {
       // public string mapID = "";
        public int mapAnimalsCount = 0;
        public string ActiveMateThresholdText = "0";
        
        public bool ActiveMateThresholdWildCache = false;
        public bool ActiveMateThresholdFactionCache = false;

        public RimKeeperAnimalsHelperMod(ModContentPack content) : base(content)
        {
            GetSettings<RimKeeperAnimalsModSettings>();
            ActiveMateThresholdText = RimKeeperAnimalsModSettings.ActiveMateThreshold.ToString();
            ActiveMateThresholdWildCache = RimKeeperAnimalsModSettings.ActiveMateThresholdWild;
            ActiveMateThresholdFactionCache = RimKeeperAnimalsModSettings.ActiveMateThresholdFaction;
        }

        public void Init() => mapAnimalsCount = RimKeeperAnimalsModSettings.MateCount();

        public override string SettingsCategory() => "RK Wild Animal Procreation";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            RimKeeperAnimalsModSettings.DoWindowContents(()=>Init());
            if (ActiveMateThresholdWildCache != RimKeeperAnimalsModSettings.ActiveMateThresholdWild)
            {
                ActiveMateThresholdWildCache = RimKeeperAnimalsModSettings.ActiveMateThresholdWild;
                Init();
            }
            if (ActiveMateThresholdFactionCache != RimKeeperAnimalsModSettings.ActiveMateThresholdFaction)
            {
                ActiveMateThresholdFactionCache = RimKeeperAnimalsModSettings.ActiveMateThresholdFaction;
                Init();
            }

            Listing_Standard listingStandard = new Listing_Standard();
            Rect newRect = new Rect(inRect.x, inRect.y, inRect.width / 2, inRect.height);
            listingStandard.Begin(newRect);

            listingStandard.CheckboxLabeled("Debug Log", ref RimKeeperAnimalsModSettings.DebugLog, "Log Messages");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Active wild animal procreation", ref RimKeeperAnimalsModSettings.ActiveMateWild, "Wild animals that do not belong to your faction will breed with other animals.");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Active egg incubation", ref RimKeeperAnimalsModSettings.ActiveEggIncubation, "The logic of animals incubating eggs to protect them from external factors, that slow they deterioration rate.");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Active egg incumation protection", ref RimKeeperAnimalsModSettings.ActiveEggIncubationProtect, "An animals with wildness, will attack any other race/faction that comes too close while incubating their eggs.");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Active egg lay logic", ref RimKeeperAnimalsModSettings.ActiveEggLayLogic, "Expansion logic for egg laying. Animals will try to lay egg in place to reduce deterioration rate by environment.");
            listingStandard.Gap();

            listingStandard.Label($"Animal procreation threshold (detected:{mapAnimalsCount})");
            listingStandard.IntEntry(ref RimKeeperAnimalsModSettings.ActiveMateThreshold, ref ActiveMateThresholdText, 1);
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Inclode wild animals in mate threshold", ref RimKeeperAnimalsModSettings.ActiveMateThresholdWild, "");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Inclode fication animals in mate threshold", ref RimKeeperAnimalsModSettings.ActiveMateThresholdFaction, "");
            listingStandard.Gap();

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}