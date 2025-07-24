using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace Keepercraft.RimKeeperAnimals.ThinkNodes
{
    public class WildManBabyCarryToMomJobGiver : ThinkNode_JobGiver
    {
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
                //DebugHelper.Message("BABY: {0} : {1}", pawn3.LabelCap, pawn3.ageTracker?.CurLifeStage.ToString());
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
                30f,
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
                //DebugHelper.Message("BABY: {0} : {1}", pawn3.LabelCap, pawn3.ageTracker?.CurLifeStage.ToString());
                return //pawn3.CanCasuallyInteractNow(false, false, false, false) &&
                    !pawn3.IsForbidden(pawn) &&
                    pawn3.IsWildMan() &&
                    pawn3.health.hediffSet.HasHediff(HediffDefOf.Lactating, false);
            };
            Pawn pawn4 = (Pawn)GenClosest.ClosestThingReachable(
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
            if (pawn4 == null) return null;

            DebugHelper.Message("WildManBabyCarryToMomJobGiver: {0} -> {1} -> {2}", pawn.LabelCap, pawn2.LabelCap, pawn4.LabelCap);
            Job job = JobMaker.MakeJob(JobDefOf.BreastfeedCarryToMom, pawn2, pawn4);
            job.count = 1;
            return job;
        }
    }

    public class WildManBabyCareJobGiver : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!RimKeeperAnimalsModSettings.ActiveMateWildMan) return null;
            if (pawn.health.Downed || pawn.health.Dead || pawn.health.Downed) return null;
            if (pawn.CurJobDef == JobDefOf.Breastfeed) return null;
            // if (pawn.CurJob != null) return null;
            if (!pawn.IsWildMan()) return null;
            //  if (!pawn.gender.HasFlag(Gender.Female)) return null;
            if (!pawn.health.hediffSet.HasHediff(HediffDefOf.Lactating, false)) return null;
            if (PawnUtility.EnemiesAreNearby(pawn, 10)) return null;

            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn3 = t as Pawn;
                //DebugHelper.Message("BABY: {0} : {1}", pawn3.LabelCap, pawn3.ageTracker?.CurLifeStage.ToString());
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
                30f,
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

            //var job = JobMaker.MakeJob(JobDefOf.BringBabyToSafety, pawn2);
            var job = JobMaker.MakeJob(JobDefOf.Breastfeed, pawn2);
            job.count = 1;
            //var jobdef = DefDatabase<JobDef>.GetNamed(nameof(WildManBabyCareJobDriver));
            //var job = JobMaker.MakeJob(jobdef, pawn2);
            //job.expireOnEnemiesNearby = true;
            return job;
        }

        // CarryToBreastfeed
        // JobGiver_PickUpBaby
        // JobDriver_Breastfeed
        // JobGiver_CareForChild
    }

    public class ThinkNode_ChancePerHour_Wait : ThinkNode_ChancePerHour
    {
        protected override float MtbHours(Pawn pawn)
        {
            return 0.5f;
        }
    }

}
