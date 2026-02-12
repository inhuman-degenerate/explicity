using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace Explicity
{
    public class PawnRenderNodeWorker_Breasts : PawnRenderNodeWorker
    {
        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            float depth = (parms.facing == Rot4.North) ? -0.002f : 0.002f;
            pivot = PivotFor(node, parms);
            Vector3 vector = new Vector3(0, depth, 0);

            return vector;
        }

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            // if (!base.CanDrawNow(node, parms) || !ExplicityMod.Settings.DrawBodyParts)
            //     return false;

            if (!ExplicityMod.Settings.DrawBodyParts)
                return false;

            // if (ExplicityUtility.IsChestCovered(parms.pawn))
            //     return false;

            return true;
        }
    }

    public class PawnRenderNodeProperties_Breasts : PawnRenderNodeProperties
    {
        public PawnRenderNodeProperties_Breasts()
        {
            workerClass = typeof(PawnRenderNodeWorker_Breasts);
            nodeClass = typeof(PawnRenderNode_Breasts);
            visibleFacing = new List<Rot4> { Rot4.North, Rot4.East, Rot4.South, Rot4.West };
        }
    }

    public class PawnRenderNode_Breasts : PawnRenderNode
    {
        public PawnRenderNode_Breasts(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree) { }

        public override Graphic GraphicFor(Pawn pawn)
        {
            ExplicityHediff hediff = ExplicityUtility.GetHediff(pawn, HediffDefOf.Explicity_Breasts);
            if (hediff == null || ExplicityUtility.IsChestCovered(pawn))
                return null;

            string path = $"Things/Pawn/Humanlike/Breasts/Breasts_{pawn.story.bodyType.defName}_{hediff.Scale}";
            if (ContentFinder<Texture2D>.Get(path + "_south", false) == null)
                return null;

            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderUtility.GetSkinShader(pawn), Vector2.one, pawn.story.SkinColor);
        }
    }
}
