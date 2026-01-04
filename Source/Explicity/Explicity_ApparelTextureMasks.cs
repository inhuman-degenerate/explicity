using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Explicity
{
    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class Patch_TryGetGraphicApparel
    {
        static void Prefix(Apparel apparel, BodyTypeDef bodyType, bool forStatue)
        {
            ApparelTextureCache.Apparel = apparel;
            ApparelTextureCache.Pawn = apparel.Wearer;
            ExplicityHediff hediff = ExplicityUtility.GetHediff(ApparelTextureCache.Pawn, HediffDefOf.Explicity_Breasts);
            ApparelTextureCache.BreastSize = hediff?.Scale ?? -1;
        }

        static void Postfix()
        {
            ApparelTextureCache.Apparel = null;
            ApparelTextureCache.Pawn = null;
            ApparelTextureCache.BreastSize = -1;
        }
    }

    [HarmonyPatch]
    public static class GraphicDatabase_Get_Patch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get), new[] { typeof(string), typeof(Shader), typeof(Vector2), typeof(Color) })?.MakeGenericMethod(typeof(Graphic_Multi));
        }

        static bool Prefix(ref string path, ref Graphic __result)
        {
            if (!path.StartsWith("Things/Pawn/Humanlike/Apparel/") || ApparelTextureCache.Apparel == null || ApparelTextureCache.Pawn == null)
                return true;

            if (ApparelTextureCache.Graphics.TryGetValue((path, ApparelTextureCache.BreastSize), out var graphic))
            {
                __result = graphic;
                return false;
            }

            if (!ApparelTextureCache.TryBake((path, ApparelTextureCache.BreastSize), out var baked))
            {
                ApparelTextureCache.Failed.Add((path, ApparelTextureCache.BreastSize));
                return true;
            }

            ApparelTextureCache.Graphics[(path, ApparelTextureCache.BreastSize)] = baked;
            __result = baked;

            return false;
        }
    }

    //

    public class Graphic_Multi_Runtime : Graphic_Multi
    {
        private static readonly AccessTools.FieldRef<Graphic_Multi, Material[]> MatsField = AccessTools.FieldRefAccess<Graphic_Multi, Material[]>("mats");

        public void InitRuntime(
            Texture2D[] textures,
            Texture2D[] maskTextures,
            GraphicData graphicData,
            Shader shader,
            Color color,
            Color colorTwo,
            List<ShaderParameter> shaderParameters,
            int renderQueue
        )
        {
            data = graphicData;
            drawSize = graphicData.drawSize;
            this.color = color;
            this.colorTwo = colorTwo;

            Material[] mats = new Material[4];

            for (int i = 0; i < 4; i++)
            {
                MaterialRequest req = new MaterialRequest
                {
                    mainTex = textures[i],
                    maskTex = maskTextures?[i],
                    shader = shader,
                    color = color,
                    colorTwo = colorTwo,
                    shaderParameters = shaderParameters,
                    renderQueue = renderQueue
                };

                mats[i] = MaterialPool.MatFrom(req);
            }

            MatsField(this) = mats;
        }
    }

    public static class ApparelTextureCache
    {
        public static readonly Dictionary<(string, int), Graphic> Graphics = new Dictionary<(string, int), Graphic>();
        public static readonly HashSet<(string, int)> Failed = new HashSet<(string, int)>();
        public static Apparel Apparel;
        public static Pawn Pawn;
        public static int BreastSize;

        public static bool TryBake((string originalPath, int size) key, out Graphic result)
        {
            result = null;

            BodyTypeDef bodyType = Pawn.story.bodyType;
            if (bodyType == null)
                return false;

            // key.originalPath = key.originalPath.Replace(bodyType, "Fat");

            Texture2D sourceTexture = ContentFinder<Texture2D>.Get(key.originalPath + "_south", false);
            if (sourceTexture == null)
                return false;

            string maskPath = $"Masks/Pawn/Humanlike/Bodies/Mask_{bodyType.defName}";
            float stretch = 1f;
            if (BreastSize != -1)
            {
                maskPath += $"_{BreastSize}";
                stretch += 0.15f * BreastSize;
            }

            string burnPath = maskPath.Replace("Mask_", "Burn_");

            Texture2D maskTexture = ContentFinder<Texture2D>.Get(maskPath + "_south", false);
            if (maskTexture == null)
                return false;

            Texture2D burnTexture = ContentFinder<Texture2D>.Get(burnPath + "_south", false);
            if (burnTexture == null)
                return false;

            int size = 0;

            Texture2D[] apparelTextures =
            {
                CloneTexture(ContentFinder<Texture2D>.Get(key.originalPath + "_north", false), ref size, true, stretch),
                CloneTexture(ContentFinder<Texture2D>.Get(key.originalPath + "_east", false), ref size,false, stretch),
                CloneTexture(sourceTexture, ref size,false, stretch),
                null,
            };

            Texture2D[] maskTextures =
            {
                CloneTexture(ContentFinder<Texture2D>.Get(maskPath + "_north", false), ref size),
                CloneTexture(ContentFinder<Texture2D>.Get(maskPath + "_east", false), ref size),
                CloneTexture(maskTexture, ref size),
            };

            Texture2D[] burnTextures =
            {
                CloneTexture(ContentFinder<Texture2D>.Get(burnPath + "_north", false), ref size),
                CloneTexture(ContentFinder<Texture2D>.Get(burnPath + "_east", false), ref size),
                CloneTexture(burnTexture, ref size),
            };

            for (int i = 0; i < 3; i++)
            {
                Color32[] color = apparelTextures[i].GetPixels32();
                Color32[] mask = maskTextures[i].GetPixels32();
                Color32[] burn = burnTextures[i].GetPixels32();

                int len = color.Length;
                for (int p = 0; p < len; p++)
                {
                    if (color[p].a > mask[p].r) color[p].a = mask[p].r;

                    float redness = (burn[p].r / -255f) + 1f;

                    color[p].r = (byte)(color[p].r * redness);
                    color[p].g = (byte)(color[p].g * redness);
                    color[p].b = (byte)(color[p].b * redness);
                    if (color[p].a < burn[p].r) color[p].a = burn[p].r;
                }

                apparelTextures[i].SetPixels32(color);
                apparelTextures[i].Apply();
            }

            apparelTextures[3] = apparelTextures[1];

            Graphic_Multi_Runtime graphic = new Graphic_Multi_Runtime();
            graphic.InitRuntime(
                apparelTextures,
                null,
                Apparel.def.graphicData,
                Apparel.def.graphicData.shaderType.Shader,
                Apparel.DrawColor,
                Apparel.DrawColorTwo,
                Apparel.def.graphicData.shaderParameters,
                Apparel.def.graphicData.renderQueue
            );

            result = graphic;

            return true;
        }

        private static Texture2D CloneTexture(Texture2D source, ref int size, bool getter = false, float stretchFactor = 1f)
        {
            size = getter ? source.width : size;

            RenderTexture rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture prev = RenderTexture.active;
            UnityEngine.Graphics.Blit(source, rt);
            RenderTexture.active = rt;

            float scaleX = 1f / stretchFactor;
            float offsetX = (1f - scaleX) / 2f;

            UnityEngine.Graphics.Blit(source, rt, new Vector2(scaleX, 1f), new Vector2(offsetX, 0f));

            RenderTexture.active = rt;

            Texture2D readable = new Texture2D(size, size, TextureFormat.RGBA32, true);
            readable.ReadPixels(new Rect(0, 0, size, size), 0, 0);

            readable.Apply(false, false);

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            readable.filterMode = FilterMode.Bilinear;
            readable.wrapMode = TextureWrapMode.Clamp;

            return readable;
        }
    }
}
