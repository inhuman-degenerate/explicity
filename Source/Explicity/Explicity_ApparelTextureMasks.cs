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
        }

        static void Postfix()
        {
            ApparelTextureCache.Apparel = null;
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
            if (!path.StartsWith("Things/Pawn/Humanlike/Apparel/") || ApparelTextureCache.Apparel == null || ApparelTextureCache.Apparel.Wearer == null)
                return true;

            ExplicityHediff hediff = ExplicityUtility.GetHediff(ApparelTextureCache.Apparel.Wearer, HediffDefOf.Explicity_Breasts);
            int breastSize = hediff?.Scale ?? -1;

            if (ApparelTextureCache.Graphics.TryGetValue((path, breastSize), out var graphic))
            {
                __result = graphic;
                return false;
            }

            if (!ApparelTextureCache.TryBake((path, breastSize), out var baked))
            {
                ApparelTextureCache.Failed.Add((path, breastSize));
                return true;
            }

            ApparelTextureCache.Graphics[(path, breastSize)] = baked;
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

    public class ApparelTextureCache
    {
        public static readonly Dictionary<(string, int), Graphic> Graphics = new Dictionary<(string, int), Graphic>();
        public static readonly HashSet<(string, int)> Failed = new HashSet<(string, int)>();
        public static Apparel Apparel;

        public static bool TryBake((string apparelPath, int breastSize) key, out Graphic result)
        {
            result = null;

            BodyTypeDef bodyType = Apparel.Wearer.story.bodyType;
            if (bodyType == null || bodyType == BodyTypeDefOf.Fat)
                return false;

            int apparelSize = 0;
            Texture2D[] apparelTextures = LoadTextures(key.apparelPath, ref apparelSize);
            if (apparelSize == 0 || apparelTextures == null)
                return false;

            string maskPath = $"Masks/Pawn/Humanlike/Bodies/Mask_{bodyType}_{key.breastSize}";
            maskPath = maskPath.Replace("_-1", "");
            string burnPath = maskPath.Replace("Mask_", "Burn_");

            Texture2D[] maskTextures = LoadTextures(maskPath, ref apparelSize);
            Texture2D[] burnTextures = LoadTextures(burnPath, ref apparelSize);
            if (maskTextures == null || burnTextures == null)
                return false;


            string breastPath = key.apparelPath.Replace($"_{bodyType}", "_Fat");
            Texture2D[] breastTextures = (key.breastSize != -1) ? LoadTextures(breastPath, ref apparelSize) : null;

            for (int i = 0; i < 3; i++)
            {
                Color32[] color = apparelTextures[i].GetPixels32();
                Color32[] mask = maskTextures[i].GetPixels32();
                Color32[] burn = burnTextures[i].GetPixels32();

                Color32[] breast = (breastTextures != null && (key.breastSize > 1 || i == 1 || bodyType != BodyTypeDefOf.Female)) ? breastTextures[i].GetPixels32() : null;

                for (int p = 0; p < color.Length; p++)
                {
                    bool green = mask[p].g > 0;

                    if (i == 0 && breast != null && green && mask[p].r != 255)
                    {
                        color[p] = Color32.Lerp(breast[p], new Color32(0, 0, 0, 255), 0.2f);
                    }

                    //

                    if (color[p].a > mask[p].r && !green) color[p].a = 0;

                    //

                    if (i == 1 && breast != null && green) color[p] = breast[p];
                    if (i == 2 && breast != null && green) color[p] = breast[p];

                    float redness = (burn[p].r / -255f) + 1f;
                    color[p].r = (byte)(color[p].r * redness);
                    color[p].g = (byte)(color[p].g * redness);
                    color[p].b = (byte)(color[p].b * redness);

                    if (color[p].a < burn[p].r) color[p].a = burn[p].r;
                }

                apparelTextures[i].SetPixels32(color);
                apparelTextures[i].Apply(true, false);
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

        private static Texture2D[] LoadTextures(string path, ref int size)
        {
            Texture2D[] textures =
            {
                ContentFinder<Texture2D>.Get(path + "_north", false),
                ContentFinder<Texture2D>.Get(path + "_east", false),
                ContentFinder<Texture2D>.Get(path + "_south", false),
                null
            };

            ReadTexture(ref textures[0], size);
            ReadTexture(ref textures[1], size);
            ReadTexture(ref textures[2], size);
            if (textures[0] == null || textures[1] == null || textures[2] == null)
                return null;

            if (size == 0) size = textures[0].width;
            if (size == 0)
                return null;

            return textures;
        }

        private static void ReadTexture(ref Texture2D texture, int reqSize)
        {
            reqSize = (reqSize > 0) ? reqSize : texture.width;

            if (texture == null)
                return;

            RenderTexture rt = RenderTexture.GetTemporary(reqSize, reqSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture prev = RenderTexture.active;
            UnityEngine.Graphics.Blit(texture, rt);
            RenderTexture.active = rt;

            texture = new Texture2D(reqSize, reqSize, TextureFormat.RGBA32, true);
            texture.ReadPixels(new Rect(0, 0, reqSize, reqSize), 0, 0);
            texture.Apply(true, false);

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
