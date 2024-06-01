using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using Keepercraft.RimKeeperAnimals.Helpers;
using System.Linq;
using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Models;

namespace Keepercraft.RimKeeperAnimals.ThinkNodes
{
    public class IncubationJobDriver : JobDriver_LayDown
    {
        public string name_root = nameof(IncubationJobDriver);

        private const int TicksPerHour = 2500;
        private const float TargetHungerLevel = 0.30f;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //DebugHelper.Message("IncubationJobDriver {0} TryMakePreToilReservations:{1}", pawn.ToString(), errorOnFailed);
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //DebugHelper.Message("IncubationJobDriver {0} MakeNewToils", pawn.ToString());
            Toil sleepToil = Toils_LayDown.LayDown(TargetIndex.A, false, this.LookForOtherJobs, true, true, PawnPosture.Standing, false);
            Toil waitToil = new Toil();
            waitToil.defaultCompleteMode = ToilCompleteMode.Never;
            waitToil.initAction = () =>
            {
                pawn.pather.StartPath(job.targetA, PathEndMode.OnCell);
            };
            waitToil.tickAction = () =>
            {
                if (pawn.IsHashIntervalTick(TicksPerHour / 2))
                {
                    //DebugHelper.Message("IncubationJobDriver {0} tickAction", pawn.ToString());
                    if(job != null)
                    {
                        if (
                            pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry ||
                            PawnUtility.EnemiesAreNearby(pawn, 10) ||
                            (job.targetA != null &&!job.targetA.Thing.def.IsEgg))
                        {
                            DebugHelper.Message("IncubationJobDriver {0} stop", pawn.ToString());
                            EndJobWith(JobCondition.InterruptForced);
                        }
                        if (RimKeeperAnimalsModSettings.ActiveEggIncubationProtect 
                            && pawn.RaceProps.wildness > 0f
                            && job.targetA != null
                            && pawn.Position == job.targetA.Cell)
                        {
                            var pawnOther = pawn.FindUndesirablePawn(3);
                            if (pawnOther != null)
                            {
                                DebugHelper.Message("IncubationJobDriver {0} findOtherPawn {1}", pawn.ToString(), pawnOther.ToString());
                                EndJobWith(JobCondition.Succeeded);

                                IntVec3 intVec = CellFinderLoose.GetFleeDest(pawnOther, new List<Thing>() { pawn });
                                if (intVec != pawnOther.Position)
                                {
                                    Job job2 = JobMaker.MakeJob(JobDefOf.Flee, intVec, pawn);
                                    pawnOther.jobs.TryTakeOrderedJob(job2);
                                }

                                Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, pawnOther);
                                job.maxNumMeleeAttacks = 6;
                                job.expiryInterval = 500;
                                job.followRadius = 6;
                                pawn.jobs.TryTakeOrderedJob(job);
                            }
                        }
                    }
                    if (pawn.needs.rest.CurLevelPercentage < 0.5f)
                    {
                        DebugHelper.Message("IncubationJobDriver {0} sleep", pawn.ToString());
                        JumpToToil(sleepToil);
                    }
                    CompEggLayer compEggLayer = pawn.TryGetComp<CompEggLayer>();
                    if (compEggLayer != null && compEggLayer.CanLayNow)
                    {
                        DebugHelper.Message("IncubationJobDriver {0} layegg", pawn.ToString());
                        EndJobWith(JobCondition.Succeeded);
                    }
                    if (pawn.Position.GetThingList(pawn.Map).Any(a => (a is Pawn p) ? pawn != p : false))
                    {
                        DebugHelper.Message("IncubationJobDriver {0} space in use", pawn.Position);
                        EndJobWith(JobCondition.InterruptForced);
                    }
                }
            };
            yield return waitToil;
            yield return sleepToil;
        }
    }
}
