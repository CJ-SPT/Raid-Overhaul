using Mono.Cecil;
using System.Collections.Generic;
using LegionPreLoader.Helpers;

namespace LegionPreLoader.Patches
{
    public static class LegionWildSpawnTypePatch
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static void Patch(ref AssemblyDefinition assembly)
        {
            var wildSpawnType = assembly.MainModule.GetType("EFT.WildSpawnType");
            LegionUtils.AddEnumValue(ref wildSpawnType, LegionEnums.BossLegionName, LegionEnums.BossLegionValue);
        }
    }
}