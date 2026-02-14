using Verse;
using UnityEngine;
using System.Collections.Generic;
using RimWorld;

namespace Explicity
{
    public class PawnRenderNodeWorker_Vagina : PawnRenderNodeWorker_Explicity
    {
        // public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        // {
        //     pivot = PivotFor(node, parms);
        //     float offsetY = 0f;

        //     PawnRenderNodeProperties_Vagina props = node.Props as PawnRenderNodeProperties_Vagina;
        //     if (props.TryGetOffset(parms.pawn.story.bodyType, out var values))
        //     {
        //         offsetY = parms.facing == Rot4.North ? values.OffsetYNorth : values.OffsetY;
        //     }

        //     return new Vector3(0, 0.002f, offsetY);
        // }

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms) || !ExplicityMod.Settings.DrawBodyParts)
                return false;

            return true;
        }
    }

    public class PawnRenderNodeProperties_Vagina : PawnRenderNodeProperties_Explicity
    {
        public PawnRenderNodeProperties_Vagina()
        {
            workerClass = typeof(PawnRenderNodeWorker_Vagina);
            nodeClass = typeof(PawnRenderNode_Vagina);
            visibleFacing = new List<Rot4> { Rot4.North, Rot4.South };
        }

        // public class OffsetPosition
        // {
        //     public BodyTypeDef BodyType;
        //     public float OffsetY;
        //     public float OffsetYNorth;
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

    public class PawnRenderNode_Vagina : PawnRenderNode
    {
        public PawnRenderNode_Vagina(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree) { }

        public override Graphic GraphicFor(Pawn pawn)
        {
            HediffDef hediffDef = ExplicityMod.GenderWorks ? GenderWorks.HediffDefOf.SEX_Womb : HediffDefOf.Explicity_Vagina;
            ExplicityHediff hediff = ExplicityUtility.GetHediff(pawn, hediffDef);
            if (hediff == null)
                return null;

            string path = $"Things/Pawn/Humanlike/Vagina/Vagina_{hediff.Scale}";
            if (ContentFinder<Texture2D>.Get(path + "_south", false) == null)
                return null;

            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderUtility.GetSkinShader(pawn), Vector2.one, pawn.story.SkinColor);
        }
    }
}
