using HarmonyLib;
using RimWorld;
using Verse.AI;
using Verse;
using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.ThinkNodes;
using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Models;

namespace Keepercraft.RimKeeperAnimals.Patches
{
    [HarmonyPatch(typeof(JobGiver_GetRest), "TryGiveJob")]
    public static class AnimalSleepMod_Patch
    {
        static bool Prefix(JobGiver_LayEgg __instance, Pawn pawn, ref Job __result)
        {
            if (!RimKeeperAnimalsModSettings.ActiveEggIncubation) return true;
            //if (!pawn.RaceProps.Animal) return true;
            if (!pawn.gender.HasFlag(Gender.Female)) return true;
            if (!pawn.HasComp<CompEggLayer>()) return true;

            var egg = pawn.Incubation_Egg();
            if (egg == null) return true;

            DebugHelper.Message("Incubation {0} on {1}", pawn.LabelCap, egg.Position.ToString());
            var jobdef = DefDatabase<JobDef>.GetNamed(nameof(IncubationJobDriver));
            var job = JobMaker.MakeJob(jobdef, egg);
            job.expireOnEnemiesNearby = true;
            __result = job;
            return false;
        }
    }
}
