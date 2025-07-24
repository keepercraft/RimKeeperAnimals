using HarmonyLib;
using Keepercraft.RimKeeperAnimals.Extensions;
using Keepercraft.RimKeeperAnimals.Helpers;
using Keepercraft.RimKeeperAnimals.Models;
using Keepercraft.RimKeeperAnimals.ThinkNodes;
using RimWorld;
using System.Linq;
using System.Reflection;
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
    }
} 