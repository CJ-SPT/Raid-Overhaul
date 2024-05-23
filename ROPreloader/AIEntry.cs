using BepInEx;

namespace LegionPrePatch
{
    [BepInPlugin("DJ.LegionPrePatch", "Legion Boss PrePatch", "2.2.1")]
    public class LegionAIEntry : BaseUnityPlugin
    {
        public static LegionAIEntry Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
        }
    }
}
