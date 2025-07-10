using Rage;
using System.Windows.Forms;

namespace CampusCallouts
{
    internal static class Settings
    {
        public static readonly InitializationFile ini = new InitializationFile(@"plugins/LSPDFR/CampusCallouts.ini");

        //Callouts
        public static readonly bool StudentsFighting = ini.ReadBoolean("Callouts", "StudentsFighting", true);
        public static readonly bool NoiseComplaint = ini.ReadBoolean("Callouts", "NoiseComplaint", true);
        public static readonly bool Stalking = ini.ReadBoolean("Callouts", "Stalking", true);
        public static readonly bool StudentEscort = ini.ReadBoolean("Callouts", "StudentEscort", true);
        public static readonly bool UnderageDrinking = ini.ReadBoolean("Callouts", "UnderageDrinking", true);
        public static readonly bool WeaponViolation = ini.ReadBoolean("Callouts", "WeaponViolation", true);
        public static readonly bool HitAndRun = ini.ReadBoolean("Callouts", "HitAndRun", true);
        public static readonly bool Trespasser = ini.ReadBoolean("Callouts", "Trespasser", true);
        public static readonly bool DroneUse = ini.ReadBoolean("Callouts", "DroneUse", true);
        public static readonly bool Vandalism = ini.ReadBoolean("Callouts", "Vandalism", true);
        public static readonly bool IntoxicatedStudent = ini.ReadBoolean("Callouts", "IntoxicatedStudent", true);
        public static readonly bool KillerClown = ini.ReadBoolean("Callouts", "KillerClown", true);
        public static readonly bool SchoolShooter = ini.ReadBoolean("Callouts", "SchoolShooter", true);
        public static readonly bool ProtestOnCampus = ini.ReadBoolean("Callouts", "ProtestOnCampus", true);

        //Keybinds 
        public static readonly Keys DialogueKey = ini.ReadEnum("Keybinds", "DialogueKey", Keys.Y);
        public static readonly Keys EndCallout = ini.ReadEnum("Keybinds", "EndCallout", Keys.End);

        //Preferences
        public static readonly bool UseBluelineAudio = ini.ReadBoolean("Preferences", "UseBluelineDispatchAudio", false);

    }
}
