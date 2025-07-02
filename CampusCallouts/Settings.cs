using System.Windows.Forms;
using CampusCallouts.Callouts;
using Rage;

namespace CampusCallouts
{
    internal static class Settings
    {
        internal static bool StudentsFighting;
        internal static bool NoiseComplaint;
        internal static bool Stalking;
        internal static bool StudentEscort;
        internal static bool UnderageDrinking;
        internal static bool WeaponViolation;
        internal static bool HitAndRun;
        internal static bool Trespasser;

        internal static void LoadSettings()
        {
            string path = "plugins/LSPDFR/CampusCallouts.ini";
            InitializationFile ini = new InitializationFile(path);
            ini.Create();

            StudentsFighting = ini.Read<bool>("Callouts", "StudentsFighting", true);
            NoiseComplaint = ini.Read<bool>("Callouts", "NoiseComplaint", true);
            Stalking = ini.Read<bool>("Callouts", "Stalking", true);
            StudentEscort = ini.Read<bool>("Callouts", "StudentEscort", true);
            UnderageDrinking = ini.Read<bool>("Callouts", "UnderageDrinking", true);
            WeaponViolation = ini.Read<bool>("Callouts", "WeaponViolation", true);
            HitAndRun = ini.Read<bool>("Callouts", "HitAndRun", true);
            Trespasser = ini.Read<bool>("Callouts", "Trespasser", true);
        }
    }
}
