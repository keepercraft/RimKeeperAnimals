using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using RimWorld;
using System;
using Verse;
using Verse.AI;
namespace Keepercraft.RimKeeperAnimals.ThinkNodes;

public class WildManBabyCareJobGiver : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!RimKeeperAnimalsModSettings.ActiveMateWildMan) return null;

        if (pawn.health.Downed || pawn.health.Dead || pawn.health.Downed) return null;
        if (pawn.CurJobDef == JobDefOf.Breastfeed) return null;
        if (!pawn.IsWildMan()) return null;
        if (FeedPatientUtility.IsHungry(pawn)) return null;
        if (!pawn.health.hediffSet.HasHediff(HediffDefOf.Lactating, false)) return null;
        if (PawnUtility.EnemiesAreNearby(pawn, 10)) return null;

        Predicate<Thing> validator = delegate (Thing t)
        {
            Pawn pawn3 = t as Pawn;
            return //pawn3.CanCasuallyInteractNow(false, false, false, false) &&
                !pawn3.IsForbidden(pawn) &&
                pawn3.IsWildMan() &&
                FeedPatientUtility.IsHungry(pawn3) &&
                pawn3.ageTracker?.CurLifeStage == LifeStageDefOf.HumanlikeBaby;
            //FertileMateTarget_X(pawn, pawn3);
        };
        Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(
            pawn.Position,
            pawn.Map,
            ThingRequest.ForDef(pawn.def),
            PathEndMode.Touch,
            TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false),
            9999f,
            validator,
            null,
            0,
            -1,
            false,
            RegionType.Set_Passable,
            false);

        //DebugHelper.Message("WildManBabyCareJobGiver: {0} -> {1}", pawn.LabelCap, pawn2?.LabelCap ?? "----");
        if (pawn2 == null) return null;
        DebugHelper.Message("WildManBabyCareJobGiver: {0} -> {1}", pawn.LabelCap, pawn2.LabelCap);

        var job = JobMaker.MakeJob(JobDefOf.Breastfeed, pawn2);
        job.count = 1;
        return job;
    }
}