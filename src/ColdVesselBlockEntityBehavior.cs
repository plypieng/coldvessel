using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ColdVessel
{
    public class ColdVesselBlockEntityBehavior : BlockEntityBehavior
    {
        private const string TreeKey = "coldvessel";

        private double coolingHoursRemaining;
        private double lastTotalHours;
        private long tickListenerId;
        private bool lastCoolingActive;
        private bool coldMulApplied;
        private bool hadOriginalPerishMul;
        private bool loggedFirstCoolingMultiplier;
        private float originalPerishMul = 1f;

        public ColdVesselBlockEntityBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            tickListenerId = Blockentity.RegisterGameTickListener(OnTick, Math.Max(1000, ColdVesselModSystem.Config.TickIntervalMs), 0);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            ITreeAttribute ownTree = tree.GetTreeAttribute(TreeKey);
            if (ownTree == null) return;

            coolingHoursRemaining = ownTree.GetDouble("coolingHoursRemaining");
            lastTotalHours = ownTree.GetDouble("lastTotalHours");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            ITreeAttribute ownTree = tree.GetOrAddTreeAttribute(TreeKey);
            ownTree.SetDouble("coolingHoursRemaining", coolingHoursRemaining);
            ownTree.SetDouble("lastTotalHours", lastTotalHours);
        }

        public override void OnBlockUnloaded()
        {
            ApplyCoolingMultiplier(GetInventory(), false);
            if (tickListenerId != 0) Blockentity.UnregisterGameTickListener(tickListenerId);
            base.OnBlockUnloaded();
        }

        public override void OnBlockRemoved()
        {
            ApplyCoolingMultiplier(GetInventory(), false);
            if (tickListenerId != 0) Blockentity.UnregisterGameTickListener(tickListenerId);
            base.OnBlockRemoved();
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            if (IsCoolingActive())
            {
                dsc.AppendLine(Lang.Get("coldvessel:coldvessel-info-active", coolingHoursRemaining, GetCoolingRate()));
            }
            else
            {
                dsc.AppendLine(Lang.Get("coldvessel:coldvessel-info-inactive"));
            }
        }

        private void OnTick(float dt)
        {
            IInventory inventory = GetInventory();
            if (inventory == null) return;

            bool wasCoolingActive = IsCoolingActive();
            bool hasPerishable = HasPerishableFood(inventory);
            double now = Blockentity.Api.World.Calendar.TotalHours;
            if (lastTotalHours <= 0) lastTotalHours = now;

            double elapsedHours = Math.Max(0, now - lastTotalHours);
            lastTotalHours = now;

            if (coolingHoursRemaining > 0)
            {
                coolingHoursRemaining = Math.Max(0, coolingHoursRemaining - elapsedHours);
            }

            if (coolingHoursRemaining <= 0 && (!ColdVesselModSystem.Config.ConsumeOnlyWhenPerishablePresent || hasPerishable))
            {
                if (TryConsumeCoolant(inventory))
                {
                    MarkPerishableSlotsDirty(inventory);
                }
            }

            bool isCoolingActive = IsCoolingActive();
            ApplyCoolingMultiplier(inventory, isCoolingActive);

            if (wasCoolingActive != isCoolingActive || lastCoolingActive != isCoolingActive)
            {
                lastCoolingActive = isCoolingActive;
                MarkPerishableSlotsDirty(inventory);
                Blockentity.MarkDirty(true);
                return;
            }

            Blockentity.MarkDirty(false);
        }

        private IInventory GetInventory()
        {
            IBlockEntityContainer container = Blockentity as IBlockEntityContainer;
            if (container != null) return container.Inventory;

            PropertyInfo inventoryProperty = Blockentity.GetType().GetProperty("Inventory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (inventoryProperty == null) return null;

            return inventoryProperty.GetValue(Blockentity, null) as IInventory;
        }

        private bool HasPerishableFood(IInventory inventory)
        {
            foreach (ItemSlot slot in inventory)
            {
                if (slot.Empty) continue;
                if (IsCoolant(slot.Itemstack)) continue;
                if (IsPerishable(slot.Itemstack)) return true;
            }

            return false;
        }

        private void MarkPerishableSlotsDirty(IInventory inventory)
        {
            foreach (ItemSlot slot in inventory)
            {
                if (slot.Empty) continue;
                if (IsCoolant(slot.Itemstack)) continue;
                if (!IsPerishable(slot.Itemstack)) continue;

                slot.MarkDirty();
            }
        }

        private bool IsPerishable(ItemStack stack)
        {
            if (stack == null || stack.Collectible == null) return false;

            TransitionableProperties[] props = stack.Collectible.GetTransitionableProperties(Blockentity.Api.World, stack, null);
            if (props == null) return false;

            foreach (TransitionableProperties prop in props)
            {
                if (prop.Type == EnumTransitionType.Perish) return true;
            }

            return false;
        }

        private bool TryConsumeCoolant(IInventory inventory)
        {
            foreach (ItemSlot slot in inventory)
            {
                if (slot.Empty) continue;

                ColdVesselCoolant coolant = GetCoolant(slot.Itemstack);
                if (coolant == null) continue;

                ItemStack taken = slot.TakeOut(1);
                if (taken == null) continue;

                coolingHoursRemaining += Math.Max(0, coolant.CoolingHours);
                slot.MarkDirty();
                return true;
            }

            return false;
        }

        private bool ShouldApplyCooling(EnumTransitionType transType, ItemStack stack)
        {
            return transType == EnumTransitionType.Perish && IsCoolingActive() && !IsCoolant(stack);
        }

        private bool IsCoolant(ItemStack stack)
        {
            return GetCoolant(stack) != null;
        }

        private ColdVesselCoolant GetCoolant(ItemStack stack)
        {
            if (stack == null || stack.Collectible == null || stack.Collectible.Code == null) return null;

            string stackCode = stack.Collectible.Code.ToString();
            foreach (ColdVesselCoolant coolant in ColdVesselModSystem.Config.Coolants)
            {
                if (string.Equals(stackCode, coolant.Code, StringComparison.OrdinalIgnoreCase)) return coolant;
            }

            return null;
        }

        private bool IsCoolingActive()
        {
            return coolingHoursRemaining > 0;
        }

        private void ApplyCoolingMultiplier(IInventory inventory, bool active)
        {
            IDictionary speedMulByType;
            if (!TryGetTransitionMulByType(inventory, out speedMulByType)) return;

            if (active)
            {
                if (!coldMulApplied)
                {
                    hadOriginalPerishMul = speedMulByType.Contains(EnumTransitionType.Perish);
                    originalPerishMul = hadOriginalPerishMul ? Convert.ToSingle(speedMulByType[EnumTransitionType.Perish]) : 1f;
                    coldMulApplied = true;
                }

                float coldMul = GetCoolingRate();
                speedMulByType[EnumTransitionType.Perish] = originalPerishMul * coldMul;

                if (ColdVesselModSystem.Config.DebugLogging && !loggedFirstCoolingMultiplier)
                {
                    loggedFirstCoolingMultiplier = true;
                    Blockentity.Api.Logger.Notification("[coldvessel] Applied inventory perish speed multiplier at {0}: base={1:0.###}, cold={2:0.###}, final={3:0.###}", Blockentity.Pos, originalPerishMul, coldMul, speedMulByType[EnumTransitionType.Perish]);
                }

                return;
            }

            if (!coldMulApplied) return;

            if (hadOriginalPerishMul)
            {
                speedMulByType[EnumTransitionType.Perish] = originalPerishMul;
            }
            else
            {
                speedMulByType.Remove(EnumTransitionType.Perish);
            }

            coldMulApplied = false;
            hadOriginalPerishMul = false;
            originalPerishMul = 1f;
        }

        private float GetCoolingRate()
        {
            return Math.Max(0.01f, Math.Min(1f, ColdVesselModSystem.Config.CooledPerishRate));
        }

        private bool TryGetTransitionMulByType(IInventory inventory, out IDictionary speedMulByType)
        {
            speedMulByType = null;
            if (inventory == null) return false;

            PropertyInfo property = inventory.GetType().GetProperty("TransitionableSpeedMulByType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || !property.CanRead || !property.CanWrite) return false;

            object value = property.GetValue(inventory, null);
            if (value == null)
            {
                value = Activator.CreateInstance(property.PropertyType);
                property.SetValue(inventory, value, null);
            }

            speedMulByType = value as IDictionary;
            return speedMulByType != null;
        }
    }
}
