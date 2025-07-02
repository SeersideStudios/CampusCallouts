using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Linq;

// [assembly: Rage.Attributes.Plugin("CampusCallouts", Description = "University Callouts ReMake for LSPDFR 0.4.9", Author = "SeersideStudios")] Is this really needed?
namespace CampusCallouts
{
    public class Main : Plugin
    {
        private static string modname = "Campus Callouts";
        private static string version = "1.0.0";
        private static string author = "Seerside Studios";
        public static bool CalloutInterface;

        public override void Initialize()
        {
            try
            {
                Settings.LoadSettings();
                Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;

            }
            catch (Exception ex)
            {
                Game.LogTrivial("CampusCallouts: An error occurred while initializing the plugin: " + ex.Message);
            }
        }

        public override void Finally()
        {
            Game.LogTrivial("CampusCallouts: Campus Callouts has been cleaned up.");
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                //Display Notification
                Game.DisplayNotification("~b~" + modname + " ~g~has loaded successfully!" + " ~w~ Today will be a normal School day...");

                //Log
                Game.LogTrivial(modname + " by " + author + " version " + version + " loaded.");

                //Register Callouts
                RegisterCallouts();
            }
        }

        private static void RegisterCallouts()
        {
            //CalloutInterface integration
            if (Functions.GetAllUserPlugins().ToList().Any(a => a != null && a.FullName.Contains("CalloutInterface")) == true)
            {
                Game.LogTrivial("User has Callout Interface installed.");
                CalloutInterface = true;
            }
            else
            {
                Game.LogTrivial("User does NOT have CalloutInterface installed.");
                CalloutInterface = false;
            }

            //Register Callouts Here
            if (Settings.UnderageDrinking) { Functions.RegisterCallout(typeof(Callouts.UnderageDrinking)); }
            if (Settings.StudentsFighting) { Functions.RegisterCallout(typeof(Callouts.StudentsFighting)); }
            if (Settings.NoiseComplaint) { Functions.RegisterCallout(typeof(Callouts.NoiseComplaint)); }
            if (Settings.StudentEscort) { Functions.RegisterCallout(typeof(Callouts.StudentEscort)); }
            if (Settings.Stalking) { Functions.RegisterCallout(typeof(Callouts.StalkingReport)); }
            if (Settings.WeaponViolation) { Functions.RegisterCallout(typeof(Callouts.WeaponViolation)); }
            if (Settings.HitAndRun) { Functions.RegisterCallout(typeof(Callouts.HitAndRun)); }
            if (Settings.Trespasser) { Functions.RegisterCallout(typeof(Callouts.Trespasser)); Game.LogTrivial("CampusCallouts - Trespasser Loaded"); }
            
        }
    }
}