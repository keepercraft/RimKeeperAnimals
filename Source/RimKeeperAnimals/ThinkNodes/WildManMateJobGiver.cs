using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace Keepercraft.RimKeeperAnimals.ThinkNodes
{
    public class WildManMateJobGiver : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!RimKeeperAnimalsModSettings.ActiveMateWildMan) return null;
            if (!pawn.IsWildMan()) return null;
            if (!pawn.gender.HasFlag(Gender.Male)) return null;
            if (pawn.Sterile()) return null;
            if (!RimKeeperAnimalsModSettings.MateActive()) return null;

            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn3 = t as Pawn;
                return !pawn3.Downed &&
                    pawn3.CanCasuallyInteractNow(false, false, false, false) &&
                    !pawn3.IsForbidden(pawn) &&
                    pawn3.IsWildMan() &&
                    //pawn3.Faction == pawn.Faction &&
                    PawnUtility.FertileMateTarget(pawn, pawn3);
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

            DebugHelper.Message("WildManMateJobGiver: {0} -> {1}", pawn.LabelCap, pawn2.LabelCap);
            return JobMaker.MakeJob(JobDefOf.Mate, pawn2);
        }

        public static bool FertileMateTarget_X(Pawn male, Pawn female)
        {
            return male != female && female.gender != Gender.Female;
            //PawnUtility.FertileMateTarget(male, female);
        }
    }
}
