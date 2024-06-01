using Verse.AI;
using Verse;
using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Models;
using RimWorld;

namespace Keepercraft.RimKeeperAnimals.ThinkNodes
{
    public class Incubation_JobGiver : ThinkNode_JobGiver
    {
        private const int TicksPerHour = 50;
        private int lastTick = 0;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!RimKeeperAnimalsModSettings.ActiveEggIncubation) return null;
            if (!pawn.gender.HasFlag(Gender.Female)) return null;
            if (!pawn.HasComp<CompEggLayer>()) return null;

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
