using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Explicity
{
    [HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
    public static class Patch_PawnRenderNode_Body_GraphicFor
    {
        public static bool Prefix(PawnRenderNode_Body __instance, ref Pawn pawn, ref Graphic __result)
        {
            if (!pawn.RaceProps.Humanlike || pawn.gender != Gender.Female || pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
                return true;

            BodyTypeDef bodyType = pawn.story.bodyType;
            if (bodyType == BodyTypeDefOf.Female || bodyType == BodyTypeDefOf.Male)
                return true;

            string bodyPath = $"{bodyType.bodyNakedGraphicPath}_Female";
            if (ContentFinder<Texture2D>.Get(bodyPath + "_south", false) == null)
                return true;

            __result = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(pawn), Vector2.one, __instance.ColorFor(pawn));

            return false;
        }
    }

    [HarmonyPatch(typeof(ShaderUtility), "GetSkinShader")]
    public static class Patch_ShaderUtility_GetSkinShader
    {
        public static bool IsDarkSkinned(Color color) => (color.r + color.g + color.b) < 250;

        public static void Postfix(ref Pawn pawn, ref Shader __result)
        {
            if (!pawn.RaceProps.Humanlike || __result != ShaderDatabase.CutoutSkin || !IsDarkSkinned(pawn.story.SkinColor))
                return;

            __result = ShaderDatabase.Cutout;
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", new Type[] { typeof(HediffDef), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) })]
    public static class Patch_Pawn_HealthTracker_AddHediff
    {
        public static readonly FieldInfo Access_Pawn = AccessTools.Field(typeof(Pawn_HealthTracker), "pawn");

        public static bool Prefix(Pawn_HealthTracker __instance, HediffDef def, BodyPartRecord part, ref Hediff __result)
        {
            if (def != ExplicityUtility.HediffDefOf.Lactating)
                return true;

            Pawn pawn = (Pawn)Access_Pawn.GetValue(__instance);

            part = ExplicityUtility.GetBodyPart(pawn, BodyPartDefOf.Explicity_Chest);

            Hediff hediff = HediffMaker.MakeHediff(def, pawn, part);
            __instance.AddHediff(hediff, part, null, null);
            __result = hediff;

            return false;
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) })]
    public static class Patch_PawnGenerator_GeneratePawn
    {
        public static void Postfix(ref Pawn __result)
        {
            if (ExplicityMod.GenderWorks)
            {

            }

            BodyPartRecord anus = ExplicityUtility.GetBodyPart(__result, BodyPartDefOf.Explicity_Anus);
            if (anus != null) ExplicityUtility.AddHediff(__result, HediffDefOf.Explicity_Anus, anus);

            //

            // Hediff phallor = ExplicityUtility.GetHediff(__result, Intimacy.HediffDefOf.SEX_Penis);
            // Hediff gestor = ExplicityUtility.GetHediff(__result, Intimacy.HediffDefOf.SEX_Womb);
            // bool isPhallor = phallor != null;
            // bool isGestor = gestor != null;

            // if (isPhallor) ExplicityUtility.SizeHediff(__result, phallor, Intimacy.BodyPartDefOf.SEX_Reproduction);
            // if (isGestor) ExplicityUtility.SizeHediff(__result, gestor, Intimacy.BodyPartDefOf.SEX_Reproduction);

            //

            // if (!ExplicityUtility.ShouldHaveBreasts(__result, isPhallor, isGestor))
            //     return;

            BodyPartRecord genitals = ExplicityUtility.GetBodyPart(__result, BodyPartDefOf.Explicity_Genitals);

            if (__result.gender != Gender.Female)
            {
                if (genitals != null) ExplicityUtility.AddHediff(__result, HediffDefOf.Explicity_Penis, genitals);
                return;
            }

            if (genitals != null) ExplicityUtility.AddHediff(__result, HediffDefOf.Explicity_Vagina, genitals);

            BodyPartRecord chest = ExplicityUtility.GetBodyPart(__result, BodyPartDefOf.Explicity_Chest);
            if (chest != null) ExplicityUtility.AddHediff(__result, HediffDefOf.Explicity_Breasts, chest);
        }
    }
}
