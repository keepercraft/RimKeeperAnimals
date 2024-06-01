using Keepercraft.RimKeeperAnimals.Models;
using UnityEngine;
using Verse;

namespace Keepercraft.RimKeeperAnimals
{
    public class RimKeeperAnimalsHelperMod : Mod
    {
        public RimKeeperAnimalsHelperMod(ModContentPack content) : base(content)
        {
            GetSettings<RimKeeperAnimalsModSettings>();
        }

        public override string SettingsCategory() => "RK Wild Animal Procreation";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            Rect newRect = new Rect(inRect.x, inRect.y, inRect.width / 2, inRect.height);
            listingStandard.Begin(newRect);

            listingStandard.CheckboxLabeled("Debug Log", ref RimKeeperAnimalsModSettings.DebugLog, "Log Messages");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Active wild animal reproduction", ref RimKeeperAnimalsModSettings.ActiveMateWild, "");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Active animal egg incubation logic", ref RimKeeperAnimalsModSettings.ActiveEggIncubation, "");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Active animal to try protect egg", ref RimKeeperAnimalsModSettings.ActiveEggIncubationProtect, "");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Active animal egg logic", ref RimKeeperAnimalsModSettings.ActiveEggLayLogic, "");
            listingStandard.Gap();

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}