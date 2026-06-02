using System.Reflection;
using HarmonyLib;
using Verse;

namespace SplitTheRaid
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            new Harmony("boreymarf.splittheraid").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}