using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using RimWorld;
using System;
using Verse;
using Verse.AI;
namespace Keepercraft.RimKeeperAnimals.ThinkNodes;

public class AnimalEatAny_JobGiver : ThinkNode_JobGiver
{
    private static int lastTime = 0;
    protected override Job TryGiveJob(Pawn pawn)
    {
        try
        {
            if (!RimKeeperAnimalsModSettings.ActiveAnimalEatAny) return null;
            int currentTime = Find.TickManager.TicksGame;
            if (currentTime - lastTime < 120) return null;

            if (pawn.health.Downed || pawn.health.Dead || pawn.health.Downed) return null;
            if (pawn.needs?.food?.CurLevelPercentage > pawn.needs.food.PercentageThreshHungry) return null;
            Hediff malnutrition = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition, false);
            if (malnutrition == null) return null;

            lastTime = currentTime;

            if (malnutrition.Severity > 0.5f)
            {
                Predicate<Thing> validator2 = delegate (Thing t)
                {
                    Pawn pawn3 = t as Pawn;
                    var val =
                        pawn3 != pawn &&
                        pawn3.CanCasuallyInteractNow(false, false, false, false) &&
                        !pawn3.IsForbidden(pawn) &&
                        pawn3.RaceProps.IsFlesh;
                    //  pawn3.IngestibleNow;
                    //DebugHelper.Message("EAT ANY Hunt {2} ::: {0} look at {1} IsFlesh:{3}", pawn.LabelCap, pawn3.LabelCap, val, (pawn3?.RaceProps?.IsFlesh ?? false));
                    return val;
                };
                Thing thing2 = GenClosest.ClosestThingReachable(
                        pawn.Position,
                        pawn.Map,
                        ThingRequest.ForDef(pawn.def),
                        PathEndMode.Touch,
                        TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false),
                        30f,
                        validator2,
                        null,
                        0,
                        -1,
                        false,
                        RegionType.Set_Passable,
                        false);
                if (thing2 != null)
                {
                    DebugHelper.Message("AnimalEatAny: {0} {1} -> {2}", pawn.ThingID, pawn.Position.ToString(), thing2.LabelCap);
                    var job = JobMaker.MakeJob(JobDefOf.AttackMelee, thing2);
                    job.maxNumMeleeAttacks = Rand.Range(5, 10);
                    job.expiryInterval = Rand.Range(600, 1000);
                    return job;
                }
            }
            if (malnutrition.Severity > 0.25f)
            {

                Predicate<Thing> validator = delegate (Thing t)
                {
                    DebugHelper.Message("AnimalEatAny: {0} {1} -> {2} IngestibleNow:{3}", pawn.ThingID, pawn.Position.ToString(), t.LabelCap, t.IngestibleNow);
                    return
                        (t.def.category == ThingCategory.Item || t.def.category == ThingCategory.Plant) &&
                        t.IngestibleNow &&
                        pawn.CanReserve(t);
                };
                Thing thing = GenClosest.ClosestThingReachable(
                    pawn.Position,
                    pawn.Map,
                    ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways),
                    PathEndMode.OnCell,
                    TraverseParms.For(pawn),
                    30f,
                    validator);
                if (thing != null)
                {
                    DebugHelper.Message("AnimalEatAny: {0} {1} -> {2}", pawn.ThingID, pawn.Position.ToString(), thing.LabelCap);
                    return JobMaker.MakeJob(JobDefOf.Ingest, thing);
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}
