using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
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
           // DebugHelper.Message("Incubation_Egg try get {0} ", pawn.LabelCap);
            if (compProperties == null) return null;
           // DebugHelper.Message("Incubation EGG {0} on {1}", pawn.LabelCap, compProperties.eggFertilizedDef.label);

            Thing egg = pawn.Map.listerThings
                .ThingsOfDef(compProperties.eggFertilizedDef)
                .Where(w => !w.Position.GetThingList(w.Map).Any(c => c is IStorageGroupMember))
                .Where(w => !w.Position.GetThingList(w.Map).Any(c => c is Pawn))
                .Where(w => !w.Map.mapPawns.AllPawns.Any(c => c.RaceProps.Animal && c.jobs.AllJobs().Any(a => a.targetA.Thing == w)))
                //.ThingsInGroup(ThingRequestGroup.Egg)
                .OrderBy(t => pawn.Position.DistanceTo(t.Position))
                .FirstOrDefault();

            //if (egg == null) return null;
            //DebugHelper.Message("Incubation egg {0} -> {1}", pawn.LabelCap, egg?.def.defName ?? "");
            //if (egg.Position.GetFirstPawn(egg.Map) != null) return null;
            //if (egg.Map.mapPawns.AllPawns.Any(c => c.RaceProps.Animal && c.jobs.AllJobs().Any(a => a.targetA.Thing == egg))) return null;
            return egg;
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

        public static void PrintAllPropertiesOfThingDef<T>(T thingDef)
        {
            // Pobierz wszystkie właściwości ThingDef
            PropertyInfo[] properties = typeof(T).GetProperties();

            // Iteruj przez wszystkie właściwości i wypisz je na konsoli
            foreach (PropertyInfo property in properties)
            {
                // Pobierz wartość właściwości dla thingDef
                object value = property.GetValue(thingDef);

                DebugHelper.Message("{0} => {1}", thingDef.ToString(), value.ToString());
            }
        }

        public static int CountAnimals(this Map map, bool includeTamed = true)
        {
            if (map.mapPawns == null) return -1;
            return map.mapPawns.AllPawns
                .Count(pawn =>
                    pawn.RaceProps?.Animal == true &&
                    (includeTamed || pawn.Faction == null || pawn.Faction != Faction.OfPlayer));
        }

        public static int CountAnimalsOnAllMaps(this List<Map> maps, bool includeWild = true, bool includeFactionPlayer = true, bool includeFactionOther = false)
        {
            if (maps == null || maps.Count == 0)
                return 0;

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
    }
}
