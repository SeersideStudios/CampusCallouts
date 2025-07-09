using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Drawing;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Trespasser", CalloutProbability.Medium, "A suspect is reported to be trespassing at the ULSA campus track field.", "Code 2", "ULSAPD")]
    public class Trespasser : Callout
    {
        //Private References
        private Ped Ped;

        private Vector3 PedSpawn;
        private float PedHeading;

        private Random rand = new Random();

        private Blip PedBlip;

        private bool OnScene = false;
        private bool GatheredInfo = false;

        private LHandle Pursuit;

        private int DialogueStep = 0;
        private bool IsInDialogue = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            //Setting Spawn location for Ped
            PedSpawn = new Vector3(-1743.35f, 154.1355f, 64.37103f);
            PedHeading = 215.159f;

            //Setting the Callout location
            this.CalloutPosition = PedSpawn;

            //LSPDFR Handling
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);
            if (Main.CalloutInterface) CalloutInterfaceAPI.Functions.SendMessage(this, "A suspect is said to have been trespassing at the Track field at ULSA.");
            CalloutMessage = "Trespasser";
            CalloutAdvisory = "A suspect is said to have been trespassing at the Track field at ULSA.";

            if (Settings.UseBluelineAudio)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_CIVILIAN_NEEDING_ASSISTANCE_01 IN_OR_ON_POSITION", CalloutPosition);
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_CITIZENS_REPORT CC_CRIME_TRESPASSING_01 IN_OR_ON_POSITION", CalloutPosition);
            }

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_02_02");

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            

            // Create Peds
            Ped = new Ped(PedSpawn, PedHeading);
            Ped.MakePersistent();
            Ped.BlockPermanentEvents = true;
            Ped.Face(Game.LocalPlayer.Character);

            //Log
            Game.LogTrivial("CampusCallouts - Trespasser - Ped Created");

            // Create Ped Blip
            PedBlip = Ped.AttachBlip();
            PedBlip.Color = Color.Red;

            //Create Route
            PedBlip.EnableRoute(Color.Red);

            //Draw Help
            Game.DisplayHelp("Security at the University has reported a Trespasser at the Track on campus. Please investigate.");

            //Callout Interface
            if (Main.CalloutInterface) CalloutInterfaceAPI.Functions.SendMessage(this, "Trespasser reported at the ULSA Campus");

            //Make ped go to their destination
            Ped.Tasks.Wander();

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
        }

        public override void Process()
        {
            base.Process();

            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 10f)
            {
                OnScene = true;
                PedBlip.DisableRoute();
                Ped.Tasks.Clear();
                Ped.Tasks.StandStill(-1);
                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to advance dialogue. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
                Game.DisplaySubtitle("~y~[INFO]~w~ Speak to the trespasser.");
            }

            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 3f)
            {
                if (!IsInDialogue)
                {
                    Ped.Face(Game.LocalPlayer.Character);
                    IsInDialogue = true;
                    DialogueStep = 0;
                }

                if (Game.IsKeyDown(Settings.DialogueKey))
                {
                    switch (DialogueStep)
                    {
                        case 0:
                            Game.DisplaySubtitle("~b~You: ~w~Hey, what are you doing here? This area is off-limits.");
                            break;
                        case 1:
                            if (rand.Next(0, 2) == 1)
                            {
                                Game.DisplaySubtitle("~y~Trespasser: ~w~I'm just testing out the track! Leave me alone!");
                                Ped.Tasks.ReactAndFlee(Game.LocalPlayer.Character);
                                Pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                                LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(Pursuit, Ped);
                                LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                                LSPD_First_Response.Mod.API.Functions.SetPursuitCopsCanJoin(Pursuit, true);
                                Game.DisplayNotification("The suspect is running away!");
                                if (Main.CalloutInterface) CalloutInterfaceAPI.Functions.SendMessage(this, "Trespasser is fleeing on foot.");
                                GatheredInfo = true;
                                IsInDialogue = false;
                            }
                            else
                            {
                                Game.DisplaySubtitle("~y~Trespasser: ~w~I'm sorry, I didn't realize. I was just going for a run.");
                            }
                            break;
                        case 2:
                            Game.DisplaySubtitle("~b~You: ~w~You need to leave immediately. I’ll have to file a report.");
                            break;
                        case 3:
                            Game.DisplaySubtitle("~y~Trespasser: ~w~Understood, I’ll head out now.");
                            if (Main.CalloutInterface) CalloutInterfaceAPI.Functions.SendMessage(this, "Trespasser was cooperative and is leaving.");
                            GatheredInfo = true;
                            IsInDialogue = false;
                            End();
                            break;
                    }

                    DialogueStep++;
                    GameFiber.Wait(200); // debounce
                }
            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Ped) || Game.IsKeyDown(Settings.EndCallout) || Ped.IsDead)
            {
                End();
            }
        }


        public override void End()
        {
            base.End();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_ARE_CODE FOUR");
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            Game.LogTrivial("CampusCallouts - Trespassing - Callout cleaned up.");
        }
    }
}
