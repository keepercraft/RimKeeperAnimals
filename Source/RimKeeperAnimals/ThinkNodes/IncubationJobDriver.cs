using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;
namespace Keepercraft.RimKeeperAnimals.ThinkNodes;

public class IncubationJobDriver : JobDriver
{
    public string name_root = nameof(IncubationJobDriver);
    public const int CheckInterval = 100;
    public const float AggroActive = 5f;
    public const int AggroFleeBodySize = 3;
    protected int AggroActivationLastTick = 0;
    public const int AggroDelay = 5;
    protected int IncubationTickDuration = 3 * 60 * 60;
    protected float AggroWildnessTreshhold = 0.1f;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (job.targetA == null) return false;
        if (!job.targetA.IsValid) return false;
        // DebugHelper.Message("IncubationJobDriver {0} TryMakePreToilReservations:{1}", pawn.ToString(), errorOnFailed);
        return pawn.Reserve(job.targetA, job, 1, -1, null, false);
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
                //DebugHelper.Message("Dont find soundAngry");
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

    protected bool TrySleep()
    {
        if (pawn?.jobs == null) return false;
        if ((pawn.needs?.rest?.CurLevelPercentage ?? 1f) > 0.5f) return false;
        //if (pawn?.needs?.rest.CurCategory == RestCategory.Rested) return false;
        //if (!RestUtility.CanFallAsleep(pawn)) return false;
        return pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.LayDown, pawn.Position, pawn));
    }

    protected void TryScareAwayAction(Pawn pawnOther)
    {
        if (pawnOther.Drafted) return;            
        IntVec3 intVec = CellFinderLoose.GetFleeDest(pawnOther, new List<Thing>() { pawn });
        if (intVec == pawnOther.Position)  return;
       // DebugHelper.Message($">>>>>>>>>>>>>> FLEE PAWN:{pawn.ThingID}");
        Job job2 = JobMaker.MakeJob(JobDefOf.Flee, intVec, pawn);
        pawnOther.jobs.TryTakeOrderedJob(job2);
    }

    protected bool TryAttackAction(Pawn pawnOther)
    {
        if (pawn.CalculateDistanceBetweenPawns(pawnOther) > AggroActive - 1) return false;          
        Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, pawnOther);
        job.maxNumMeleeAttacks = Rand.Range(2, 6);
        job.expiryInterval = Rand.Range(600, 1000);
        job.followRadius = AggroActive - 1;
        pawn.jobs.TryTakeOrderedJob(job);
        MoteMaker.MakeColonistActionOverlay(pawn, ThingDefOf.Mote_ColonistAttacking);  
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        //var deflist = DefDatabase<ThinkTreeDef>.AllDefs;

        Toil toil_1 = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
        yield return toil_1;

        Toil toil_2 = ToilMaker.MakeToil("Incubation");
        toil_2.defaultCompleteMode = ToilCompleteMode.Never;
        toil_2.defaultDuration = IncubationTickDuration;
        toil_2.handlingFacing = true;
        toil_2.initAction = () => 
        { 
            pawn.Rotation = Rot4.South;
            pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
        };
        toil_2.AddFinishAction(() => pawn?.pather?.StopDead());
        //toil_2.AddPreInitAction(() => DebugHelper.Message("toil_2 int: {0} {1}", pawn.ThingID, pawn.Position.ToString()));
        //toil_2.AddFinishAction(() => DebugHelper.Message("toil_2 finish: {0} {1}", pawn.ThingID, pawn.Position.ToString()));

        Action<int> tickIntervalAction = delta =>
        {
            if (ticksLeftThisToil < 0)
            {
                DebugHelper.Message("ticksLeftThisToil: {0} {1}", pawn.ThingID, pawn.Position.ToString());
                EndJobWith(JobCondition.Succeeded);
            }
            else if (pawn.Position != TargetA.Cell)
            {
                DebugHelper.Message("Incompletable: {0} {1} A:{2}", pawn.ThingID, pawn.Position.ToString(), TargetA);
                EndJobWith(JobCondition.Incompletable);
            }
            else if (TrySleep())
            {
                DebugHelper.Message("GoSleep: {0} {1}", pawn.ThingID, pawn.Position.ToString());
                EndJobWith(JobCondition.Succeeded);
            }
            else if (RimKeeperAnimalsModSettings.ActiveEggIncubationProtect 
                && pawn.IsWild(AggroWildnessTreshhold) 
                && pawn.SeeNearPawns(AggroActive).FilterPawnJobs().Any())
            {
                ReadyForNextToil();
            }
            else if (pawn.jobs.jobQueue.Any())
            {
                DebugHelper.Message("jobQueue: {0} {1}", pawn.ThingID, pawn.Position.ToString());
            }
            else if (job?.expireOnEnemiesNearby ?? false)
            {
#if !RIMWORLD_15
                pawn.jobs.CheckForJobOverride(1.0f, true);
#endif
            }
        };
#if RIMWORLD_15
        toil_2.tickAction = () => tickIntervalAction(0);
#else
        toil_2.tickIntervalAction = tickIntervalAction;
#endif
        yield return toil_2;

        Toil toil_3 = ToilMaker.MakeToil("AlertNest");
        toil_3.defaultDuration = 60 * AggroDelay;
        toil_3.defaultCompleteMode = ToilCompleteMode.Delay;
        toil_3.tickAction = () =>
        {
            if (!RimKeeperAnimalsModSettings.ActiveEggIncubationProtect) return;
            if (Find.TickManager.TicksGame % CheckInterval != 0) return;
            if (!pawn.IsWild(AggroWildnessTreshhold)) return;
            //var pawns_danger = ;
            if (pawn.SeeNearPawns(AggroActive).FilterPawnJobs().Any())
            {
                TryScareAwayAnimation();
                foreach (var pawn_danger in pawn.SeeNearPawns(AggroActive).FilterPawnJobs().ToList())
                {
                    if (pawn.IfPawnTooAgressive(pawn_danger, AggroFleeBodySize))
                    {
                        TryScareAwayAction(pawn);
                        EndJobWith(JobCondition.Succeeded);
                        return;
                    }
                    TryScareAwayAction(pawn_danger);               
                }
            }
            else
            {
                JumpToToil(toil_1);
            }
        };
        yield return toil_3;

        Toil toil_4 = ToilMaker.MakeToil("DefendNest");
        toil_4.defaultCompleteMode = ToilCompleteMode.Instant;
        toil_4.initAction = () =>
        {
            if (!RimKeeperAnimalsModSettings.ActiveEggIncubationProtect) return;
            if (!pawn.IsWild(AggroWildnessTreshhold)) return;
            foreach (var pawn_danger in pawn.SeeNearPawns(AggroActive).FilterPawnJobs())
            {
                if (TryAttackAction(pawn_danger)) return;
            }
        };
        yield return toil_4;
    }
}