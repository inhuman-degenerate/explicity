// using System.Collections.Generic;
// using System.Reflection;
// using HarmonyLib;
// using RimWorld;
// using UnityEngine;
// using Verse;

// namespace Explicity
// {
//     [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
//     public static class Patch_ApparelGraphicRecordGetter_TryGetGraphicApparel
//     {
//         static void Prefix(Apparel apparel, BodyTypeDef bodyType)
//         {
//             ApparelTextureCache.Key.Apparel = apparel;
//             ApparelTextureCache.Key.BodyType = bodyType.defName;
//             ApparelTextureCache.Key.BreastSize = ExplicityUtility.GetHediff(apparel.Wearer,
//             HediffDefOf.Explicity_Breasts)?.Scale ?? -1;
//         }
//     }

//     [HarmonyPatch]
//     public static class Patch_GraphicDatabase_Get
//     {
//         static MethodBase TargetMethod()
//         {
//             return AccessTools.Method(typeof(GraphicDatabase), "Get", new[] { typeof(string), typeof(Shader),
//             typeof(Vector2), typeof(Color) })?.MakeGenericMethod(typeof(Graphic_Multi));
//         }

//         static bool Prefix(ref string path, ref Graphic __result)
//         {
//             if (!path.StartsWith("Things/Pawn/Humanlike/Apparel/") || ApparelTextureCache.Key.Apparel == null ||
//             !ApparelTextureCache.Key.Apparel.def.apparel.tags.Contains("Explicity"))
//                 return true;

//             ApparelTextureCache.Key.ApparelPath = path;
//             if (ApparelTextureCache.Graphics.TryGetValue(ApparelTextureCache.Key.Data(), out var graphic))
//             {
//                 __result = graphic;
//                 return false;
//             }

//             if (!ApparelTextureCache.TryBake(ApparelTextureCache.Key, out var baked))
//             {
//                 ApparelTextureCache.Failed.Add(ApparelTextureCache.Key.Data());
//                 return true;
//             }

//             ApparelTextureCache.Graphics[ApparelTextureCache.Key.Data()] = baked;
//             __result = baked;

//             return false;
//         }
//     }

//     //

//     public class CustomApparelGraphic : Graphic_Multi
//     {
//         private static readonly AccessTools.FieldRef<Graphic_Multi, Material[]> MatsField =
//         AccessTools.FieldRefAccess<Graphic_Multi, Material[]>("mats");

//         public void Create(Texture2D[] textures)
//         {
//             Apparel apparel = ApparelTextureCache.Key.Apparel;
//             data = apparel.def.graphicData;
//             drawSize = data.drawSize;
//             color = apparel.DrawColor;
//             colorTwo = apparel.DrawColorTwo;

//             for (int i = 0; i < 4; i++)
//             {
//                 MaterialRequest req = new MaterialRequest
//                 {
//                     mainTex = textures[i],
//                     maskTex = null,
//                     shader = apparel.def.graphicData.shaderType.Shader,
//                     color = color,
//                     colorTwo = colorTwo,
//                     shaderParameters = apparel.def.graphicData.shaderParameters,
//                     renderQueue = apparel.def.graphicData.renderQueue
//                 };

//                 MatsField(this)[i] = MaterialPool.MatFrom(req);
//             }
//         }
//     }

//     public class ApparelTextureCacheKey
//     {
//         public Apparel Apparel;
//         public string BodyType;
//         public int BreastSize;
//         public string ApparelPath;

//         public (string, int) Data() => (ApparelPath, BreastSize);
//     }

//     public class ApparelTextureCache
//     {
//         public static readonly Dictionary<(string, int), Graphic> Graphics = new Dictionary<(string, int),
//         Graphic>(); public static readonly HashSet<(string, int)> Failed = new HashSet<(string, int)>(); public
//         static ApparelTextureCacheKey Key = new ApparelTextureCacheKey();

//         public static bool TryBake(ApparelTextureCacheKey key, out Graphic result)
//         {
//             result = null;

//             if (key.BodyType == null || key.BodyType == "Fat")
//                 return false;

//             int apparelSize = 0;
//             Texture2D[] apparelTextures = LoadTextures(key.ApparelPath, ref apparelSize);
//             if (apparelSize == 0 || apparelTextures == null)
//                 return false;

//             string maskPath = $"Masks/Pawn/Humanlike/Bodies/Mask_{key.BodyType}_{key.BreastSize}".Replace("_-1", "");

//             Texture2D[] maskTextures = LoadTextures(maskPath, ref apparelSize);
//             if (maskTextures == null)
//                 return false;

//             // string breastPath = key.ApparelPath.Replace($"_{bodyType}", "_Fat");
//             // Texture2D[] breastTextures = (key.BreastSize != -1) ? LoadTextures(breastPath, ref apparelSize) :
//             null;

//             for (int i = 0; i < 3; i++)
//             {
//                 Color32[] color = apparelTextures[i].GetPixels32();
//                 Color32[] mask = maskTextures[i].GetPixels32();
//                 Color32 black = new Color32(0, 0, 0, 255);

//                 // Color32[] breast = (breastTextures != null && (key.BreastSize > 1 || i == 1 || bodyType !=
//                 BodyTypeDefOf.Female)) ? breastTextures[i].GetPixels32() : null;

//                 for (int p = 0; p < color.Length; p++)
//                 {
//                     bool red = mask[p].r > 0;
//                     bool blue = mask[p].b > 0;

//                     if (red && !blue) continue;

//                     if (!red)
//                     {
//                         color[p] = black;
//                         color[p].a = 0;
//                     }

//                     if (blue)
//                     {
//                         if (color[p].a == 0) color[p].a = mask[p].b;
//                         else
//                         {
//                             float fblue = (255 - mask[p].b) / 255f;
//                             color[p].r = (byte)(color[p].r * fblue);
//                             color[p].g = (byte)(color[p].g * fblue);
//                             color[p].b = (byte)(color[p].b * fblue);
//                         }
//                     }

//                     // bool green = mask[p].g > 0;

//                     // if (i == 0 && breast != null && green && mask[p].r != 255)
//                     // {
//                     //     color[p] = Color32.Lerp(breast[p], new Color32(0, 0, 0, 255), 0.2f);
//                     // }

//                     //

//                     // if (color[p].a > mask[p].r && !green) color[p].a = 0;

//                     // Red channel is used for apparel cutout

//                     //

//                     // if (i == 1 && breast != null && green) color[p] = breast[p];
//                     // if (i == 2 && breast != null && green) color[p] = breast[p];

//                     // float redness = (burn[p].r / -255f) + 1f;
//                     // color[p].r = (byte)(color[p].r * redness);
//                     // color[p].g = (byte)(color[p].g * redness);
//                     // color[p].b = (byte)(color[p].b * redness);

//                     // if (color[p].a < burn[p].r) color[p].a = burn[p].r;
//                     // Blue channel is used for black outlines and inlines
//                     // if (mask[p].b > 0)
//                     // {
//                     //     // float blue = mask[p].b / 255f;
//                     //     // color[p] = Color32.Lerp(color[p], black, blue);
//                     //     color[p] = mask[p].b;
//                     //     if (color[p].a < mask[p].b) color[p].a = mask[p].b;
//                     // }
//                 }

//                 apparelTextures[i].SetPixels32(color);
//                 apparelTextures[i].Apply(true, false);
//             }

//             apparelTextures[3] = apparelTextures[1];

//             CustomApparelGraphic graphic = new CustomApparelGraphic();
//             graphic.Create(apparelTextures);

//             result = graphic;

//             return true;
//         }

//         private static Texture2D[] LoadTextures(string path, ref int size)
//         {
//             Texture2D[] textures =
//             {
//                 ContentFinder<Texture2D>.Get(path + "_north", false),
//                 ContentFinder<Texture2D>.Get(path + "_east", false),
//                 ContentFinder<Texture2D>.Get(path + "_south", false),
//                 null
//             };

//             ReadTexture(ref textures[0], size);
//             ReadTexture(ref textures[1], size);
//             ReadTexture(ref textures[2], size);
//             if (textures[0] == null || textures[1] == null || textures[2] == null)
//                 return null;

//             if (size == 0) size = textures[0].width;
//             if (size == 0)
//                 return null;

//             return textures;
//         }

//         private static void ReadTexture(ref Texture2D texture, int reqSize)
//         {
//             reqSize = (reqSize > 0) ? reqSize : texture.width;

//             if (texture == null)
//                 return;

//             RenderTexture rt = RenderTexture.GetTemporary(reqSize, reqSize, 0, RenderTextureFormat.ARGB32,
//             RenderTextureReadWrite.sRGB); RenderTexture prev = RenderTexture.active;
//             UnityEngine.Graphics.Blit(texture, rt);
//             RenderTexture.active = rt;

//             texture = new Texture2D(reqSize, reqSize, TextureFormat.RGBA32, true);
//             texture.ReadPixels(new Rect(0, 0, reqSize, reqSize), 0, 0);
//             texture.Apply(true, false);

//             RenderTexture.active = prev;
//             RenderTexture.ReleaseTemporary(rt);
//             texture.filterMode = FilterMode.Bilinear;
//             texture.wrapMode = TextureWrapMode.Clamp;
//         }
//     }
// }
