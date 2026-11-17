using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Models;
using Verse;
using Verse.AI;
namespace Keepercraft.RimKeeperAnimals.ThinkNodes;

public class Incubation_JobGiver : ThinkNode_JobGiver
{
    private uint tickThreshold = 0;
    protected override Job TryGiveJob(Pawn pawn)
    {
        try
        {
            if (!RimKeeperAnimalsModSettings.ActiveEggIncubation) return null;
            //if (Find.TickManager.TicksGame % 3 != 0) return null;
            if (pawn.jobs == null || pawn.jobs.curJob != null) return null;
            if (pawn.IncubationInValid()) return null;
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
            var egg = pawn.FindIncubation_Egg();
            if (egg == null) return null;
            //if (!pawn.CanReach(egg, PathEndMode.OnCell, Danger.None)) return null;
            //if (!pawn.CanReserve(egg, 1, -1, null, false)) return null;
            //DebugHelper.Message("Incubation_JobGiver: {0} {1}", pawn.ThingID, pawn.Position.ToString());
            //DebugHelper.Message("Incubation ThinkNode {0} on {1}", pawn.LabelCap, egg.Position.ToString());
            var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(nameof(IncubationJobDriver)), egg);
            job.expireOnEnemiesNearby = true;
            return job;
        }
        catch (System.Exception ex)
        {
            return null;
        }
    }
}
