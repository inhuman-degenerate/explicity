using Verse;
using UnityEngine;
using System.Collections.Generic;
using RimWorld;

namespace Explicity
{
    public class PawnRenderNodeWorker_Penis : PawnRenderNodeWorker
    {
        public static Vector2 GetPosition(BodyTypeDef body, Rot4 facing)
        {
            float offsetX = 0f;
            float offsetY = 0f;

            float dir = 0f;
            if (facing == Rot4.East) dir = 1f;
            else if (facing == Rot4.West) dir = -1f;

            if (body == BodyTypeDefOf.Male)
            {
                offsetX = 0.08f * dir;
                offsetY = -0.46f;
                if (dir != 0) offsetY += 0.06f;
            }
            else if (body == BodyTypeDefOf.Thin)
            {
                offsetX = 0.08f * dir;
                offsetY = -0.46f;
                if (dir != 0) offsetY += 0.04f;
            }

            return new Vector2(offsetX, offsetY);
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            float width = 0f;
            float height = 0f;
            if (Current.Game != null)
            {
                var comp = Current.Game.GetComponent<MyGameComponent>();

                if (comp == null)
                {
                    comp = new MyGameComponent(Current.Game);
                    Current.Game.components.Add(comp);
                }

                width = comp.width;
                height = comp.height;
            }

            pivot = PivotFor(node, parms);
            float depth = (parms.facing == Rot4.North) ? 0.001f : 0.003f;

            Vector2 position = GetPosition(parms.pawn.story.bodyType, parms.facing);

            Vector3 vector = new Vector3(width + position.x, depth, height + position.y);

            return vector;
        }

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!ExplicityMod.Settings.DrawBodyParts)
                return false;

            return true;
        }
    }

    public class PawnRenderNodeProperties_Penis : PawnRenderNodeProperties
    {
        public PawnRenderNodeProperties_Penis()
        {
            workerClass = typeof(PawnRenderNodeWorker_Penis);
            nodeClass = typeof(PawnRenderNode_Penis);
            visibleFacing = new List<Rot4> { Rot4.North, Rot4.East, Rot4.South, Rot4.West };
        }
    }

    public class PawnRenderNode_Penis : PawnRenderNode
    {
        public PawnRenderNode_Penis(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree) { }

        public override Graphic GraphicFor(Pawn pawn)
        {
            ExplicityHediff hediff = ExplicityUtility.GetHediff(pawn, HediffDefOf.Explicity_Penis);
            if (hediff == null || ExplicityUtility.IsChestCovered(pawn))
                return null;

            string path = $"Things/Pawn/Humanlike/Penis/Penis_{hediff.Scale}";
            if (ContentFinder<Texture2D>.Get(path + "_south", false) == null)
                return null;

            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderUtility.GetSkinShader(pawn), Vector2.one, pawn.story.SkinColor);
        }
    }
}
