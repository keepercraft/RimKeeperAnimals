using Verse;

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
    }
}
