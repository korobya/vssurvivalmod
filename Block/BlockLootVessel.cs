﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent
{
    public class LootItem
    {
        public AssetLocation[] codes;
        public EnumItemClass type;
        public float chance;
        public float minQuantity;
        public float maxQuantity;

        public static LootItem Item(float chance, float minQuantity, float maxQuantity, params string[] codes)
        {
            return new LootItem()
            {
                codes = AssetLocation.toLocations(codes),
                type = EnumItemClass.Item,
                chance = chance,
                minQuantity = minQuantity,
                maxQuantity = maxQuantity
            };
        }

        public static LootItem Item(float chance, float minQuantity, float maxQuantity, params AssetLocation[] codes)
        {
            return new LootItem()
            {
                codes = codes,
                type = EnumItemClass.Item,
                chance = chance,
                minQuantity = minQuantity,
                maxQuantity = maxQuantity
            };
        }

        public static LootItem Block(float chance, float minQuantity, float maxQuantity, params string[] codes)
        {
            return new LootItem()
            {
                codes = AssetLocation.toLocations(codes),
                type = EnumItemClass.Block,
                chance = chance,
                minQuantity = minQuantity,
                maxQuantity = maxQuantity
            };
        }

        public static LootItem Block(float chance, float minQuantity, float maxQuantity, params AssetLocation[] codes)
        {
            return new LootItem()
            {
                codes = codes,
                type = EnumItemClass.Block,
                chance = chance,
                minQuantity = minQuantity,
                maxQuantity = maxQuantity
            };
        }

        public ItemStack GetItemStack(IWorldAccessor world, int variant)
        {
            ItemStack stack = null;

            AssetLocation code = codes[variant % codes.Length];

            int quantity = (int)minQuantity + (int)(world.Rand.NextDouble() * (maxQuantity - (int)minQuantity));

            if (type == EnumItemClass.Block)
            {
                Block block = world.GetBlock(code);
                if (block != null)
                {
                    stack = new ItemStack(block, quantity);
                } else
                {
                    world.Logger.Warning("BlockLootVessel: Failed resolving block code {0}", code);
                }
            } else
            {
                Item item = world.GetItem(code);
                if (item != null)
                {
                    stack = new ItemStack(item, quantity);
                }
                else
                {
                    world.Logger.Warning("BlockLootVessel: Failed resolving item code {0}", code);
                }
            }

            return stack;
        }
    }

    public class LootList
    {
        public float Tries = 0;
        public List<LootItem> lootItems = new List<LootItem>();
        public float TotalChance = 0;


        public ItemStack[] GenerateLoot(IWorldAccessor world)
        {
            List<ItemStack> stacks = new List<ItemStack>();

            int variant = world.Rand.Next();
            float curtries = Tries;

            while (curtries >= 1 || curtries > world.Rand.NextDouble())
            {
                lootItems.Shuffle(world.Rand);

                double choice = world.Rand.NextDouble() * TotalChance;

                foreach(LootItem lootItem in lootItems)
                {
                    choice -= lootItem.chance;

                    if (choice <= 0)
                    {
                        ItemStack stack = lootItem.GetItemStack(world, variant);
                        if (stack != null)
                        {
                            stacks.Add(stack);
                        }
                        break;
                    }
                }

                curtries--;
            }

            return stacks.ToArray();
        }

        public static LootList Create(float tries, params LootItem[] lootItems)
        {
            LootList list = new LootList();
            list.Tries = tries;
            list.lootItems.AddRange(lootItems);

            for (int i = 0; i < lootItems.Length; i++) list.TotalChance += lootItems[i].chance;

            return list;
        }
        
    }

    public class BlockLootVessel : Block
    {
        // Types of loot
        // - Seed container (several seeds of one type)
        // - Food container (bones, grain)
        // - Forage container (flint, sticks, some logs, dry grass, stones, clay)
        // - Ore container (some low level ores, rarely ingots)
        // - Tool container (some half used stone and copper tools, rarely lantern)
        // - Farming container (linen sack, flax fibers, flax twine, 

        public static Dictionary<string, LootList> lootLists = new Dictionary<string, LootList>();

        static BlockLootVessel() {

            lootLists["seed"] = LootList.Create(1, 
                LootItem.Item(1, 3, 5, "seeds-carrot", "seeds-onion", "seeds-spelt", "seeds-turnip", "seeds-rice", "seeds-rye", "seeds-soybean", "seeds-pumpkin", "seeds-cabbage")
            );

            lootLists["food"] = LootList.Create(1, 
                LootItem.Item(3, 8, 15, "grain-spelt", "grain-rice", "grain-flax", "grain-rye"),
                LootItem.Item(0.1f, 1, 1, "resonancearchive-1", "resonancearchive-2", "resonancearchive-3", "resonancearchive-4", "resonancearchive-5", "resonancearchive-6", "resonancearchive-7", "resonancearchive-8", "resonancearchive-9")
            );

            lootLists["forage"] = LootList.Create(3.5f, 
                LootItem.Item(1, 2, 6, "flint"),
                LootItem.Item(1, 3, 9, "stick"),
                LootItem.Item(1, 3, 16, "drygrass"),
                LootItem.Item(1, 3, 24, "stone-andesite", "stone-chalk", "stone-claystone", "stone-granite", "stone-sandstone", "stone-shale"),
                LootItem.Item(1, 3, 24, "clay-blue", "clay-fire"),
                LootItem.Item(1, 3, 24, "cattailtops"),
                LootItem.Item(1, 1, 4, "poultice-linen-horsetail"), 
                LootItem.Item(0.5f, 1, 12, "flaxfibers"),
                LootItem.Item(0.3f, 1, 3, "honeycomb"),
                LootItem.Item(0.3f, 2, 6, "bamboostakes"),
                LootItem.Item(0.3f, 2, 6, "beenade-closed")
            );

            lootLists["ore"] = LootList.Create(2.75f,
                LootItem.Item(1, 2, 12, "ore-lignite", "ore-bituminouscoal"),
                LootItem.Item(1, 2, 8, "nugget-nativecopper", "ore-quartz", "nugget-galena"),
                LootItem.Item(0.3f, 4, 12, "nugget-galena", "nugget-cassiterite", "nugget-sphalerite", "nugget-bismuthinite"),
                LootItem.Item(0.1f, 4, 12, "nugget-limonite", "nugget-nativegold", "nugget-chromite", "nugget-ilmenite", "nugget_nativesilver", "nugget-magnetite")
            );

            lootLists["tool"] = LootList.Create(2.2f, 
                LootItem.Item(1, 1, 1, "axe-flint"),
                LootItem.Item(1, 1, 1, "shovel-flint"),
                LootItem.Item(1, 1, 1, "knife-flint"),
                LootItem.Item(0.1f, 1, 1, "axe-copper", "axe-copper", "axe-tinbronze"),
                LootItem.Item(0.1f, 1, 1, "shovel-copper", "shovel-copper", "shovel-tinbronze"),
                LootItem.Item(0.1f, 1, 1, "pickaxe-copper", "pickaxe-copper", "pickaxe-tinbronze"),
                LootItem.Item(0.1f, 1, 1, "knife-copper", "knife-copper", "knife-tinbronze"),
                LootItem.Item(0.1f, 1, 1, "sword-copper", "sword-copper", "sword-tinbronze"),
                LootItem.Item(0.1f, 1, 4, "gear-rusty")
            );

            lootLists["farming"] = LootList.Create(2.75f,
                LootItem.Item(0.1f, 1, 1, "linensack"),
                LootItem.Item(0.5f, 1, 1, "basket"),
                LootItem.Item(0.75f, 3, 10, "feather"),
                LootItem.Item(0.75f, 2, 10, "flaxfibers"),
                LootItem.Item(0.35f, 2, 10, "flaxtwine"),
                LootItem.Item(0.75f, 2, 4, "seeds-cabbage"),
                LootItem.Item(0.75f, 5, 10, "cattailtops"),
                LootItem.Item(0.1f, 1, 1, "scythe-copper", "scythe-tinbronze")
            );    
        }

        
        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            LootList list = lootLists[LastCodePart()];
            if (list == null) return new ItemStack[0];

            return list.GenerateLoot(world);
        }

        public override float OnGettingBroken(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter)
        {
            EnumTool? tool = itemslot.Itemstack?.Collectible?.Tool;

            if (tool == EnumTool.Hammer || tool == EnumTool.Pickaxe || tool == EnumTool.Shovel || tool == EnumTool.Sword || tool == EnumTool.Spear || tool == EnumTool.Axe || tool == EnumTool.Hoe)
            {
                return remainingResistance - 0.05f;
            }

            return base.OnGettingBroken(player, blockSel, itemslot, remainingResistance, dt, counter);
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            SimpleParticleProperties props =
                new SimpleParticleProperties(
                    15, 22,
                    ColorUtil.ToRgba(150, 255, 255, 255),
                    new Vec3d(pos.X, pos.Y, pos.Z),
                    new Vec3d(pos.X + 1, pos.Y + 1, pos.Z + 1),
                    new Vec3f(-0.2f, -0.1f, -0.2f),
                    new Vec3f(0.2f, 0.2f, 0.2f),
                    1.5f,
                    0,
                    0.5f,
                    1.0f,
                    EnumParticleModel.Quad
                );

            props.OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -200);
            props.SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, 2);

            world.SpawnParticles(props);



            SimpleParticleProperties spiders =
                new SimpleParticleProperties(
                    8, 16,
                    ColorUtil.ToRgba(255, 30, 30, 30),
                    new Vec3d(pos.X, pos.Y, pos.Z),
                    new Vec3d(pos.X + 1, pos.Y + 1, pos.Z + 1),
                    new Vec3f(-2f, -0.3f, -2f),
                    new Vec3f(2f, 1f, 2f),
                    1f,
                    0.5f,
                    0.5f,
                    1.5f,
                    EnumParticleModel.Cube
                );
            
            
            world.SpawnParticles(spiders);



            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
    }
}
