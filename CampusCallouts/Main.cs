using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Net;
using System.Threading;
using System.Linq;

[assembly: Rage.Attributes.Plugin("CampusCallouts", Description = "University Callouts ReMake for LSPDFR 0.4.9", Author = "Seerside Studios")] //Is this really needed?
namespace CampusCallouts
{
    public class Main : Plugin
    {
        public static Version ClientVersion = new Version();
        public static Version curVersion = new Version("1.0.0");

        public static bool UpToDate;
        public static bool CalloutInterface;
        public static bool Beta = false;

        public override void Initialize()
        {
            try
            {
                Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
                Game.LogTrivial("CampusCallouts: Campus Callouts version " + curVersion + " has been loaded.");
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
                    int num = (int)Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Campus Callouts", "~y~v." + curVersion + " ~b~by Seerside Studios", " ~g~Loaded Successfully. ~b~Have a good day at School!");
                    GameFiber.StartNew(delegate
                    {
                        Game.LogTrivial("CampusCallouts: Player Went on Duty. Checking for Updates.");
                        try
                        {
                            Thread FetchVersionThread = new Thread(() =>
                            {
                                using (WebClient client = new WebClient())
                                {
                                    try
                                    {
                                        string s = client.DownloadString("https://raw.githubusercontent.com/SeersideStudios/CampusCallouts/refs/heads/master/version.txt");

                                        ClientVersion = new Version(s);
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
                                if (curVersion.CompareTo(ClientVersion) < 0)
                                {
                                    Game.LogTrivial("CampusCallouts: Finished Checking Campus Callouts for Updates.");
                                    Game.LogTrivial("CampusCallouts: Update Available for Campus Callouts. Installed Version " + curVersion + " ,New Version " + ClientVersion);
                                    Game.DisplayNotification("~g~Update Available~w~ for ~b~CampusCallouts! It is ~y~Strongly Recommended~w~ to~g~ Update~b~ CampusCallouts. ~w~Playing on an Old Version ~r~May Cause Issues.");
                                    Game.LogTrivial("====================CAMPUSCALLOUTS WARNING====================");
                                    Game.LogTrivial("Outdated CampusCallouts Version. Please update as soon as possible for the best compatibility!");
                                    Game.LogTrivial("====================CAMPUSCALLOUTS WARNING====================");
                                    UpToDate = false;
                                }
                                else if (curVersion.CompareTo(ClientVersion) > 0)
                                {
                                    Game.LogTrivial("CampusCallouts: DETECTED BETA RELEASE. DO NOT REDISTRIBUTE. PLEASE REPORT ALL ISSUES.");
                                    Game.DisplayNotification("CampusCallouts: ~r~DETECTED BETA RELEASE. ~w~DO NOT REDISTRIBUTE. PLEASE REPORT ALL ISSUES.");
                                    UpToDate = true;
                                    Beta = true;
                                }
                                else
                                {
                                    Game.LogTrivial("CampusCallouts: Finished Checking Campus Callouts for Updates.");
                                    Game.DisplayNotification("You are on the ~g~Latest Version~w~ of ~b~CampusCallouts.");
                                    Game.LogTrivial("CampusCallouts: Campus Callouts is Up to Date.");
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
            if (Settings.ini.Exists()) { Game.LogTrivial("CampusCallout.ini is installed."); }
            else { Game.LogTrivial("CampusCallouts.ini is NOT installed"); }

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
            Game.LogTrivial("====================CAMPUSCALLOUTS CALLOUTS REGISTRATION====================");
            
        }
    }
}