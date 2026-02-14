using Verse;
using UnityEngine;
using System.Collections.Generic;
using RimWorld;

namespace Explicity
{
    public class PawnRenderNodeWorker_Penis : PawnRenderNodeWorker_Explicity
    {
        // public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        // {
        //     pivot = PivotFor(node, parms);
        //     float depth = (parms.facing == Rot4.North) ? 0.001f : 0.003f;
        //     float offsetX = 0f;
        //     float offsetY = 0f;

        //     PawnRenderNodeProperties_Penis props = node.Props as PawnRenderNodeProperties_Penis;
        //     if (props.TryGetOffset(parms.pawn.story.bodyType, out var values))
        //     {
        //         float dir = 0f;
        //         if (parms.facing == Rot4.East) dir = 1f;
        //         else if (parms.facing == Rot4.West) dir = -1f;

        //         offsetX = values.OffsetXSide * dir;
        //         offsetY = (dir == 0f) ? values.OffsetY : values.OffsetYSide;
        //     }

        //     return new Vector3(offsetX, depth, offsetY);

        //     // float width = 0f;
        //     // float height = 0f;
        //     // if (Current.Game != null)
        //     // {
        //     //     var comp = Current.Game.GetComponent<MyGameComponent>();

        //     //     if (comp == null)
        //     //     {
        //     //         comp = new MyGameComponent(Current.Game);
        //     //         Current.Game.components.Add(comp);
        //     //     }

        //     //     width = comp.width;
        //     //     height = comp.height;
        //     // }


        //     // Vector2 position = GetPosition(parms.pawn.story.bodyType, parms.facing);

        //     // Vector3 vector2 = new Vector3(width + position.x, depth, height + position.y);

        //     // return vector2;
        // }

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms) || !ExplicityMod.Settings.DrawBodyParts)
                return false;

            return true;
        }
    }

    public class PawnRenderNodeProperties_Penis : PawnRenderNodeProperties_Explicity
    {
        public PawnRenderNodeProperties_Penis()
        {
            workerClass = typeof(PawnRenderNodeWorker_Penis);
            nodeClass = typeof(PawnRenderNode_Penis);
            visibleFacing = new List<Rot4> { Rot4.North, Rot4.East, Rot4.South, Rot4.West };
        }

        // public class OffsetPosition
        // {
        //     public BodyTypeDef BodyType;
        //     public float OffsetY;
        //     public float OffsetYSide;
        //     public float OffsetXSide;
        // }

        // public List<OffsetPosition> BodyTypeOffset;
        // public Dictionary<BodyTypeDef, OffsetPosition> CachedOffsets;

        // public bool TryGetOffset(BodyTypeDef bodyType, out OffsetPosition result)
        // {
        //     if (CachedOffsets == null)
        //     {
        //         CachedOffsets = new Dictionary<BodyTypeDef, OffsetPosition>();
        //         foreach (var entry in BodyTypeOffset)
        //         {
        //             if (entry == null || entry.BodyType == null)
        //                 continue;

        //             CachedOffsets[entry.BodyType] = entry;
        //         }
        //     }

        //     return CachedOffsets.TryGetValue(bodyType, out result);
        // }
    }

    public class PawnRenderNode_Penis : PawnRenderNode
    {
        public PawnRenderNode_Penis(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree) { }

        public override Graphic GraphicFor(Pawn pawn)
        {
            HediffDef hediffDef = ExplicityMod.GenderWorks ? GenderWorks.HediffDefOf.SEX_Penis : HediffDefOf.Explicity_Penis;
            ExplicityHediff hediff = ExplicityUtility.GetHediff(pawn, hediffDef);
            if (hediff == null)
                return null;

            string path = $"Things/Pawn/Humanlike/Penis/Penis_{hediff.Scale}";
            if (ContentFinder<Texture2D>.Get(path + "_south", false) == null)
                return null;

            if (ExplicityMod.Intimacy && pawn.needs?.TryGetNeed(Intimacy.NeedDefOf.SEX_Intimacy)?.CurLevel < 0.2f)
            {
                string pathAlt = $"Things/Pawn/Humanlike/Penis/Penis_Horny_{hediff.Scale}";
                if (ContentFinder<Texture2D>.Get(pathAlt + "_south", false) != null)
                {
                    path = pathAlt;
                }
            }

            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderUtility.GetSkinShader(pawn), Vector2.one, pawn.story.SkinColor);
        }
    }
}
