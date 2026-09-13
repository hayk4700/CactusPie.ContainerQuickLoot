using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;

namespace CactusPie.ContainerQuickLoot
{
    public class QuickTransferPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ItemManipulator).GetMethod("QuickFindAppropriatePlace", BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPrefix]
        public static bool PatchPrefix(
            ref OperationResult<IItemOperationResult> __result,
            Item item,
            ItemController controller,
            IEnumerable<CompoundItem> targets,
            ItemManipulator.EMoveItemOrder order,
            bool simulate)
        {
            Inventory inventory;

            // If is ctrl+click loot
            if (order == ItemManipulator.EMoveItemOrder.MoveToAnotherSide)
            {
                if (!ContainerQuickLootPlugin.EnableForCtrlClick.Value)
                {
                    return true;
                }
            }
            // If is loose loot pick up
            else if (order == ItemManipulator.EMoveItemOrder.PickUp && controller.OwnerType == EOwnerType.Profile)
            {
                if (!ContainerQuickLootPlugin.EnableForLooseLoot.Value)
                {
                    if (!TryGetInventory(out inventory))
                    {
                        return true;
                    }

                    return !TryMergeItemIntoAnExistingStack(item, inventory, controller, simulate, ref __result);
                }
            }
            else
            {
                return true;
            }

            // This check needs to be done only in game - otherwise we will not be able to receive quest rewards!
            if (item.QuestItem)
            {
                return true;
            }

            if (!TryGetInventory(out inventory))
            {
                return true;
            }

            IEnumerable<IContainer> targetContainers = FindTargetContainers(item, inventory);

            foreach (IContainer collectionContainer in targetContainers)
            {
                if (!(collectionContainer is Grid container))
                {
                    return !TryMergeItemIntoAnExistingStack(item, inventory, controller, simulate, ref __result);
                }

                // ReSharper disable once PossibleMultipleEnumeration
                if (!(targets.SingleOrDefaultWithoutException() is InventoryEquipment))
                {
                    continue;
                }

                if (ContainerQuickLootPlugin.AutoMergeStacks.Value && item.StackMaxSize > 1 && item.StackObjectsCount != item.StackMaxSize)
                {
                    foreach (KeyValuePair<Item, LocationInGrid> containedItem in container.ContainedItems)
                    {
                        if (containedItem.Key.TemplateId != item.TemplateId)
                        {
                            continue;
                        }

                        if (containedItem.Key.StackObjectsCount + item.StackObjectsCount > item.StackMaxSize)
                        {
                            continue;
                        }

                        OperationResult<MergeResult> mergeResult = ItemManipulator.Merge(item, containedItem.Key, controller, simulate);
                        __result = new OperationResult<IItemOperationResult>(mergeResult.Value);
                        return false;
                    }
                }

                ItemAddress location = container.FindLocationForItem(item);
                if (location == null)
                {
                    continue;
                }

                OperationResult<MoveResult> moveResult = ItemManipulator.Move(item, location, controller, simulate);
                if (moveResult.Failed)
                {
                    return true;
                }

                if (!moveResult.Value.ItemsDestroyRequired)
                {
                    __result = new OperationResult<IItemOperationResult>(moveResult.Value);
                }

                return false;
            }

            return !TryMergeItemIntoAnExistingStack(item, inventory, controller, simulate, ref __result);
        }

        private static bool TryGetInventory(out Inventory inventory)
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;

            // If gameWorld is null that means the game is currently not in progress, for instance you're in your hideout
            // start 3.7 hideout gameWorld no longer be null. So need to use getlocationId
            if (gameWorld == null || gameWorld.LocationId == null)
            {
                inventory = null;
                return false;
            }

            Player player = GetLocalPlayerFromWorld(gameWorld);
            inventory = player?.Inventory;

            if (inventory == null)
            {
                return false;
            }

            return true;
        }

        private static IEnumerable<IContainer> FindTargetContainers(Item item, Inventory inventory)
        {
            var matchingContainerCollections = new List<(CompoundItem containerCollection, int priority)>();

            string tag = ContainerQuickLootPlugin.CustomizeTagForLootContainers.Value;
            Regex lootTagRegex = new Regex
            (
                tag + "[0-9]*",
                RegexOptions.None,
                TimeSpan.FromMilliseconds(100)
            );

            foreach (Item inventoryItem in inventory.Equipment.GetAllItems())
            {
                // It has to be a container collection - an item that we can transfer the loot into
                if (!inventoryItem.IsContainer)
                {
                    continue;
                }

                // The container has to have a tag - later we will check it's the @loot tag
                if (!inventoryItem.TryGetItemComponent(out TagComponent tagComponent))
                {
                    continue;
                }

                // We check if there is a tag
                Match regexMatch = lootTagRegex.Match(tagComponent.Name);

                if (!regexMatch.Success)
                {
                    continue;
                }

                // We check if any of the containers in the collection can hold our item
                var containerCollection = inventoryItem as CompoundItem;

                if (containerCollection == null || !containerCollection.Containers.Any(container => container.CanAccept(item)))
                {
                    continue;
                }

                // We extract the suffix - if no suffix provided, we assume 0
                // Length of the tag - we only want the number suffix
                string priorityString = regexMatch.Value.Substring(tag.Length);
                int priority = priorityString.Length == 0 ? 0 : int.Parse(priorityString);

                matchingContainerCollections.Add((containerCollection, priority));
            }

            IEnumerable<IContainer> result = matchingContainerCollections
                .OrderBy(x => x.priority)
                .SelectMany(x => x.containerCollection.Containers);

            return result;
        }

        // If there are no matching @loot containers found, we will try to merge the item into an existing stack
        // anyway - but only if this behavior is enabled in the config
        private static bool TryMergeItemIntoAnExistingStack(
            Item item,
            Inventory inventory,
            ItemController controller,
            bool simulate,
            ref OperationResult<IItemOperationResult> result)
        {
            if (!ContainerQuickLootPlugin.AutoMergeStacksForNonLootContainers.Value)
            {
                return false;
            }

            if (item.Template.StackMaxSize <= 1)
            {
                return false;
            }

            foreach (Item targetItem in inventory.Equipment.GetNotMergedItems().Reverse())
            {
                if (targetItem.TemplateId != item.TemplateId)
                {
                    continue;
                }

                if (targetItem.StackObjectsCount + item.StackObjectsCount > item.Template.StackMaxSize)
                {
                    continue;
                }

                OperationResult<MergeResult> mergeResult = ItemManipulator.Merge(item, targetItem, controller, simulate);

                if (!mergeResult.Succeeded)
                {
                    return false;
                }

                result = new OperationResult<IItemOperationResult>(mergeResult.Value);
                return true;
            }

            return false;
        }

        private static Player GetLocalPlayerFromWorld(GameWorld gameWorld)
        {
            if (gameWorld == null || gameWorld.MainPlayer == null)
            {
                return null;
            }

            return gameWorld.MainPlayer;
        }
    }
}
