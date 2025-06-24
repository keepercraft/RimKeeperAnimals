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
            if (pawn.health.Downed || pawn.health.Dead || pawn.Drafted || pawn.jobs != null) return null;
            if (!RimKeeperAnimalsModSettings.ActiveEggIncubation) return null;
            if (!pawn.gender.HasFlag(Gender.Female)) return null;
            if (!pawn.HasComp<CompEggLayer>()) return null;
            if (pawn.health.summaryHealth.SummaryHealthPercent < 0.5) return null;
            if (pawn.needs.food.CurLevelPercentage <= pawn.needs.food.PercentageThreshHungry) return null;
            if (PawnUtility.EnemiesAreNearby(pawn, 10)) return null;

            //int tick = Find.TickManager.TicksGame;
            //if ((tick - lastTick) < TicksPerHour) return null;
            //lastTick = tick;

            //if (!pawn.IsHashIntervalTick(TicksPerHour / 2)) return null;
            var egg = pawn.Incubation_Egg();
            if (egg == null) return null;
            
            DebugHelper.Message("Incubation {0} on {1}", pawn.LabelCap, egg.Position.ToString());
            var jobdef = DefDatabase<JobDef>.GetNamed(nameof(IncubationJobDriver));
            var job = JobMaker.MakeJob(jobdef, egg);
            job.expireOnEnemiesNearby = true;
            return job;
        }
    }
}
