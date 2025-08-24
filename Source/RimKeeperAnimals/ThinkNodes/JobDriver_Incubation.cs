using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Keepercraft.RimKeeperAnimals.ThinkNodes
{
    public class IncubationJobDriver : JobDriver
    {
        public string name_root = nameof(IncubationJobDriver);
        public const int CheckInterval = 60; // co 1 sekundę
        public const float AggroActive = 5f;
        public const int AggroActivationDelay = 3;
        protected int AggroActivationLastTick = 0;
        public const int AggroDelay = 5;
        protected int IncubationAggroLastTick = 0;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (job.targetA == null) return false;
            if (!job.targetA.IsValid) return false;
            // DebugHelper.Message("IncubationJobDriver {0} TryMakePreToilReservations:{1}", pawn.ToString(), errorOnFailed);
            return pawn.Reserve(job.targetA, job, 1, -1, null, false);
        }

        public static bool IncubationValid(Pawn pawn, Job job = null)
        {
            try
            {
                if (pawn == null) return true;
                if (pawn.health.Downed || pawn.health.Dead || pawn.health.Downed) return true;
                if (!pawn.gender.HasFlag(Gender.Female)) return true;
                if (pawn.GetComp<CompEggLayer>()?.CanLayNow ?? false) return true;
                if (pawn.health.hediffSet.GetInjuredParts().Any()) return true;
                if (pawn.health.summaryHealth.SummaryHealthPercent < 0.8f) return true;
                if (pawn.needs?.food?.CurLevelPercentage <= pawn.needs.food.PercentageThreshHungry) return true;
                if (PawnUtility.EnemiesAreNearby(pawn, 10)) return true;
                if (job != null && pawn.Map != null)
                {
                    if ((job.targetA.Cell.GetFirstPawn(pawn.Map) ?? pawn) != pawn) return true;
                    if (job.targetA.Cell.GetThingList(pawn.Map)?.Any(t => t.def.IsShell) ?? false) return true;
                }
            }
            catch (System.Exception ex)
            {
                DebugHelper.Message($"ERROR IncubationValid {ex.Message}");
            }
            return false;
        }

        protected void TryScareAwayAnimation()
        {
            try
            {
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
            }
            catch (System.Exception ex)
            {
                DebugHelper.Message("[ERROR CATCH] IncubationJobDriver PlayNearestLifestageSound {0}", ex.InnerException);
                return;
            }
            MoteMaker.MakeColonistActionOverlay(pawn, ThingDefOf.Mote_ColonistFleeing);
        }

        protected bool TrySlep()
        {
            if((pawn.needs?.rest?.CurLevelPercentage??0) > 0.5 || !RestUtility.CanFallAsleep(pawn)) return false;
            pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.LayDown, pawn.Position, pawn));
            return true;
        }

        protected void TryScareAwayAction(Pawn pawnOther)
        {
            if (pawnOther.Drafted) return;            
            IntVec3 intVec = CellFinderLoose.GetFleeDest(pawnOther, new List<Thing>() { pawn });
            if (intVec == pawnOther.Position)  return; 
            Job job2 = JobMaker.MakeJob(JobDefOf.Flee, intVec, pawn);
            pawnOther.jobs.TryTakeOrderedJob(job2);
        }

        protected bool TryAttackAction(Pawn pawnOther)
        {
            if (pawn.CalculateDistanceBetweenPawns(pawnOther) > AggroActive - 1) return false;          
            DebugHelper.Message("IncubationJobDriver {0} start attack {1}", pawn.ToString(), pawnOther.ToString());
            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, pawnOther);
            //job.startTick = Find.TickManager.TicksGame + 1000;
            job.maxNumMeleeAttacks = Rand.Range(2, 6);
            job.expiryInterval = Rand.Range(600, 1000);
            job.followRadius = AggroActive - 1;
            pawn.jobs.TryTakeOrderedJob(job);
            EndJobWith(JobCondition.Succeeded);
            MoteMaker.MakeColonistActionOverlay(pawn, ThingDefOf.Mote_ColonistAttacking);  
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnCatch(() => job == null || job.targetA == null, "JOB NULL");
            Toil incubate = new Toil
            {
                defaultDuration = 5000,
                defaultCompleteMode = ToilCompleteMode.Delay,
                initAction = () => pawn.pather.StartPath(job.targetA, PathEndMode.OnCell),
                tickAction = () =>
                {
                    try
                    {
                        if (Find.TickManager.TicksGame % CheckInterval != 0) return;
                        if (IncubationValid(pawn, job))
                        {
                            EndJobWith(JobCondition.InterruptForced);
                            return;
                        }

                        if(TrySlep()) return;

                        if (!RimKeeperAnimalsModSettings.ActiveEggIncubationProtect) return;
                        if (pawn.Position != job.targetA.Cell) return;
#if RW15
                        if (pawn.GetStatValue(StatDefOf.MinimumHandlingSkill) < 0.1f) return;
#elif RW16
                        if (StatDefOf.Wildness.Worker.GetValue(pawn) <= 0.0f) return;
#endif
                        var pawnOther = pawn.FindUndesirablePawn(AggroActive);
                        if (pawnOther == null)
                        {
                            AggroActivationLastTick = 0;
                            IncubationAggroLastTick = 0;
                            return;
                        }

                        var curDriver = pawnOther.jobs?.curDriver;
                        if (curDriver == null) return;
                        if (curDriver is IncubationJobDriver) return;
                        if (curDriver is JobDriver_LayDown) return;
                        if (curDriver is JobDriver_LayEgg) return;
                        if (curDriver is JobDriver_GoForWalk) return;
                        if (curDriver is JobDriver_Flee) return;
                        if (curDriver is JobDriver_LayDownResting) return;
                        if (curDriver is JobDriver_Lovin) return;

                        TryScareAwayAnimation();

                        if (pawnOther.InMentalState) return;
                        if (pawnOther.CurJob != null && pawnOther.CurJob.def == JobDefOf.Hunt) return;
                        if (pawn.CurJob == null) return;
                        
                        if(pawn.BodySize * 3 < pawnOther.BodySize)
                        {
                            pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Flee));
                            EndJobWith(JobCondition.InterruptForced);
                            return;
                        }
                        TryScareAwayAction(pawnOther);

                        if (++AggroActivationLastTick < AggroActivationDelay) return;
                        AggroActivationLastTick = 0;
                        if (++IncubationAggroLastTick < AggroDelay) return;
                        //if (Find.TickManager.TicksGame - IncubationAggroLastTick < IncubationAggroDelay) return; // 10s
                        if (pawnOther.CurJob != null && pawnOther.CurJob.def == JobDefOf.Flee) return;
                        if (TryAttackAction(pawnOther)) IncubationAggroLastTick = 0;
                    }
                    catch (System.Exception ex)
                    {
                        DebugHelper.Message($"ERROR Toil {ex.Message}");
                        EndJobWith(JobCondition.InterruptForced);
                        return;
                    }
                }
            };
            incubate.AddFinishAction(() => DebugHelper.Message("JobDriver_Incubation finish: {0} on {1}", pawn.LabelShortCap, pawn.Position.ToString()));

            yield return incubate;
        }
    }

}
