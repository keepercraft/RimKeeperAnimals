using HarmonyLib;
using Keepercraft.RimKeeperAnimals.Extensions;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Keepercraft.RimKeeperAnimals.Patches
{
    [HarmonyPatch(typeof(SteadyEnvironmentEffects), "FinalDeteriorationRate", new Type[] { typeof(Thing), typeof(bool), typeof(bool), typeof(TerrainDef), typeof(List<string>) })]
    public static class FinalDeteriorationRate_SteadyEnvironmentEffects_Terrian_Path
    {
        static bool Prefix(SteadyEnvironmentEffects __instance, ref float __result, Thing t, bool roofed, bool roomUsesOutdoorTemperature, TerrainDef terrain, List<string> reasons = null)
        {
            return __instance.FinalDeteriorationRate_Egg(ref __result, t, reasons);
        }
    }

    [HarmonyPatch(typeof(SteadyEnvironmentEffects), "FinalDeteriorationRate", new Type[] { typeof(Thing), typeof(List<string>) })]
    public static class FinalDeteriorationRate_SteadyEnvironmentEffects_Path
    {
        static bool Prefix(SteadyEnvironmentEffects __instance, ref float __result, Thing t, List<string> reasons = null)
        {
            return __instance.FinalDeteriorationRate_Egg(ref __result, t, reasons);
        }
    }
}