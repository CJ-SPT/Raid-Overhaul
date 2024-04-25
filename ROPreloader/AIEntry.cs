using BepInEx;

namespace LegionPrePatch
{
    [BepInPlugin("DJ.LegionPrePatch", "Legion Boss PrePatch", "1.1.0")]
    public class LegionAIEntry : BaseUnityPlugin
    {
        public static LegionAIEntry Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
        }
    }
}