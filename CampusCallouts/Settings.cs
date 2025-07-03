using Rage;

namespace CampusCallouts
{
    internal static class Settings
    {
        public static readonly InitializationFile ini = new InitializationFile(@"plugins/LSPDFR/CampusCallouts.ini");

        public static readonly bool StudentsFighting = ini.ReadBoolean("Callouts", "StudentsFighting", true);
        public static readonly bool NoiseComplaint = ini.ReadBoolean("Callouts", "NoiseComplaint", true);
        public static readonly bool Stalking = ini.ReadBoolean("Callouts", "Stalking", true);
        public static readonly bool StudentEscort = ini.ReadBoolean("Callouts", "StudentEscort", true);
        public static readonly bool UnderageDrinking = ini.ReadBoolean("Callouts", "UnderageDrinking", true);
        public static readonly bool WeaponViolation = ini.ReadBoolean("Callouts", "WeaponViolation", true);
        public static readonly bool HitAndRun = ini.ReadBoolean("Callouts", "HitAndRun", true);
        public static readonly bool Trespasser = ini.ReadBoolean("Callouts", "Trespasser", true);
        public static readonly bool TestCallout = ini.ReadBoolean("Callouts", "TestCallout", true);

    }
}
