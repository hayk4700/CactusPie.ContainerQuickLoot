using BepInEx;
using BepInEx.Configuration;

namespace CactusPie.ContainerQuickLoot
{
    [BepInPlugin("com.cactuspie.containerquickloot.cqlv4", "CactusPie.ContainerQuickLoot.CQLv4", "1.9.1")]
    public class ContainerQuickLootPlugin : BaseUnityPlugin
    {
        internal static ConfigEntry<bool> EnableForCtrlClick { get; private set; }

        internal static ConfigEntry<bool> EnableForLooseLoot { get; private set; }

        internal static ConfigEntry<bool> AutoMergeStacks { get; private set; }

        internal static ConfigEntry<bool> AutoMergeStacksForNonLootContainers { get; private set; }

        internal static ConfigEntry<string> CustomizeTagForLootContainers { get; private set; }

        internal void Start()
        {
            const string sectionName = "Container quick loot setting (CQLv4)";

            EnableForCtrlClick = Config.Bind
            (
                sectionName,
                "Enable for Ctrl+click",
                true,
                new ConfigDescription
                (
                    "Automatically put the items in containers while transferring them with ctrl+click"
                )
            );

            EnableForLooseLoot = Config.Bind
            (
                sectionName,
                "Enable for loose loot",
                true,
                new ConfigDescription
                (
                    "Automatically put loose loot in containers"
                )
            );

            AutoMergeStacks = Config.Bind
            (
                sectionName,
                "Merge stacks",
                true,
                new ConfigDescription
                (
                    "Automatically merge stacks (money, ammo, etc.) when transferring them into a container"
                )
            );

            AutoMergeStacksForNonLootContainers = Config.Bind
            (
                sectionName,
                "Merge stacks for non-loot containers",
                true,
                new ConfigDescription
                (
                    "Automatically merge stacks (money, ammo, etc.) when quickly transferring items to " +
                    "containers not marked with a loot tag"
                )
            );

            CustomizeTagForLootContainers = Config.Bind
            (
                sectionName,
                "Customize the keyword used to denote loot containers in tags",
                "@loot",
                new ConfigDescription
                (
                    "Customize the keyword used to denote loot containers in tags\nDefault value: \"@loot\""
                )
            );

            new QuickTransferPatch().Enable();
        }
    }
}
