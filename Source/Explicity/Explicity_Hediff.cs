using System;
using LoveyDoveySexWithRosaline;
using Verse;

namespace Explicity
{
    public static class SizeLabels
    {
        public static string[] LabelHoles = { "micro", "tight", "average", "comfortable", "loose" };
        public static string[] LabelExternals = { "tiny", "small", "average", "large", "huge" };
    }

    public class ExplicityHediff : Hediff_Reproductive
    {
        public string Name => def.label;

        private int ScaleCache = -1;
        public int Scale
        {
            get
            {
                if (ScaleCache != -1 && !pawn.IsHashIntervalTick(600))
                    return ScaleCache;

                ScaleCache = GetScale();
                return ScaleCache;
            }
        }

        private int GetScale()
        {
            float growth = Math.Clamp(pawn.ageTracker.AgeBiologicalYears / (pawn.RaceProps.lifeExpectancy * 0.25f), 0.01f, 1f);
            float size = Severity * growth;
            if (size <= 0.05) return 0;     // 0 = smallest 0.00 - 0.05 (5%)
            if (size <= 0.3) return 1;      // 1 = small    0.05 - 0.30 (25%)
            if (size <= 0.7) return 2;      // 2 = average  0.30 - 0.70 (40%)
            if (size <= 0.95) return 3;     // 3 = large    0.70 - 0.95 (25%)
            return 4;                       // 4 = largest  0.95 - 1.00 (5%)
        }

        public override bool Visible
        {
            get => ExplicityMod.Settings.ShowHediffs;
        }
    }

    public class AnusHediff : ExplicityHediff
    {
        public override string LabelBase
        {
            get => $"{Name} ({SizeLabels.LabelHoles[Scale]})";
        }
    }

    public class BreastsHediff : ExplicityHediff
    {
        public override string LabelBase
        {
            get => $"{Name} ({SizeLabels.LabelExternals[Scale]})";
        }
    }

    public class PenisHediff : ExplicityHediff
    {
        public override string LabelBase
        {
            get => $"{Name} ({SizeLabels.LabelExternals[Scale]})";
        }
    }

    public class VaginaHediff : ExplicityHediff
    {
        public override string LabelBase
        {
            get => $"{Name} ({SizeLabels.LabelHoles[Scale]})";
        }
    }
}
