using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Drawing;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Vandalism", CalloutProbability.Medium, "911 Caller reports of someone vandalizing university property", "Code 2", "ULSAPD")]
    public class Vandalism : Callout
    {
        private Ped Suspect;
        private Blip SuspectBlip;

        private Vector3 SuspectSpawn;
        private float SuspectHeading;

        private bool OnScene = false;
        private bool GatheredInfo = false;

        private Random rand = new Random();

        public override bool OnBeforeCalloutDisplayed()
        {
            // Set spawn location
            SuspectSpawn = new Vector3(-1611.607f, 182.3768f, 59.72588f);
            SuspectHeading = 223.0731f;

            CalloutPosition = SuspectSpawn;

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);
            CalloutMessage = "Vandalism in Progress";
            CalloutAdvisory = "911 Caller reports of someone vandalizing university property.";
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_PROPERTY_DAMAGE_01 IN_OR_ON_POSITION", CalloutPosition);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("RESPOND_CODE_2");

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Spawn suspect
            Suspect = new Ped(SuspectSpawn, SuspectHeading);
            Suspect.MakePersistent();
            Suspect.BlockPermanentEvents = true;

            // Gives Suspect Bat 
            Suspect.Inventory.GiveNewWeapon("WEAPON_BAT", -1, true);

            // Suspect swings bat
            Suspect.Tasks.PlayAnimation("melee@large_wpn@streamed_core", "plyr_rear_takedown_bat_r_facehit", 1.0f, AnimationFlags.Loop);

            // Create blip
            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.Color = Color.Red;
            SuspectBlip.EnableRoute(Color.Red);

            Game.DisplayHelp("A student appears to be smashing school property with a bat. Approach and investigate.");
            Game.LogTrivial("CampusCallouts - Vandalism - Suspect spawned and swinging bat.");

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (Suspect.Exists()) Suspect.Dismiss();
            if (SuspectBlip.Exists()) SuspectBlip.Delete();
        }

        public override void Process()
        {
            base.Process();

            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Suspect) <= 10f)
            {
                OnScene = true;
                Game.DisplayHelp("Press the ~y~END~w~ key to end the call at any time.");
                Game.DisplaySubtitle("~y~[INFO]~w~ Speak to the suspect to gather information.");
            }

            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Suspect) <= 3f)
            {
                Suspect.Tasks.Clear();
                Suspect.Face(Game.LocalPlayer.Character);
                int num = rand.Next(0, 2);

                Game.LogTrivial($"CampusCallouts - Vandalism - Dialogue variation: {num}");

                if (num == 1)
                {
                    Game.DisplaySubtitle("~b~You: ~w~Put the bat down! Step away from the flagpole!");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~r~Suspect: ~w~They rejected my transfer again! This school’s a joke.");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~b~You: ~w~Smashing things won’t fix that. Drop the weapon, now!");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~r~Suspect: ~w~No! I’ve had it! You’re not stopping me.");
                    GameFiber.Sleep(3500);

                    Game.DisplayNotification("The suspect is becoming aggressive!");
                    Suspect.BlockPermanentEvents = false;
                    Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);
                }

                else
                {
                    Game.DisplaySubtitle("~b~You: ~w~Put the bat down. What the hell are you doing?");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~r~Suspect: ~w~I... I’m sorry. I just lost it.");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~b~You: ~w~You're damaging school property. That’s a serious offense.");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~r~Suspect: ~w~I know. I just got an email saying I failed my final... I snapped.");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~b~You: ~w~That doesn’t excuse this behavior. You’re lucky someone didn’t get hurt.");
                    GameFiber.Sleep(3500);

                    Game.DisplayNotification("The suspect appears remorseful and is complying. Deal with him as you deem fit");
                }


                GatheredInfo = true;
            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Suspect) || Game.IsKeyDown(System.Windows.Forms.Keys.End))
            {
                End();
            }
        }

        public override void End()
        {
            base.End();
            if (Suspect.Exists()) Suspect.Dismiss();
            if (SuspectBlip.Exists()) SuspectBlip.Delete();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_ARE_CODE FOUR");
            Game.LogTrivial("CampusCallouts - Vandalism - Callout cleaned up.");
        }
    }
}
