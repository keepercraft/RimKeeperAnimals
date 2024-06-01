using Keepercraft.RimKeeperAnimals.Models;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Keepercraft.RimKeeperAnimals.Extensions
{
    public static class LogicsExtension
    {
        public static bool FinalDeteriorationRate_Egg(this SteadyEnvironmentEffects contect, ref float value, Thing thing, List<string> reasons = null)
        {
            if (!RimKeeperAnimalsModSettings.ActiveEggIncubation) return true;
            if (!thing.def.IsEgg) return true;

            Pawn pawn = thing.Position.GetFirstPawn(thing.Map);
            if (pawn == null) return true;
            //if (!pawn.RaceProps.Animal) return true;
            if (!pawn.HasComp<CompEggLayer>()) return true;

            //DebugHelper.Message("FinalDeteriorationRate_Egg {0}-{1}", thing.def.defName, pawn.LabelCap);
            reasons?.Add("Incubation");
            value = 1f;
            return false;
        }

        public static Thing Incubation_Egg(this Pawn pawn)
        {
            //if (!RimKeeperAnimalsModSettings.ActiveEggIncubation) return null;
            //if (pawn.def.race.body.defName != "bird") return null;
            //if (!pawn.gender.HasFlag(Gender.Female)) return null;
            //if (!pawn.HasComp<CompEggLayer>()) return null;
            //if (pawn.needs.rest.CurInstantLevelPercentage > 0.2f) return null;
            // if (!RestUtility.CanFallAsleep(pawn)) return null;

            CompProperties_EggLayer compProperties = pawn.kindDef.race.GetCompProperties<CompProperties_EggLayer>();
            if (compProperties == null) return null;
            Thing egg = pawn.Map.listerThings
                .ThingsOfDef(compProperties.eggFertilizedDef)
                //.ThingsInGroup(ThingRequestGroup.Egg)
                .OrderBy(t => pawn.Position.DistanceTo(t.Position))
                .FirstOrDefault();
            //Log.Message(string.Format("Hatch egg {0} -> {1}", pawn.LabelCap, egg?.def.defName ?? ""));
            if (egg == null) return null;
            if (egg.Position.GetFirstPawn(egg.Map) != null) return null;

            if (egg.Map.mapPawns.AllPawns.Any(c => c.RaceProps.Animal && c.jobs.AllJobs().Any(a => a.targetA.Thing == egg))) return null;
            return egg;
        }
    }
}
