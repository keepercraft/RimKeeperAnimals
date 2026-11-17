using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using RimWorld;
using System;
using Verse;
using Verse.AI;
namespace Keepercraft.RimKeeperAnimals.ThinkNodes;

public class WildManBabyCarryToMomJobGiver : ThinkNode_JobGiver
{
    public static float lastTime;
    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!RimKeeperAnimalsModSettings.ActiveMateWildMan) return null;

        if (pawn.health.Downed || pawn.health.Dead || pawn.health.Downed) return null;
        if (!pawn.IsWildMan()) return null;

        if (pawn.health.hediffSet.HasHediff(HediffDefOf.Lactating, false)) return null;
        if (PawnUtility.EnemiesAreNearby(pawn, 10)) return null;

        Predicate<Thing> validator = delegate (Thing t)
        {
            Pawn pawn3 = t as Pawn;
            return !pawn3.IsForbidden(pawn) &&
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
        if (pawn2 == null) return null;

        Predicate<Thing> validator2 = delegate (Thing t)
        {
            Pawn pawn3 = t as Pawn;
            return !pawn3.IsForbidden(pawn) &&
                pawn3.IsWildMan() &&
                pawn3.CurJobDef != JobDefOf.Breastfeed &&
                !FeedPatientUtility.IsHungry(pawn3) &&
                pawn3.health.hediffSet.HasHediff(HediffDefOf.Lactating, false);
        };
        Pawn pawn4 = (Pawn)GenClosest.ClosestThingReachable(
            pawn.Position,
            pawn.Map,
            ThingRequest.ForDef(pawn.def),
            PathEndMode.Touch,
            TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false),
            9999f,
            validator2,
            null,
            0,
            -1,
            false,
            RegionType.Set_Passable,
            false);
        if (pawn4 == null) return null;

        DebugHelper.Message("WildManBabyCarryToMomJobGiver: {0} -> {1} -> {2}", pawn.LabelCap, pawn2?.LabelCap ?? "--", pawn4?.LabelCap ?? "--");
        Job job = JobMaker.MakeJob(JobDefOf.BreastfeedCarryToMom, pawn2, pawn4);
        job.count = 1;
        return job;
    }
}