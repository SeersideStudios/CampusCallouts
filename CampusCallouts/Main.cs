using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;


[assembly: Rage.Attributes.Plugin("CampusCallouts", Description = "A remake of the University Callouts plugin", Author = "Seerside Studios")]

namespace CampusCallouts
{
    public class Main : Plugin
    {
        public static Version LatestVersion = new Version();
        public static Version UserVersion = new Version("1.0.2");
        public static bool UpToDate;
        public static bool CalloutInterface;
        public static bool Beta = false;

        public override void Initialize()
        {
            try
            {
                Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
                Game.LogTrivial("CampusCallouts: Campus Callouts version " + UserVersion + " has been loaded.");
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
            // Huge Thanks to Yob1n for this section of the code
                if (OnDuty)
                {
                    int num = (int)Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Campus Callouts", "~y~v." + UserVersion + " ~b~by Seerside Studios", " ~g~Loaded Successfully. ~b~Have a good day at School!");
                    GameFiber.StartNew(delegate
                    {
                        Game.LogTrivial("CampusCallouts: Plugin initialized, checking for updates.");
                        try
                        {
                            Thread FetchVersionThread = new Thread(() =>
                            {
                                using (WebClient client = new WebClient())
                                {
                                    try
                                    {
                                        string s = client.DownloadString("https://raw.githubusercontent.com/SeersideStudios/CampusCallouts/refs/heads/master/version.txt");

                                        LatestVersion = new Version(s);
                                    }
                                    catch (Exception) { Game.LogTrivial("CampusCallouts: GitHub version link down. Version UNVERIFIED."); }
                                }
                            });
                            FetchVersionThread.Start();
                            try
                            {
                                while (FetchVersionThread.ThreadState != System.Threading.ThreadState.Stopped)
                                {
                                    GameFiber.Yield();
                                }
                                // compare the versions  
                                if (UserVersion.CompareTo(LatestVersion) < 0)
                                {
                                    Game.LogTrivial("CampusCallouts: Completed update check.");
                                    Game.LogTrivial("CampusCallouts: Update Available for Campus Callouts. Installed Version " + UserVersion + " ,New Version " + LatestVersion);
                                    Game.DisplayNotification("~r~IMPORTANT:~w~ A new version of ~b~CampusCallouts~w~ is available.\n\n~y~Installed:~w~ v" + UserVersion + "\n~y~Latest:~w~ v" + LatestVersion + "\n\n~r~Please update to ensure optimal performance and prevent issues.");
                                    Game.LogTrivial("====================CAMPUSCALLOUTS WARNING====================");
                                    Game.LogTrivial("Outdated CampusCallouts Version. Please update as soon as possible for the best compatibility!");
                                    Game.LogTrivial("====================CAMPUSCALLOUTS WARNING====================");
                                    UpToDate = false;
                                }
                                else if (UserVersion.CompareTo(LatestVersion) > 0)
                                {
                                    Game.LogTrivial("CampusCallouts: DETECTED BETA RELEASE. DO NOT REDISTRIBUTE. PLEASE REPORT ALL ISSUES.");
                                    Game.DisplayNotification("CampusCallouts: ~r~DETECTED BETA RELEASE. ~w~DO NOT REDISTRIBUTE. PLEASE REPORT ALL ISSUES.");
                                    UpToDate = true;
                                    Beta = true;
                                }
                                else
                                {
                                    Game.LogTrivial("CampusCallouts: Completed update check.");
                                    Game.DisplayNotification("You are on the ~g~Latest Version~w~ of ~b~CampusCallouts.");
                                    Game.LogTrivial("CampusCallouts: Latest version is downloaded!");
                                    UpToDate = true;
                                }
                            }
                            catch (Exception)
                            {
                                Game.LogTrivial("CampusCallouts: Error while Processing Thread to Check for Updates.");
                            }
                        }
                        catch (Exception)
                        {
                            Game.LogTrivial("CampusCallouts: Error while checking Campus Callouts for updates.");
                        }
                    });
                    RegisterCallouts();
                }
        }

        private static void RegisterCallouts()
        {
            Game.LogTrivial("====================CAMPUSCALLOUTS CALLOUTS REGISTRATION====================");

            // Check for required DLLs
            string gtaFolder = System.IO.Directory.GetCurrentDirectory(); 
            string calloutInterfacePath = System.IO.Path.Combine(gtaFolder, "CalloutInterfaceAPI.dll");
            string naudioPath = System.IO.Path.Combine(gtaFolder, "NAudio.dll");

            if (System.IO.File.Exists(calloutInterfacePath))
                Game.LogTrivial("CampusCallouts: CalloutInterfaceAPI.dll found in main directory.");
            else
                Game.LogTrivial("CampusCallouts: CalloutInterfaceAPI.dll NOT found in main directory.");

            if (System.IO.File.Exists(naudioPath))
                Game.LogTrivial("CampusCallouts: NAudio.dll found in main directory.");
            else
                Game.LogTrivial("CampusCallouts: NAudio.dll NOT found in main directory.");

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

            //Check for INI file
            if (Settings.ini.Exists()) { Game.LogTrivial("CampusCallouts.ini is installed."); }
            else { Game.LogTrivial("CampusCallouts.ini is NOT installed"); }

            //Checks for BluelineAudio Preference
            if (Settings.UseBluelineAudio) {                 
                Game.LogTrivial("CampusCallouts: Using BluelineAudio for Callouts.");
            }
            else
            {
                Game.LogTrivial("CampusCallouts: Using LSPDFR/CC Audio for Callouts.");
            }

            //Register Callouts Here
            Game.LogTrivial("Started Registering Callouts.");
            if (Settings.UnderageDrinking || !Settings.ini.Exists())  Functions.RegisterCallout(typeof(Callouts.UnderageDrinking)); 
            if (Settings.StudentsFighting || !Settings.ini.Exists())  Functions.RegisterCallout(typeof(Callouts.StudentsFighting)); 
            if (Settings.NoiseComplaint || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.NoiseComplaint));
            if (Settings.StudentEscort || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.StudentEscort));
            if (Settings.Stalking || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.StalkingReport));
            if (Settings.WeaponViolation || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.WeaponViolation));
            if (Settings.HitAndRun || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.HitAndRun));        
            if (Settings.Trespasser || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.Trespasser));
            if (Settings.DroneUse || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.DroneUse));
            if (Settings.Vandalism || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.Vandalism));
            if (Settings.IntoxicatedStudent || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.IntoxicatedStudent));
            if (Settings.KillerClown || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.KillerClown));
            if (Settings.SchoolShooter || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.SchoolShooter));
            if (Settings.ProtestOnCampus || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.ProtestOnCampus));
            if (Settings.MissingStudent || !Settings.ini.Exists()) Functions.RegisterCallout(typeof(Callouts.MissingStudent));
            Game.LogTrivial("====================CAMPUSCALLOUTS CALLOUTS REGISTRATION====================");
            
        }
    }
}
