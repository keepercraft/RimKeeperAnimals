using System.Linq;
using Verse;
using Verse.AI;
namespace Keepercraft.RimKeeperAnimals.Extensions;

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
}
