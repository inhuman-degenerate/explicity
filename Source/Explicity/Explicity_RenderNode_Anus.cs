using Verse;
using UnityEngine;
using System.Collections.Generic;
using RimWorld;

namespace Explicity
{
    public class PawnRenderNodeWorker_Anus : PawnRenderNodeWorker
    {
        public static float GetPosition(BodyTypeDef body)
        {
            if (body == BodyTypeDefOf.Male)
                return -0.40f;
            else if (body == BodyTypeDefOf.Female)
                return -0.42f;
            else if (body == BodyTypeDefOf.Thin)
                return -0.40f;
            else if (body == BodyTypeDefOf.Hulk)
                return -0.58f;
            else if (body == BodyTypeDefOf.Fat)
                return -0.44f;

            return 0f;
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            pivot = PivotFor(node, parms);
            Vector3 vector = new Vector3(0, 0.003f, GetPosition(parms.pawn.story.bodyType));

            return vector;
        }

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms) || !ExplicityMod.Settings.DrawBodyParts)
                return false;

            return true;
        }
    }

    public class PawnRenderNodeProperties_Anus : PawnRenderNodeProperties
    {
        public PawnRenderNodeProperties_Anus()
        {
            workerClass = typeof(PawnRenderNodeWorker_Anus);
            nodeClass = typeof(PawnRenderNode_Anus);
            visibleFacing = new List<Rot4> { Rot4.North };
        }
    }

    public class PawnRenderNode_Anus : PawnRenderNode
    {
        public PawnRenderNode_Anus(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree) { }

        public override Graphic GraphicFor(Pawn pawn)
        {
            ExplicityHediff hediff = ExplicityUtility.GetHediff(pawn, HediffDefOf.Explicity_Anus);
            if (hediff == null || ExplicityUtility.IsChestCovered(pawn))
                return null;

            string path = $"Things/Pawn/Humanlike/Anus/Anus_{hediff.Scale}";
            if (ContentFinder<Texture2D>.Get(path, false) == null)
                return null;

            return GraphicDatabase.Get<Graphic_Single>(path, ShaderUtility.GetSkinShader(pawn), Vector2.one, pawn.story.SkinColor);
        }
    }
}
