using Rage;
using System.Windows.Forms;

namespace CampusCallouts
{
    internal static class Settings
    {
        public static readonly InitializationFile ini = new InitializationFile(@"plugins/LSPDFR/CampusCallouts.ini");

        //Callouts
        public static readonly bool StudentsFighting = ini.ReadBoolean("Callouts", "Students Fighting", true);
        public static readonly bool NoiseComplaint = ini.ReadBoolean("Callouts", "Noise Complaint", true);
        public static readonly bool Stalking = ini.ReadBoolean("Callouts", "Stalking", true);
        public static readonly bool StudentEscort = ini.ReadBoolean("Callouts", "Student Escort", true);
        public static readonly bool UnderageDrinking = ini.ReadBoolean("Callouts", "Underage Drinking", true);
        public static readonly bool WeaponViolation = ini.ReadBoolean("Callouts", "Weapon Violation", true);
        public static readonly bool HitAndRun = ini.ReadBoolean("Callouts", "Hit And Run", true);
        public static readonly bool Trespasser = ini.ReadBoolean("Callouts", "Trespasser", true);
        public static readonly bool DroneUse = ini.ReadBoolean("Callouts", "Drone Use", true);
        public static readonly bool Vandalism = ini.ReadBoolean("Callouts", "Vandalism", true);
        public static readonly bool IntoxicatedStudent = ini.ReadBoolean("Callouts", "Intoxicated Student", true);

        //Dialogue Key
        public static readonly Keys DialogueKey = ini.ReadEnum("Keybinds", "Dialogue Key", Keys.Y);
        public static readonly Keys EndCallout = ini.ReadEnum("Keybinds", "End Callout", Keys.End);

    }
}
