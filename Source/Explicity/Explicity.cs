using Verse;
using HarmonyLib;
using System.Collections.Generic;

namespace Explicity
{
    public class ExplicityMod : Mod
    {
        public static ExplicitySettings Settings;
        // public static bool Turk;

        public ExplicityMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ExplicitySettings>();
            // Turk = LoadedModManager.RunningModsListForReading.Any(m => m.PackageId == "author.othermod");

            var harmony = new Harmony("InhumanDegenerate.Explicity");
            harmony.PatchAll();
        }

        public override string SettingsCategory()
        {
            return "Explicity for Intimacy";
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.CheckboxLabeled("Draw sexual body parts", ref Settings.DrawBodyParts);
            listing.GapLine();

            listing.Label("Female Breasts:");
            listing.CheckboxLabeled("Female phallors(penis carrier) have breasts", ref Settings.BreastsOnFemalePhallors);
            listing.CheckboxLabeled("Female gestors(womb carrier) have breasts", ref Settings.BreastsOnFemaleGestors);
            listing.CheckboxLabeled("Female aphrodors(both carrier) have breasts", ref Settings.BreastsOnFemaleAphrodors);
            listing.GapLine();

            listing.Label("Male Breasts:");
            listing.CheckboxLabeled("Male phallors(penis carrier) have breasts", ref Settings.BreastsOnMalePhallors);
            listing.CheckboxLabeled("Male gestors(womb carrier) have breasts", ref Settings.BreastsOnMaleGestors);
            listing.CheckboxLabeled("Male aphrodors(both carrier) have breasts", ref Settings.BreastsOnMaleAphrodors);
            listing.GapLine();

            listing.Label("Debug:");
            listing.CheckboxLabeled("Show sexual body parts hediffs", ref Settings.ShowHediffs);
            listing.GapLine();

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }
    }

    public class ExplicitySettings : ModSettings
    {
        public bool DrawBodyParts = true;
        public bool BreastsOnMalePhallors = false;
        public bool BreastsOnMaleGestors = false;
        public bool BreastsOnMaleAphrodors = false;
        public bool BreastsOnFemalePhallors = true;
        public bool BreastsOnFemaleGestors = true;
        public bool BreastsOnFemaleAphrodors = true;
        public bool ShowHediffs = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref DrawBodyParts, "DrawBodyParts", true);
            Scribe_Values.Look(ref BreastsOnMalePhallors, "BreastsOnMalePhallors", false);
            Scribe_Values.Look(ref BreastsOnMaleGestors, "BreastsOnMaleGestors", false);
            Scribe_Values.Look(ref BreastsOnMaleAphrodors, "BreastsOnMaleAphrodors", false);
            Scribe_Values.Look(ref BreastsOnFemalePhallors, "BreastsOnFemalePhallors", true);
            Scribe_Values.Look(ref BreastsOnFemaleGestors, "BreastsOnFemaleGestors", true);
            Scribe_Values.Look(ref BreastsOnFemaleAphrodors, "BreastsOnFemaleAphrodors", true);
            Scribe_Values.Look(ref ShowHediffs, "ShowHediffs", true);
            base.ExposeData();
        }
    }
}
