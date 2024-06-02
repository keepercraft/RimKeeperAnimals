using Keepercraft.RimKeeperAnimals.Helpers;
using RimWorld;
using System;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace Keepercraft.RimKeeperAnimals.Extensions
{
    public static class JobGiverExtension
    {
        public static IntVec3? FindCellRoofed(this Pawn pawn, int searchRadius = 25)
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.Position, searchRadius, true))
            {
                if (cell.InBounds(pawn.Map) && pawn.Map.roofGrid.Roofed(cell))
                {
                    return cell;
                }
            }
            return null;
        }

        public static IntVec3? FindCellRoofed(this Pawn pawn, ThingDef thingDef, int searchRadius = 25, int stackThreshold = 10)
        {
            return GenRadial.RadialCellsAround(pawn.Position, searchRadius, true)
                .Where(w => w.Walkable(pawn.Map))
                .Where(w => pawn.Map.roofGrid.Roofed(w))
                .Where(w => !w.GetTerrain(pawn.Map).IsWater)
                .Where(w => w.GetFirstPawn(pawn.Map) == null)
                .Where(w => w.GetThingList(pawn.Map).Sum(s => s.stackCount) < stackThreshold)
                .OrderByDescending(c => c.GetThingList(pawn.Map).Any(t => t.def == thingDef))
                .Select(s => (IntVec3?)s)
                .FirstOrDefault();
        }

        public static IntVec3? FindThingDef(this Pawn pawn, ThingDef thingDef, int searchRadius = 25)
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.Position, searchRadius, true))
            {
                if (cell.InBounds(pawn.Map))
                {
                    var thing = cell.GetThingList(pawn.Map).FirstOrDefault(c => c.def.defName == thingDef.defName);
                    if (thing != null)
                    {
                        return thing.Position;
                    }
                }
            }
            return null;
        }

        public static Pawn FindUndesirablePawn(this Pawn pawn, float searchRadius = 3f)
        {
            return GenRadial.RadialCellsAround(pawn.Position, searchRadius, true)
                .Where(w => w.Walkable(pawn.Map))
                .Select(w => w.GetFirstPawn(pawn.Map))
                .Where(w => w != null && w != pawn)
                .Where(w => w.Faction == null || w.Faction != pawn.Faction)
                .Where(w => w.def != pawn.def)
                .FirstOrDefault();
        }
    }
}
