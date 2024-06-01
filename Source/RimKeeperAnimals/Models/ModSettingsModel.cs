using Verse;

namespace Keepercraft.RimKeeperAnimals.Models
{
    public class RimKeeperAnimalsModSettings : ModSettings
    {
        public static bool DebugLog = false;
        public static bool ActiveMateWild = true;
        public static bool ActiveEggIncubation = true; 
        public static bool ActiveEggIncubationProtect = true;
        public static bool ActiveEggLayLogic = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DebugLog, nameof(DebugLog), false);
            Scribe_Values.Look(ref ActiveMateWild, nameof(ActiveMateWild), true);
            Scribe_Values.Look(ref ActiveEggIncubation, nameof(ActiveEggIncubation), true);
            Scribe_Values.Look(ref ActiveEggIncubationProtect, nameof(ActiveEggIncubationProtect), true);
            Scribe_Values.Look(ref ActiveEggLayLogic, nameof(ActiveEggLayLogic), true);
        }
    }
}