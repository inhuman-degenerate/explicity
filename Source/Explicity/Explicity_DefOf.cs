using RimWorld;
using Verse;

namespace Explicity
{
    [DefOf]
    public static class BodyPartDefOf
    {
        public static BodyPartDef Explicity_Anus;
        public static BodyPartDef Explicity_Chest;
        public static BodyPartDef Explicity_Genitals;
    }

    [DefOf]
    public static class HediffDefOf
    {
        public static HediffDef Explicity_Anus;
        public static HediffDef Explicity_Breasts;
        public static HediffDef Explicity_Penis;
        public static HediffDef Explicity_Vagina;
    }

    public static class Intimacy
    {
        public static class NeedDefOf
        {
            public static readonly NeedDef SEX_Intimacy = DefDatabase<NeedDef>.GetNamed("SEX_Intimacy", false);
        }
    }

    public static class GenderWorks
    {
        public static class BodyPartDefOf
        {
            public static readonly BodyPartDef SEX_Reproductives = DefDatabase<BodyPartDef>.GetNamed("SEX_Reproductives", false);
        }

        public static class HediffDefOf
        {
            public static readonly HediffDef SEX_Penis = DefDatabase<HediffDef>.GetNamed("SEX_Penis", false);
            public static readonly HediffDef SEX_Womb = DefDatabase<HediffDef>.GetNamed("SEX_Womb", false);
        }
    }
}
