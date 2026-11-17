using HarmonyLib;
using RimWorld;
using Verse.AI;
using Verse;
using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Models;
namespace Keepercraft.RimKeeperAnimals.Patches;

[HarmonyPatch(typeof(JobGiver_GetRest), "TryGiveJob")]
public static class AnimalSleepMod_Patch
{
    static bool Prefix(JobGiver_GetRest __instance, Pawn pawn, ref Job __result)
    {
        if (!RimKeeperAnimalsModSettings.ActiveEggIncubation) return true;
        if (pawn.jobs == null || pawn.jobs.curJob != null) return true;
        if (pawn.IncubationInValid()) return true;

        var egg = pawn.FindIncubation_Egg();
        if (egg == null) return true;

        DebugHelper.Message("Incubation GetRest {0} on {1}", pawn.LabelCap, egg.Position.ToString());
        pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.LayDown, egg.Position, pawn));
        return false;
    }
}