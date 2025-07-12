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
        // --- Private Fields ---
        private Ped Ped;
        private Blip PedBlip;

        private Vector3 PedSpawn;
        private float PedHeading;

        private Random rand = new Random();

        private bool OnScene = false;
        private bool GatheredInfo = false;

        private LHandle Pursuit;
        private int DialogueStep = 0;
        private bool IsInDialogue = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            // Set spawn point and heading
            PedSpawn = new Vector3(-1743.35f, 154.1355f, 64.37103f);
            PedHeading = 215.159f;
            CalloutPosition = PedSpawn;

            // Show callout area and add distance checks
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);

            // Interface and advisory messages
            CalloutMessage = "Trespasser";
            CalloutAdvisory = "A suspect is said to have been trespassing at the Track field at ULSA.";
            if (Main.CalloutInterface) CalloutInterfaceAPI.Functions.SendMessage(this, CalloutAdvisory);

            // Play dispatch audio
            if (Settings.UseBluelineAudio)
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_TRESPASSING_01", CalloutPosition);
            else
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_CITIZENS_REPORT CC_CRIME_TRESPASSING_01 IN_OR_ON_POSITION", CalloutPosition);

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_02_02");

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Spawn suspect
            Ped = new Ped(PedSpawn, PedHeading);
            if (Ped.Exists())
            {
                Ped.MakePersistent();
                Ped.BlockPermanentEvents = true;
                Ped.Face(Game.LocalPlayer.Character);
                Ped.Tasks.Wander();

                // Blip for location guidance
                PedBlip = Ped.AttachBlip();
                PedBlip.Color = Color.Red;
                PedBlip.EnableRoute(Color.Red);
            }

            Game.DisplayHelp("Security at the University has reported a Trespasser at the Track on campus. Please investigate.");

            if (Main.CalloutInterface) CalloutInterfaceAPI.Functions.SendMessage(this, "Trespasser reported at the ULSA Campus");
            Game.LogTrivial("CampusCallouts - Trespasser - Ped created and wandering.");
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (Ped.Exists()) Ped.Dismiss();
            if (PedBlip.Exists()) PedBlip.Delete();
        }

        public override void Process()
        {
            base.Process();

            // When player arrives near suspect
            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 10f)
            {
                OnScene = true;

                if (PedBlip.Exists()) PedBlip.DisableRoute();
                if (Ped.Exists()) Ped.Tasks.StandStill(-1);

                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to advance dialogue. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
                Game.DisplaySubtitle("~y~[INFO]~w~ Speak to the trespasser.");
            }

            // Begin dialogue sequence
            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 3f)
            {
                if (!IsInDialogue)
                {
                    if (Ped.Exists()) Ped.Face(Game.LocalPlayer.Character);
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
                            // 50/50 chance of fleeing vs staying
                            if (rand.Next(0, 2) == 1)
                            {
                                Game.DisplaySubtitle("~y~Trespasser: ~w~I'm just testing out the track! Leave me alone!");
                                if (Ped.Exists()) Ped.Tasks.ReactAndFlee(Game.LocalPlayer.Character);

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
                            End(); // Clean exit for cooperative variant
                            break;
                    }

                    DialogueStep++;
                    GameFiber.StartNew(() => GameFiber.Sleep(200)); // debounce
                }
            }

            // Callout ends on arrest, death, or manual end
            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Ped) || Ped.IsDead || Game.IsKeyDown(Settings.EndCallout))
            {
                End();
            }
        }

        public override void End()
        {
            base.End();

            if (Ped.Exists()) Ped.Dismiss();
            if (PedBlip.Exists()) PedBlip.Delete();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - Trespasser - Callout cleaned up.");
        }
    }
}
