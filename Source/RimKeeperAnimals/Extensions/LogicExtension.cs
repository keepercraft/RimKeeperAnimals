using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using Keepercraft.RimKeeperAnimals.ThinkNodes;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
namespace Keepercraft.RimKeeperAnimals.Extensions;

public static class LogicsExtension
{
    public static bool FinalDeteriorationRate_Egg(this SteadyEnvironmentEffects contect, ref float value, Thing thing, List<string> reasons = null)
    {
        if (!RimKeeperAnimalsModSettings.ActiveEggIncubation) return true;
        if (!thing.def.IsEgg) return true;
        Pawn pawn = thing.Position.GetFirstPawn(thing.Map);
        if (pawn == null) return true;
        if (!pawn.HasComp<CompEggLayer>()) return true;
        //DebugHelper.Message("FinalDeteriorationRate_Egg {0}-{1}", thing.def.defName, pawn.LabelCap);
        reasons?.Add("Incubation");
        value = 1f;
        return false;
    }

    public static Thing? FindIncubation_Egg(this Pawn pawn)
    {
        if (pawn == null || pawn?.Map == null || !pawn.Spawned) return null;
        CompProperties_EggLayer compProperties = pawn.kindDef.race.GetCompProperties<CompProperties_EggLayer>();
        if (compProperties == null) return null;
        return pawn.Map.listerThings
            .ThingsOfDef(compProperties.eggFertilizedDef)
            .Where(w => w.Spawned)
            .Where(w => !w.Position.GetThingList(w.Map).Any(c => c is IStorageGroupMember))
            .Where(w => !w.Position.GetThingList(w.Map).Any(c => c is Pawn && c != pawn))
            .Where(w => pawn.CanReach(w, PathEndMode.OnCell, Danger.None))
            .Where(w => pawn.CanReserve(w, 1, -1, null, false))
            .Where(w => !w.Map.mapPawns.AllPawns.Any(c => c.RaceProps.Animal && c.jobs.AllJobs().Any(a => a.targetA.Thing == w)))
            //.ThingsInGroup(ThingRequestGroup.Egg)
            .OrderBy(t => pawn.Position.DistanceTo(t.Position))
            .FirstOrDefault();
    }

    public static IntVec3? FindCellRoofed(this Pawn pawn, ThingDef thingDef, int searchRadius = 25, int stackThreshold = 10)
    {
        if (pawn == null || pawn?.Map == null || !pawn.Spawned)  return null;
        return GenRadial.RadialCellsAround(pawn.Position, searchRadius, true)
            .Where(w => w.Walkable(pawn.Map))
            .Where(w => pawn.Map.roofGrid.Roofed(w))
            .Where(w => pawn.CanReach(w, PathEndMode.OnCell, Danger.None))
            .Where(w => !w.GetTerrain(pawn.Map).IsWater)
            .Where(w => w.GetFirstPawn(pawn.Map) == null)
            .Where(w => w.GetThingList(pawn.Map).Sum(s => s.stackCount) < stackThreshold)
            .OrderByDescending(c => c.GetThingList(pawn.Map).Any(t => t.def == thingDef))
            .Select(s => (IntVec3?)s)
            .FirstOrDefault();
    }

    public static IEnumerable<Pawn> SeeNearPawns(this Thing thing, float searchRadius)
    {
        if (thing == null || thing?.Map == null || !thing.Spawned || searchRadius <= 0f) return Enumerable.Empty<Pawn>();
        Map map = thing.Map;
        IntVec3 position = thing.Position;
        return GenRadial.RadialCellsAround(position, searchRadius, false)
            .Where(cell => cell.InBounds(map))
            .SelectMany(cell => cell.GetThingList(map).OfType<Pawn>())
            .Where(w => !w.Dead)
            .Where(w => w.Spawned)
            .Where(w => w.def != thing.def)
            .Where(w => w != null && w != thing)
            .Where(w => w.Faction == null || w.Faction != thing.Faction)
            .Where(w => GenSight.LineOfSightToThing(position, w, map));
    }

    public static bool IncubationInValid(this Pawn pawn, Job job = null)
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

    public static float CalculateDistanceBetweenPawns(this Pawn pawn1, Pawn pawn2)
    {
        if (pawn1 != null && pawn2 != null && pawn1.Map == pawn2.Map)
        {
            IntVec3 pos1 = pawn1.Position;
            IntVec3 pos2 = pawn2.Position;
            float distance = (float)Mathf.Sqrt(Mathf.Pow(pos2.x - pos1.x, 2) + Mathf.Pow(pos2.z - pos1.z, 2));

            return distance;
        }
        else
        {
            return -1f;
        }
    }

    public static int CountAnimalsOnAllMaps(this List<Map> maps, bool includeWild = true, bool includeFactionPlayer = true, bool includeFactionOther = false)
    {
        if (maps == null || maps.Count == 0) return 0;
        return maps
            .Where(w => w.mapPawns != null)
            .SelectMany(map => map.mapPawns.AllPawns)
            .Count(pawn =>
                pawn.RaceProps?.Animal == true &&
                (
                    (includeWild && pawn.Faction == null) ||
                    (includeFactionPlayer && pawn.Faction == Faction.OfPlayer) ||
                    (includeFactionOther && pawn.Faction != null && pawn.Faction != Faction.OfPlayer)
                )
            );
    }

    public static bool IfPawnTooAgressive(this Pawn pawn, Pawn pawn_danger, float AggroFleeBodySize = 1)
    {
        return pawn.BodySize * AggroFleeBodySize < pawn_danger.BodySize
            || pawn_danger.InMentalState
            || pawn_danger.CurJob != null && pawn_danger.CurJob.def == JobDefOf.Hunt;
    }

    public static IEnumerable<Pawn> FilterPawnJobs(this IEnumerable<Pawn> pawns) 
        => pawns.Where(w => w.FilterPawnJobs());
    public static bool FilterPawnJobs(this Pawn pawn)
    {
        var curDriver = pawn?.jobs?.curDriver;
        return !(curDriver is IncubationJobDriver) &&
               !(curDriver is JobDriver_LayDown) &&
               !(curDriver is JobDriver_LayEgg) &&
               !(curDriver is JobDriver_GoForWalk) &&
               !(curDriver is JobDriver_Flee) &&
               !(curDriver is JobDriver_LayDownResting) &&
               !(curDriver is JobDriver_Lovin);
    }

    public static bool IsWild(this Pawn pawn, float wildness = 0.0f)
    {
#if RIMWORLD_15
        return pawn.GetStatValue(StatDefOf.MinimumHandlingSkill) > wildness;
#else
        return pawn.GetStatValue(StatDefOf.Wildness) > wildness;
#endif
    }
}