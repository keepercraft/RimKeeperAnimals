using HarmonyLib;
using RimWorld;
using Verse.AI;
using Verse;
using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Models;

namespace Keepercraft.RimKeeperAnimals.Patches
{
    [HarmonyPatch(typeof(JobGiver_LayEgg), "TryGiveJob")]
    public static class TryGiveJob_JobGiver_LayEgg_Patch
    {
        static bool Prefix(JobGiver_LayEgg __instance, Pawn pawn, ref Job __result)
        {
            if (!RimKeeperAnimalsModSettings.ActiveEggLayLogic) return true;
            var compEggLayer = pawn.TryGetComp<CompEggLayer>();
            if (compEggLayer == null || !compEggLayer.CanLayNow) return false;

            ThingDef singleDef = compEggLayer.NextEggType();
            PathEndMode peMode = PathEndMode.OnCell;
            TraverseParms traverseParms = TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn, false, false, false);
            if (pawn.Faction == Faction.OfPlayer)
            {
                Thing bestEggBox = __instance.GetPrivateMethod<Thing>("GetBestEggBox", pawn, peMode, traverseParms);
                if (bestEggBox != null)
                {
                    __result = JobMaker.MakeJob(JobDefOf.LayEgg, bestEggBox);
                    return false;
                }
            }

            var eggLayerProps = pawn.kindDef.race.GetCompProperties<CompProperties_EggLayer>();
            if (eggLayerProps == null) return true;
            var cell = pawn.FindCellRoofed(eggLayerProps.eggFertilizedDef);
            if (cell == null) return true;
            DebugHelper.Message("LayEgg: {0}", cell.ToString());
            __result = new Job(JobDefOf.LayEgg, (IntVec3)cell);
            return false;
        }
    }
}
