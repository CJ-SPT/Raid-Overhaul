/*
using System.Reflection;
using EFT;
using SPT.Reflection.Patching;
using RaidOverhaul.Helpers;

namespace RaidOverhaul.Patches
{
    public class HitStaminaPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("ApplyHitDebuff", BindingFlags.Instance | BindingFlags.Public);
        } 

        [PatchPrefix]
        static bool Prefix(ref float staminaBurnRate)
        {
            staminaBurnRate *= Utils.GetStrength();
            Plugin.ROPlayer.ActiveHealthController.DoPainKiller();

            return true;
        }
    }
}
*/