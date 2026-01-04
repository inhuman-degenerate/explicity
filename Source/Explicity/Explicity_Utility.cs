using RimWorld;
using Verse;
using System.Linq;

namespace Explicity
{
    public static class ExplicityUtility
    {
        public static class HediffDefOf
        {
            public static readonly HediffDef Lactating = HediffDef.Named("Lactating");
        }

        public static void AddHediff(Pawn pawn, HediffDef hediffDef, BodyPartRecord part)
        {
            if (pawn?.health?.hediffSet?.PartIsMissing(part) != false)
                return;

            if (pawn.health.hediffSet.HasHediff(hediffDef))
                return;

            Hediff hediff = pawn.health.AddHediff(hediffDef, part);
            SizeHediff(pawn, hediff, part.def);
        }

        public static BodyPartRecord GetBodyPart(Pawn pawn, BodyPartDef part)
        {
            BodyPartRecord bodyPart = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(p => p.def == part);

            return bodyPart;
        }

        public static ExplicityHediff GetHediff(Pawn pawn, HediffDef hediff)
        {
            return pawn.health.hediffSet.GetFirstHediffOfDef(hediff) as ExplicityHediff;
        }

        public static bool IsChestCovered(Pawn pawn)
        {
            if (pawn.apparel == null)
                return false;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel.def.apparel.bodyPartGroups?.Contains(BodyPartGroupDefOf.Torso) == true)
                    return true;
            }

            return false;
        }

        public static float BodyTypeSizeFactor(Pawn pawn, BodyPartDef part)
        {
            BodyTypeDef bodyType = pawn.story.bodyType;

            if (part == BodyPartDefOf.Explicity_Anus)
            {
                // if (bodyType == BodyTypeDefOf.Fat) return 1.0f;
                // else if (bodyType == BodyTypeDefOf.Female) return 0.9f;
                // else if (bodyType == BodyTypeDefOf.Hulk) return 0.8f;
                // else if (bodyType == BodyTypeDefOf.Male) return 0.8f;
                // else if (bodyType == BodyTypeDefOf.Thin) return 0.7f;
                // else return 1f;
                return 1f;
            }
            else if (part == BodyPartDefOf.Explicity_Chest)
            {
                if (bodyType == BodyTypeDefOf.Fat) return 1.1f;
                else if (bodyType == BodyTypeDefOf.Female) return 1.0f;
                else if (bodyType == BodyTypeDefOf.Hulk) return 0.9f;
                else if (bodyType == BodyTypeDefOf.Male) return 0.9f;
                else if (bodyType == BodyTypeDefOf.Thin) return 0.8f;
                else return 1f;
            }
            else if (part == Intimacy.BodyPartDefOf.SEX_Reproduction)
            {
                if (bodyType == BodyTypeDefOf.Fat) return 0.8f;
                else if (bodyType == BodyTypeDefOf.Female) return 1.0f;
                else if (bodyType == BodyTypeDefOf.Hulk) return 0.9f;
                else if (bodyType == BodyTypeDefOf.Male) return 1.0f;
                else if (bodyType == BodyTypeDefOf.Thin) return 0.9f;
                else return 1f;
            }

            return 0f;
        }

        public static void SizeHediff(Pawn pawn, Hediff hediff, BodyPartDef part, float baseSize = 0f)
        {
            if (baseSize == 0f) baseSize = Rand.Value;
            hediff.Severity = baseSize * BodyTypeSizeFactor(pawn, part);
        }

        public static bool ShouldHaveBreasts(Pawn pawn, bool phallor, bool gestor)
        {
            bool aphrodor = phallor && gestor;

            if (pawn.gender == Gender.Female)
            {
                if (aphrodor && ExplicityMod.Settings.BreastsOnFemaleAphrodors) return true;
                else if (phallor && ExplicityMod.Settings.BreastsOnFemalePhallors) return true;
                else if (gestor && ExplicityMod.Settings.BreastsOnFemaleGestors) return true;
                return true;
            }
            else if (pawn.gender == Gender.Male)
            {
                if (aphrodor && ExplicityMod.Settings.BreastsOnMaleAphrodors) return true;
                else if (phallor && ExplicityMod.Settings.BreastsOnMalePhallors) return true;
                else if (gestor && ExplicityMod.Settings.BreastsOnMaleGestors) return true;
                return false;
            }

            return true;
        }
    }
}
