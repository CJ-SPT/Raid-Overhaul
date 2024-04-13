using BepInEx;

namespace LegionPrePatch
{
    [BepInPlugin("DJ.LegionPrePatch", "Legion Boss PrePatch", "1.0.0")]
    public class AIEntry : BaseUnityPlugin
    {
        public static AIEntry Instance { get; private set; }

        public void Awake()
        {
            AIEntry.Instance = this;
        }
    }
}