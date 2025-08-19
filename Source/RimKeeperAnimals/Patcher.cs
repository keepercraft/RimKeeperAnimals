using HarmonyLib;
using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using Keepercraft.RimKeeperAnimals.ThinkNodes;
using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Keepercraft.RimKeeperAnimals
{
    [StaticConstructorOnStartup]
    internal class Patcher
    {
        static Patcher()
        {
            Add_JobGiver_Mate();
            Add_JobGiver_Incubation();
            string namespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            DebugHelper.SetHeader(namespaceName.Split('.').LastOrDefault());
            DebugHelper.Active = RimKeeperAnimalsModSettings.DebugLog;
            DebugHelper.Message("Patching");
            new Harmony(namespaceName).PatchAll();
        }

        public static void Add_JobGiver_Mate()
        {
            DefDatabase<ThinkTreeDef>.AllDefs
                .Where(w => w.defName == "AnimalConstant")
                .Do(item =>
                {
                    var v2 = new ThinkNode_Tagger();
                    v2.SetPrivateField("tagToGive", JobTag.SatisfyingNeeds);
                    v2.subNodes.Add(new AnimalMateWild_JobGiver());
                    var v3 = new ThinkNode_ChancePerHour_Mate();
                    v3.subNodes.Add(v2);
                    item.thinkRoot.subNodes.Add(v3);
                });

            DefDatabase<ThinkTreeDef>.AllDefs
                .Where(w => w.defName == "AnimalConstant")
                .Do(item =>
                {
                    var v2 = new ThinkNode_Tagger();
                    v2.SetPrivateField("tagToGive", JobTag.SatisfyingNeeds);
                    v2.subNodes.Add(new AnimalEatAny_JobGiver());
                    //var v3 = new ThinkNode_ChancePerHour_Eat();
                    //v3.SetPrivateField("mtbHours", 1f);
                    //v3.SetPrivateField("mtbDays", -1f);
                    //v3.subNodes.Add(v2);
                    item.thinkRoot.subNodes.Add(v2);
                });

            if (JobDefOf.BreastfeedCarryToMom != null && JobDefOf.Breastfeed != null)
            {
                //DefDatabase<JobDef>.Add(new WildManBabyCareJobDef());
                DefDatabase<ThinkTreeDef>.AllDefs
                    .Where(w => w.defName == "HumanlikeConstant")
                    .Do(item =>
                    {
                        var v2 = new ThinkNode_Tagger();
                        v2.SetPrivateField("tagToGive", JobTag.MiscWork);
                        v2.subNodes.Add(new WildManBabyCareJobGiver());
                        v2.subNodes.Add(new WildManBabyCarryToMomJobGiver());
                        var v3 = new ThinkNode_ChancePerHour_Wait();
                        v3.subNodes.Add(v2);
                        item.thinkRoot.subNodes.Add(v3);
                    });

                DefDatabase<ThinkTreeDef>.AllDefs
                    .Where(w => w.defName == "HumanlikeConstant")
                    .Do(item =>
                    {
                        var v2 = new ThinkNode_Tagger();
                        v2.SetPrivateField("tagToGive", JobTag.SatisfyingNeeds);
                        v2.subNodes.Add(new WildManMateJobGiver());
                        var v3 = new ThinkNode_ChancePerHour_Mate();
                        v3.subNodes.Add(v2);
                        item.thinkRoot.subNodes.Add(v3);
                    });
            }
            else
            {
                DebugHelper.Message("WildMan procreation disable (no BiotechDLC detected)");
            }
        }

        public static void Add_JobGiver_Incubation()
        {
            DefDatabase<JobDef>.Add(new IncubationJobDef());
            DefDatabase<ThinkTreeDef>.AllDefs
                .Where(w => w.defName == "AnimalConstant" || w.defName == "Animal")
                .Do(item =>
                {
                    item.thinkRoot.subNodes.Add(new Incubation_JobGiver());
                });
        }

      //  [HarmonyPatch(typeof(JobGiver_EatRandom), "TryGiveJob")]
        public static class Patch_JobGiver_EatRandom
        {
            public static void Postfix(Pawn pawn, ref Job __result)
            {
                Log.Message($"EAT FIND");
                if (__result != null) return;
                if (pawn.Downed) return;
                Log.Message($"EAT ANY RUN");

                Predicate<Thing> validator = delegate (Thing t)
                {
                    DebugHelper.Message("EAT ANY Ingest {0} look at {1}", pawn.LabelCap, t.LabelCap);
                    if (t.def.category != ThingCategory.Item) return false;
                    if (!t.IngestibleNow) return false;
                    //if (!pawn.RaceProps.CanEverEat(t)) return false;
                    return pawn.CanReserve(t) ? true : false;
                };
                Thing thing = GenClosest.ClosestThingReachable(
                    pawn.Position, 
                    pawn.Map, 
                    ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), 
                    PathEndMode.OnCell, 
                    TraverseParms.For(pawn), 
                    30f, 
                    validator);
                if (thing != null)
                {
                    DebugHelper.Message("EAT ANY Ingest {0} -> {1}", pawn.LabelCap, thing.LabelCap);
                    __result = JobMaker.MakeJob(JobDefOf.Ingest, thing);
                    return;
                }

                Predicate<Thing> validator2 = delegate (Thing t)
                {
                    Pawn pawn3 = t as Pawn;
                    DebugHelper.Message("EAT ANY Hunt {0} look at {1}", pawn.LabelCap, pawn3.LabelCap);
                    return pawn3.CanCasuallyInteractNow(false, false, false, false) &&
                       !pawn3.IsForbidden(pawn) &&
                       pawn3.IngestibleNow;
                };
                Thing thing2 = GenClosest.ClosestThingReachable(
                    pawn.Position,
                    pawn.Map,
                    ThingRequest.ForDef(pawn.def),
                    PathEndMode.Touch,
                    TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false),
                    30f,
                    validator2,
                    null,
                    0,
                    -1,
                    false,
                    RegionType.Set_Passable,
                    false);
                if (thing2 != null)
                {
                    DebugHelper.Message("EAT ANY Hunt {0} -> {1}", pawn.LabelCap, thing.LabelCap);
                    __result = JobMaker.MakeJob(JobDefOf.Hunt, thing2);
                    return;
                }
            }
        }
    }


    public class ThinkNode_ChancePerHour_Eat : ThinkNode_ChancePerHour
    {
        protected override float MtbHours(Pawn pawn)
        {
            DebugHelper.Message("ThinkNode_ChancePerHour_Eat : {0}", pawn.RaceProps.mateMtbHours);
            return 0.0001f;
        }
    }

    public class AnimalEatAny_JobGiver : ThinkNode_JobGiver
    {
        private static int lastTime = 0;
        protected override Job TryGiveJob(Pawn pawn)
        {
            int currentTime = Find.TickManager.TicksGame;
            if (currentTime - lastTime < 20) return null;

            if (pawn.health.Downed || pawn.health.Dead || pawn.health.Downed) return null;
            if (pawn.needs?.food?.CurLevelPercentage > pawn.needs.food.PercentageThreshHungry) return null;
            Hediff malnutrition = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition, false);
            if (malnutrition == null) return null;

            //DebugHelper.Message("EAT ANY RUN malnutrition");
            lastTime = currentTime;

            Predicate<Thing> validator = delegate (Thing t)
            {
                DebugHelper.Message("EAT ANY Ingest {0} look at {1} IngestibleNow:{2}", pawn.LabelCap, t.LabelCap, t.IngestibleNow);
                // if (t.def.category != ThingCategory.Item) return false;
                //if (!t.IngestibleNow) return false;
                //if (!pawn.RaceProps.CanEverEat(t)) return false;
                return
                    (t.def.category == ThingCategory.Item || t.def.category == ThingCategory.Plant) &&
                    t.IngestibleNow &&
                    pawn.CanReserve(t);
            };
            Thing thing = GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways),
                PathEndMode.OnCell,
                TraverseParms.For(pawn),
                30f,
                validator);
            if (thing != null)
            {
                DebugHelper.Message("EAT ANY Ingest {0} -> {1}", pawn.LabelCap, thing.LabelCap);
                return JobMaker.MakeJob(JobDefOf.Ingest, thing);
            }


            if (malnutrition.Severity < 0.15f) return null;
//#if RW15
//            if (pawn.GetStatValue(StatDefOf.MinimumHandlingSkill) < 0.1f) return;
//#elif RW16
//            if (StatDefOf.Wildness.Worker.GetValue(pawn) <= 0.0f) return;
//#endif

            Predicate<Thing> validator2 = delegate (Thing t)
            {
                Pawn pawn3 = t as Pawn;
                var val =
                    pawn3 != pawn &&
                    pawn3.CanCasuallyInteractNow(false, false, false, false) &&
                    !pawn3.IsForbidden(pawn) &&
                    pawn3.RaceProps.IsFlesh;
                 //  pawn3.IngestibleNow;
                //DebugHelper.Message("EAT ANY Hunt {2} ::: {0} look at {1} IsFlesh:{3}", pawn.LabelCap, pawn3.LabelCap, val, (pawn3?.RaceProps?.IsFlesh ?? false));
                return val;
            };
            Thing thing2 = GenClosest.ClosestThingReachable(
                    pawn.Position,
                    pawn.Map,
                    ThingRequest.ForDef(pawn.def),
                    PathEndMode.Touch,
                    TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false),
                    30f,
                    validator2,
                    null,
                    0,
                    -1,
                    false,
                    RegionType.Set_Passable,
                    false);
            if (thing2 != null)
            {
                DebugHelper.Message("EAT ANY Hunt {0} -> {1}", pawn.LabelCap, thing2.LabelCap);
                var job = JobMaker.MakeJob(JobDefOf.AttackMelee, thing2);
                job.maxNumMeleeAttacks = Rand.Range(5, 10);
                job.expiryInterval = Rand.Range(600, 1000);
                return job;
            }
            return null;
        }
    }
} 