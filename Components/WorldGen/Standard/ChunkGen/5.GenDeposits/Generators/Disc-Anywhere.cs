﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Vintagestory.ServerMods
{
    public class AnywhereDiscGenerator : DiscDepositGenerator
    {
        [JsonProperty]
        public NatFloat YPosRel;

        float step;

        public AnywhereDiscGenerator(ICoreServerAPI api, DepositVariant variant, LCGRandom depositRand, NormalizedSimplexNoise noiseGen) : base(api, variant, depositRand, noiseGen)
        {
        }

        public override void Init()
        {
            base.Init();

            YPosRel.avg *= Api.WorldManager.MapSizeY;
            YPosRel.var *= Api.WorldManager.MapSizeY;
        }

        protected override void beforeGenDeposit(IMapChunk mapChunk, BlockPos targetPos)
        {
            depthf = YPosRel.nextFloat(1, DepositRand);
            depthi = (int)depthf;

            targetPos.Y = depthi;

            step = (float)mapChunk.MapRegion.OreMapVerticalDistortTop.InnerSize / regionChunkSize;
            
        }

        protected override void loadYPosAndThickness(IMapChunk heremapchunk, int lx, int lz, BlockPos targetPos, double distanceToEdge)
        {
            double curTh = depoitThickness * GameMath.Clamp(distanceToEdge * 2 - 0.2, 0, 1);
            hereThickness = (int)curTh + ((DepositRand.NextDouble() < (curTh - (int)curTh)) ? 1 : 0);

            int yOff = (int)getDepositYDistort(targetPos, lx, lz, step, heremapchunk);
            
            targetPos.Y = depthi + yOff;
        }
    }
    }
