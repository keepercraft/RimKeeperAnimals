using RimWorld.BaseGen;
using System;
using Verse;
using Verse.AI;

namespace Keepercraft.RimKeeperAnimals.Helpers
{
    public static class DebugHelper
    {
        private static string _header = "Debug";
        public static bool Active = false;

        public static void SetHeader(string text) => _header = string.Format("[{0}] ", text);

        public static void Message(string text, params object[] args)
        {
            if (Active)
            {
                Log.Message(_header + string.Format(text, args));
            }
        }

        public static T FailOnCatch<T>(this T f, Func<bool> func, string msg = "") where T : IJobEndable
        {
            Func<bool> condition = () =>
            {
                try
                {
                    var r =  func();
                    if (r) Message($"{msg} BREAK");
                    return r;
                }
                catch (Exception ex)
                {
                    Message($"{msg} {ex.Message}");
                }
                return false;
            };
            f.AddEndCondition(() => (!condition()) ? JobCondition.Ongoing : JobCondition.Incompletable);
            return f;
        }
    }
}
