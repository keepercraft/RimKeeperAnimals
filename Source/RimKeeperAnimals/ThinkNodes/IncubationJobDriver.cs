using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using Keepercraft.RimKeeperAnimals.Helpers;
using System.Linq;
using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Models;
using System.Threading;
using System.Threading.Tasks;
using Verse.Sound;

namespace Keepercraft.RimKeeperAnimals.ThinkNodes
{
    public class IncubationJobDriver : JobDriver_LayDown
    {
        public string name_root = nameof(IncubationJobDriver);

        private const int TicksPerHour = 2500;
        private const float TargetHungerLevel = 0.30f;
        private const int tickActionInterval = TicksPerHour / 2;

        private const float IncubationAggroActive = 6f;
        private const int IncubationAggroDelay = 2000;

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
                if (pawn.IsHashIntervalTick(tickActionInterval))
                {
                    //DebugHelper.Message("IncubationJobDriver {0} tickAction", pawn.ToString());
                    if(job != null)
                    {
                        if (!pawn.Position.GetThingList(pawn.Map).Any(c => c.def.IsEgg))
                        {
                            DebugHelper.Message("IncubationJobDriver {0} no egg", pawn.ToString());
                            EndJobWith(JobCondition.InterruptForced);
                        }
                        if (
                            pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry ||
                            PawnUtility.EnemiesAreNearby(pawn, 10) ||
                            (job.targetA != null &&!job.targetA.Thing.def.IsEgg))
                        {
                            DebugHelper.Message("IncubationJobDriver {0} stop", pawn.ToString());
                            EndJobWith(JobCondition.InterruptForced);
                            DebugHelper.Message("IncubationJobDriver {0} stop GO", pawn.ToString());

                        }
                        if (RimKeeperAnimalsModSettings.ActiveEggIncubationProtect 
                            && pawn.RaceProps.wildness > 0f
                            && job.targetA != null
                            && pawn.Position == job.targetA.Cell)
                        {
                            var pawnOther = pawn.FindUndesirablePawn(IncubationAggroActive);
                            if (pawnOther != null)
                            {
                                DebugHelper.Message("IncubationJobDriver {0} findOtherPawn {1}", pawn.ToString(), pawnOther.ToString());

                                int num = pawn.ageTracker.CurLifeStageIndex;
                                if (pawn.RaceProps.lifeStageAges[num]?.soundAngry != null)
                                {
                                    LifeStageUtility.PlayNearestLifestageSound(pawn, (LifeStageAge lifeStage) => lifeStage.soundAngry, null, (MutantDef mutant) => mutant.soundAngry, 1f);
                                }
                                else
                                {
                                    DebugHelper.Message("Dont find soundAngry");
                                    SoundInfo info = SoundInfo.InMap(new TargetInfo(pawn.PositionHeld, pawn.MapHeld, false), MaintenanceType.None);
                                    info.pitchFactor = 1f;
                                    info.volumeFactor = 1f;
                                    pawn.RaceProps.soundMeleeHitPawn?.PlayOneShot(info);
                                }

                                MoteMaker.MakeColonistActionOverlay(pawn, ThingDefOf.Mote_ColonistFleeing);

                                //IntVec3 directionToTarget = pawnOther.Position - pawn.Position;
                                //pawn.Rotation = Rot4.FromAngleFlat(directionToTarget.ToVector3().AngleFlat());
                                
                                if (!pawnOther.InMentalState &&
                                    !(pawn.CurJob != null && pawnOther.CurJob.def == JobDefOf.Hunt))
                                {
                                    if (!pawnOther.Drafted)
                                    {
                                        IntVec3 intVec = CellFinderLoose.GetFleeDest(pawnOther, new List<Thing>() { pawn });
                                        if (intVec != pawnOther.Position)
                                        {
                                            Job job2 = JobMaker.MakeJob(JobDefOf.Flee, intVec, pawn);
                                            pawnOther.jobs.TryTakeOrderedJob(job2);
                                        }
                                    }

                                    Task.Run(() =>
                                    {
                                        Thread.Sleep(IncubationAggroDelay);
                                        if (pawn.CalculateDistanceBetweenPawns(pawnOther) <= IncubationAggroActive)
                                        {
                                            DebugHelper.Message("IncubationJobDriver {0} start attack {1}", pawn.ToString(), pawnOther.ToString());
                                            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, pawnOther);
                                            //job.startTick = Find.TickManager.TicksGame + 1000;
                                            job.maxNumMeleeAttacks = Rand.Range(2, 6);
                                            job.expiryInterval = Rand.Range(600, 1000);
                                            job.followRadius = IncubationAggroActive;
                                            pawn.jobs.TryTakeOrderedJob(job);
                                            EndJobWith(JobCondition.Succeeded);
                                            MoteMaker.MakeColonistActionOverlay(pawn, ThingDefOf.Mote_ColonistAttacking);
                                        }
                                    });
                                }
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

                    DebugHelper.Message("IncubationJobDriver {0} end tick", pawn.ToString());
                }
            };
            yield return waitToil;
            yield return sleepToil;
        }
    }
}
