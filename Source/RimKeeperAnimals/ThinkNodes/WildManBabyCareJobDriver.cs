using Keepercraft.RimKeeperAnimals.Helpers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using Verse;
using Verse.AI;

namespace Keepercraft.RimKeeperAnimals.ThinkNodes
{
    public class WildManBabyCareJobDef : JobDef
    {
        public WildManBabyCareJobDef()
        {
            this.carryThingAfterJob = true;
            this.sleepCanInterrupt = false;
            this.defName = nameof(WildManBabyCareJobDriver);
            this.driverClass = typeof(WildManBabyCareJobDriver);
            this.reportString = "WildManBabyCare";
        }
    }

    public class Hediff_CarryingPawn : HediffWithComps
    {
        public Pawn carriedPawn;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref carriedPawn, "carriedPawn");
        }
    }

    public class WildManBabyCareJobDriver : JobDriver_FeedBaby //JobDriver
    {
        public string name_root = nameof(WildManBabyCareJobDriver);

        protected override Toil FeedingToil { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private Pawn Baby => (Pawn)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //return pawn.Reserve(Baby, job);
            return pawn.Reserve(job.targetA, job, 1, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> FeedBaby()
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnCatch(() => job == null || job.targetA == null, "JOB NULL");
            this.FailOn(() => Baby.Destroyed || !Baby.Spawned || Baby.Dead);

            var subToil = new Toil
            {
                initAction = () =>
                {
                    if (pawn.carryTracker?.CarriedThing != null)
                    {
                       // IntVec3 dropLoc = job.GetTarget(TargetIndex.A).Cell;
                        Thing carried = pawn.carryTracker.CarriedThing;
                        pawn.carryTracker.TryDropCarriedThing(IntVec3.Zero, ThingPlaceMode.Direct, out _);
                    }

                    //DebugHelper.Message("JOB Breastfeed");
                    //var feedJob = JobMaker.MakeJob(JobDefOf.Breastfeed, Baby); //JobDriver_Breastfeed
                    //feedJob.count = 1;
                    //pawn.jobs.TryTakeOrderedJob(feedJob);
                },
                tickAction = () =>
                {
                    DebugHelper.Message("JOB tick");
                    if (pawn.CurJob == null || pawn.CurJob.def != JobDefOf.Breastfeed)
                    {
                        //if (FeedPatientUtility.IsHungry(Baby))
                        //{
                        //    DebugHelper.Message("JOB Breastfeed");
                        //    var feedJob = JobMaker.MakeJob(JobDefOf.Breastfeed, Baby); //JobDriver_Breastfeed
                        //    feedJob.count = 1;
                        //    pawn.jobs.TryTakeOrderedJob(feedJob);
                        //}
                        //else
                        //{
                        //    //pawn.jobs.curDriver.ReadyForNextToil();
                        //    DebugHelper.Message("JOB CarryingPawn");
                        //    var hediff = HediffMaker.MakeHediff(HediffDef.Named("CarryingPawn"), pawn) as Hediff_CarryingPawn;
                        //    hediff.carriedPawn = Baby;
                        //    pawn.health.AddHediff(hediff);
                        //    EndJobWith(JobCondition.Succeeded);
                        //}
                    }
                },
                //defaultCompleteMode = ToilCompleteMode.Never
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            subToil.AddFinishAction(() =>
            {
                //DebugHelper.Message("JOB END");
                //var hediff = HediffMaker.MakeHediff(HediffDef.Named("CarryingPawn"), pawn) as Hediff_CarryingPawn;
                //hediff.carriedPawn = Baby;
                //pawn.health.AddHediff(hediff);
                //EndJobWith(JobCondition.Succeeded);
            });

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, false);
            Toil toil = Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false, true, false);
            //toil.AddPreInitAction(new Action(this.CheckMakeTakeeGuest));
            toil.initAction = () => job.count = 1;
            toil.defaultDuration = 1500;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            yield return toil;
            yield return subToil;

            //this.FailOnDestroyedOrNull(TargetIndex.A);
            //this.FailOnAggroMentalStateAndHostile(TargetIndex.A);

            //yield break;


            //var pickUp = new Toil
            //{
            //    defaultCompleteMode = ToilCompleteMode.Delay,
            //    defaultDuration = 500,
            //    initAction = () =>
            //    {
            //        DebugHelper.Message("JOB CarryingPawn");
            //        var hediff = HediffMaker.MakeHediff(HediffDef.Named("CarryingPawn"), pawn) as Hediff_CarryingPawn;
            //        hediff.carriedPawn = Baby;
            //        pawn.health.AddHediff(hediff);
            //    }
            //};
            //yield return pickUp;


            //Toil toil = new Toil
            //{
            //    defaultDuration = 5000,
            //    defaultCompleteMode = ToilCompleteMode.Delay,
            //    initAction = () => pawn.pather.StartPath(job.targetA, PathEndMode.OnCell),
            //    tickAction = () =>
            //    {
            //        JobMaker.MakeJob(JobDefOf.Carried, Baby);
            //        if(pawn.CurJobDef == JobDefOf.Carried)
            //        {
            //            JobMaker.MakeJob(JobDefOf.Breastfeed, Baby);
            //            EndJobWith(JobCondition.Succeeded);
            //        }
            //    }
            //};

            //yield return toil;
        }
    }
}
