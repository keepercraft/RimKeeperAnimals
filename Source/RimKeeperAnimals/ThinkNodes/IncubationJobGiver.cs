using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using Verse;
using Verse.AI;

namespace Keepercraft.RimKeeperAnimals.ThinkNodes
{
    public class Incubation_JobGiver : ThinkNode_JobGiver
    {
        private uint tickThreshold = 0;
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!RimKeeperAnimalsModSettings.ActiveEggIncubation) return null;
            //if (Find.TickManager.TicksGame % 3 != 0) return null;
            if (pawn.jobs == null || pawn.jobs.curJob != null) return null;
            if (IncubationJobDriver.IncubationValid(pawn)) return null;
            //DebugHelper.Message("Incubation ThinkNode {0} tickThreshold {1}", pawn.LabelCap, tickThreshold);
            if (++tickThreshold < 4) return null; else tickThreshold = 0;
            //CompProperties_EggLayer compProperties = pawn.def.GetCompProperties<CompProperties_EggLayer>();
            //if (compProperties != null)
            //{
            //    var eggDef = compProperties.eggFertilizedDef;
            //    if (pawn.Position.GetThingList(pawn.Map).Any(t => t.def == eggDef))
            //    {
            //        return null;
            //    }
            //}
            var egg = pawn.Incubation_Egg();
            if (egg == null) return null;
            if (!pawn.CanReserve(egg, 1, -1, null, false)) return null;
            DebugHelper.Message("Incubation ThinkNode {0} on {1}", pawn.LabelCap, egg.Position.ToString());
            var jobdef = DefDatabase<JobDef>.GetNamed(nameof(IncubationJobDriver));
            var job = JobMaker.MakeJob(jobdef, egg);
            job.expireOnEnemiesNearby = true;
            return job;
        }
    }
}
