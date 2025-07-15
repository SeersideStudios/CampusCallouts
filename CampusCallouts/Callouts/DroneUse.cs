using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Drawing;
using System.IO;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Reports of a Drone", CalloutProbability.Medium, "911 Caller reports of a Drone flying around Campus.", "Code 2", "ULSAPD")]
    public class DroneUse : Callout
    {
        //Private References
        private Ped Ped;

        private Vector3 PedSpawn;
        private float PedHeading;

        private readonly Random rand = new Random();

        private Blip PedBlip;

        private bool OnScene = false;
        private bool GatheredInfo = false;

        private Rage.Object Drone;

        private int dialogueStage = 0;
        private bool dialogueStarted = false;
        private int selectedDialogue = -1; // 0 or 1

        public override bool OnBeforeCalloutDisplayed()
        {
            //Setting Spawn location for Ped
            PedSpawn = new Vector3(-1672.595f, 213.435f, 62.12666f);
            PedHeading = 278.3569f;

            //Setting the Callout location
            this.CalloutPosition = PedSpawn;

            //LSPDFR Handling
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);
            CalloutMessage = "Reports of a Drone";
            CalloutAdvisory = "911 Caller reports of a Drone flying around Campus.";

            if (Settings.UseBluelineAudio)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_DISTURBANCE_04", CalloutPosition);
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_CRIME_CIVIL_DISTURBANCE IN_OR_ON_POSITION", CalloutPosition); 
            }


                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_02_02");

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {


            // Create Peds
            Ped = new Ped(PedSpawn, PedHeading);
            Ped.IsPersistent = true; // Make the ped persistent so it doesn't despawn
            Ped.BlockPermanentEvents = true;

            //Play Animation
            Ped.Tasks.PlayAnimation("amb@world_human_security_shine_torch@male@idle_b", "idle_e", 1.0f, AnimationFlags.Loop);

            //Spawn Drone
            Drone = new Rage.Object("xs_prop_arena_drone_01", Ped.GetOffsetPositionFront(1.5f));
            Drone.IsPersistent = true; // Make the drone persistent so it doesn't despawn

            //Start hover Loop
            GameFiber.StartNew(delegate
            {
                GameFiber.Sleep(1000); // Let the object fully spawn and settle

                if (!Drone.Exists()) return;

                float baseZ = Drone.Position.Z;
                bool goingUp = true;

                while (Drone.Exists() && !GatheredInfo)
                {
                    Vector3 currentPos = Drone.Position;

                    float offset = goingUp ? 0.005f : -0.005f;
                    Drone.Position = new Vector3(currentPos.X, currentPos.Y, currentPos.Z + offset);

                    if (Drone.Position.Z >= baseZ + 0.1f) goingUp = false;
                    if (Drone.Position.Z <= baseZ - 0.1f) goingUp = true;

                    GameFiber.Sleep(15); // Controls speed/smoothness
                }
            });

            //Log
            Game.LogTrivial("CampusCallouts - Drone Use - Ped Created");

            // Create Ped Blip
            PedBlip = Ped.AttachBlip();
            PedBlip.Color = Color.Blue;

            //Create Route
            PedBlip.EnableRoute(Color.Blue);

            //Draw Help
            Game.DisplayHelp("Security at the University has reported someone using a drone on campus. Please investigate.");


            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (Drone.Exists()) { Drone.Delete(); }
        }

        public override void Process()
        {
            base.Process();

            if (!OnScene && Ped && Ped.Exists() && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 10f)
            {
                OnScene = true;
                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to advance dialogue. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
                Game.DisplaySubtitle("~y~[INFO]~w~ Speak to the suspect to gather info.");
            }

            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 3f && !dialogueStarted)
            {
                Ped.Tasks.Clear();
                NativeFunction.Natives.TASK_LOOK_AT_ENTITY(Ped, Game.LocalPlayer.Character, -1); // Make the ped look at the player
                selectedDialogue = rand.Next(0, 2);
                dialogueStarted = true;

                Game.DisplaySubtitle("Press ~y~" + Settings.DialogueKey + "~w~ to begin conversation.");
            }

            if (dialogueStarted && Game.IsKeyDown(Settings.DialogueKey))
            {
                HandleDialogue();
                GameFiber.StartNew(() => GameFiber.Sleep(250)); // Prevent multiple triggers from holding the key
            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Ped) || Game.IsKeyDown(Settings.EndCallout) || Ped.IsDead)
            {
                End();
            }
        }

        private void HandleDialogue()
        {
            if (selectedDialogue == 0)
            {
                switch (dialogueStage)
                {
                    case 0:
                        Game.DisplaySubtitle("~b~You: ~w~Hey! Are you the one flying this drone?");
                        break;
                    case 1:
                        Game.DisplaySubtitle("~y~Student: ~w~Yeah, is everything ok?");
                        break;
                    case 2:
                        Game.DisplaySubtitle("~b~You: ~w~You’re not permitted to fly drones on school property.");
                        break;
                    case 3:
                        Game.DisplaySubtitle("~y~Student: ~w~I get it. I’ll pack it up right now.");
                        break;
                    case 4:
                        Game.DisplayNotification("The student complies. You may end the call.");
                        GatheredInfo = true;
                        dialogueStarted = false;
                        End();
                        return;
                }
            }
            else if (selectedDialogue == 1)
            {
                switch (dialogueStage)
                {
                    case 0:
                        Game.DisplaySubtitle("~b~You: ~w~Hey! Are you the one flying this drone?");
                        break;
                    case 1:
                        Game.DisplaySubtitle("~y~Student: ~w~Yeah, what about it? I have permission.");
                        break;
                    case 2:
                        Game.DisplaySubtitle("~b~You: ~w~Can I see that permit?");
                        break;
                    case 3:
                        Game.DisplaySubtitle("~y~Student: ~w~I don’t have it on me. My professor said it was fine.");
                        break;
                    case 4:
                        Game.DisplaySubtitle("~b~You: ~w~Without documented permission, I’ll need you to shut it down.");
                        break;
                    case 5:
                        Game.DisplaySubtitle("~y~Student: ~w~This is ridiculous. I won't be listening to you.");
                        break;
                    case 6:
                        Game.DisplayNotification("Deal with the suspect as you wish.");
                        GatheredInfo = true;
                        dialogueStarted = false;
                        return;
                }
            }

            dialogueStage++;
        }


        public override void End()
        {
            base.End();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (Drone.Exists()) { Drone.Delete(); }
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - Drone Use - Callout cleaned up.");
        }
    }
}
