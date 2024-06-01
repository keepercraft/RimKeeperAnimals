using Verse;

namespace Keepercraft.RimKeeperAnimals.ThinkNodes
{
    public class IncubationJobDef : JobDef
    {
        public IncubationJobDef()
        {
            this.sleepCanInterrupt = false;
            this.defName = nameof(IncubationJobDriver);
            this.driverClass = typeof(IncubationJobDriver);
            this.reportString = "Incubation";
        }
    }
}
