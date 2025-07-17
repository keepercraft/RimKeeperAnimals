using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using RimWorld;
using Verse;
using Verse.AI;

namespace Keepercraft.RimKeeperAnimals.ThinkNodes
{
    public class Incubation_JobGiver : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            //DebugHelper.Message("Incubation_JobGiver {0} #1", pawn.LabelCap);

            if (pawn.health.Downed || pawn.health.Dead || pawn.Drafted || pawn.jobs == null) return null;
           // DebugHelper.Message("Incubation_JobGiver {0} #2", pawn.LabelCap);

            if (!RimKeeperAnimalsModSettings.ActiveEggIncubation) return null;
          //  DebugHelper.Message("Incubation_JobGiver {0} #3", pawn.LabelCap);

            if (!pawn.gender.HasFlag(Gender.Female)) return null;
         //   DebugHelper.Message("Incubation_JobGiver {0} #4", pawn.LabelCap);

            if (!pawn.HasComp<CompEggLayer>()) return null;
         //   DebugHelper.Message("Incubation_JobGiver {0} #5", pawn.LabelCap);

            if (pawn.health.summaryHealth.SummaryHealthPercent < 0.5) return null;
         //   DebugHelper.Message("Incubation_JobGiver {0} #6", pawn.LabelCap);

            if (pawn.needs.food.CurLevelPercentage <= pawn.needs.food.PercentageThreshHungry) return null;
         //   DebugHelper.Message("Incubation_JobGiver {0} #7", pawn.LabelCap);

            if (PawnUtility.EnemiesAreNearby(pawn, 10)) return null;
            //   DebugHelper.Message("Incubation_JobGiver {0} #8", pawn.LabelCap);

            if (pawn?.jobs != null &&
                (pawn.jobs.curJob != null || pawn.jobs.jobQueue?.Count > 0 || pawn.jobs.curDriver != null))
            {
                return null;
            }

            CompProperties_EggLayer compProperties = pawn.def.GetCompProperties<CompProperties_EggLayer>();
            if (compProperties != null)
            {
                var eggDef = compProperties.eggFertilizedDef;
                if (pawn.Position.GetThingList(pawn.Map).Any(t => t.def == eggDef))
                {
                    return null;
                }
            }

            //int tick = Find.TickManager.TicksGame;
            //if ((tick - lastTick) < TicksPerHour) return null;
            //lastTick = tick;

            //if (!pawn.IsHashIntervalTick(TicksPerHour / 2)) return null;
            var egg = pawn.Incubation_Egg();
            if (egg == null) return null;


           // DebugHelper.Message("JOB:{0} - DEF:{1}", pawn.jobs.curJob.GetType().FullName, pawn.CurJobDef.GetType().FullName);

            DebugHelper.Message("Incubation JOB {0} on {1}", pawn.LabelCap, egg.Position.ToString());
            var jobdef = DefDatabase<JobDef>.GetNamed(nameof(IncubationJobDriver));
            var job = JobMaker.MakeJob(jobdef, egg);
            job.expireOnEnemiesNearby = true;
            return job;
        }
    }
}
