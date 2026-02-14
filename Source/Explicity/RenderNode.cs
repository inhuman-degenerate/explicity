using Verse;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Explicity
{
    public class OffsetPosition
    {
        public BodyTypeDef BodyType;
        public float OffsetY;
        public float OffsetYSide;
        public float OffsetYNorth;
        public float OffsetXSide;
    }

    public class BodyTypeOffsetDef : Def
    {
        public float DepthNorth;
        public float Depth;
        public List<OffsetPosition> Offset;
    }

    public class PawnRenderNodeProperties_Explicity : PawnRenderNodeProperties
    {
        public BodyTypeOffsetDef BodyTypeOffset;
    }

    public class PawnRenderNodeWorker_Explicity : PawnRenderNodeWorker
    {
        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            PawnRenderNodeProperties_Explicity props = node.Props as PawnRenderNodeProperties_Explicity;

            pivot = PivotFor(node, parms);
            float depth = parms.facing == Rot4.North ? props.BodyTypeOffset.DepthNorth : props.BodyTypeOffset.Depth;
            float offsetY = 0f;
            float offsetX = 0f;

            OffsetPosition entry = props.BodyTypeOffset?.Offset.Find(x => x.BodyType == parms.pawn.story.bodyType);
            if (entry != null)
            {
                if (!parms.facing.IsHorizontal || (entry.OffsetYSide == 0f && entry.OffsetXSide == 0f))
                {
                    offsetY = (parms.facing == Rot4.North && entry.OffsetYNorth != 0f) ? entry.OffsetYNorth : entry.OffsetY;
                    return new Vector3(0f, depth, offsetY);
                }

                float dir = 0f;
                if (parms.facing == Rot4.East) dir = 1f;
                else if (parms.facing == Rot4.West) dir = -1f;

                offsetX = entry.OffsetXSide * dir;
                offsetY = (dir == 0f) ? entry.OffsetY : entry.OffsetYSide;
            }

            return new Vector3(offsetX, depth, offsetY);
        }
    }
}
