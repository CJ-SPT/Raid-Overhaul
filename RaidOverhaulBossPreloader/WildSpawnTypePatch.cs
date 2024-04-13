using System.Collections.Generic;
using Mono.Cecil;

namespace LegionPrePatch
{
    public static class WildSpawnTypePatch
    {
        public static IEnumerable<string> TargetDLLs { get; } = new string[]
        {
            "Assembly-CSharp.dll"
        };

        public static void Patch(ref AssemblyDefinition assembly)
        {
            TypeDefinition type = assembly.MainModule.GetType("EFT.WildSpawnType");
            FieldDefinition item = new FieldDefinition("bosslegion", FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault, type)
            {
                Constant = bosslegionValue
            };
            type.Fields.Add(item);
        }

        public static uint bosslegionValue = 199;
    }
}