using Verse;
using Verse.AI;
namespace Keepercraft.RimKeeperAnimals.ThinkNodes;

public class ThinkNode_ChancePerHour_Wait : ThinkNode_ChancePerHour
{
    public ThinkNode_ChancePerHour_Wait(float mtbHours = 1f, float priority = -1f)
    {
        base.priority = priority;
        this.mtbHours = mtbHours;
    }

    protected float mtbHours = 1f;
    protected override float MtbHours(Pawn pawn ) => mtbHours;
}