using Keepercraft.RimKeeperAnimals.Extensions;
using Verse;

namespace Keepercraft.RimKeeperAnimals.Models
{
    public class RimKeeperAnimalsModSettings : ModSettingsInit
    {
        public static bool DebugLog = false;
        public static bool ActiveMateWild = true;
        public static bool ActiveEggIncubation = true; 
        public static bool ActiveEggIncubationProtect = true;
        public static bool ActiveEggLayLogic = true;
        public static int ActiveMateThreshold = 100;
        public static bool ActiveMateThresholdWild = true;
        public static bool ActiveMateThresholdFaction = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DebugLog, nameof(DebugLog), false);
            Scribe_Values.Look(ref ActiveMateWild, nameof(ActiveMateWild), true);
            Scribe_Values.Look(ref ActiveEggIncubation, nameof(ActiveEggIncubation), true);
            Scribe_Values.Look(ref ActiveEggIncubationProtect, nameof(ActiveEggIncubationProtect), true);
            Scribe_Values.Look(ref ActiveEggLayLogic, nameof(ActiveEggLayLogic), true);
            Scribe_Values.Look(ref ActiveMateThreshold, nameof(ActiveMateThreshold), 100);
            Scribe_Values.Look(ref ActiveMateThresholdWild, nameof(ActiveMateThresholdWild), true);
            Scribe_Values.Look(ref ActiveMateThresholdFaction, nameof(ActiveMateThresholdFaction), true);
        }

        public static bool MateActive() => ActiveMateThreshold > MateCount();
        public static int MateCount()
        {
            return Find.Maps?.CountAnimalsOnAllMaps(
                ActiveMateThresholdWild,
                ActiveMateThresholdFaction
                ) ?? -1;
        }
    }
}