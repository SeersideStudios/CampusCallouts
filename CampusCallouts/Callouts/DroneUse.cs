using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Drawing;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Reports of a Drone", CalloutProbability.Medium, "911 Caller reports of a Drone flying around Campus.", "Code 2", "ULSAPD")]
    public class DroneUse : Callout
    {
        //Private References
        private Ped Ped;

        private Vector3 PedSpawn;
        private float PedHeading;

        private Random rand = new Random();

        private Blip PedBlip;

        private bool OnScene = false;
        private bool GatheredInfo = false;

        private Rage.Object Drone;

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
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_SUSPICIOUS_ACTIVITY_01 IN_OR_ON_POSITION", CalloutPosition);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("RESPOND_CODE_2");

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {


            // Create Peds
            Ped = new Ped(PedSpawn, PedHeading);
            Ped.MakePersistent();
            Ped.BlockPermanentEvents = true;

            //Play Animation
            Ped.Tasks.PlayAnimation("amb@world_human_security_shine_torch@male@idle_b", "idle_e", 1.0f, AnimationFlags.Loop);

            //Spawn Drone
            Drone = new Rage.Object("xs_prop_arena_drone_01", Ped.GetOffsetPositionFront(1.5f));
            Drone.MakePersistent();

            //Start hover Loop
            GameFiber.StartNew(delegate
            {
                float baseZ = Drone.Position.Z;
                bool goingUp = true;

                while (Drone.Exists() && !GatheredInfo)
                {
                    Vector3 currentPos = Drone.Position;

                    // Smoothly move up and down
                    float offset = goingUp ? 0.005f : -0.005f;
                    Drone.Position = new Vector3(currentPos.X, currentPos.Y, currentPos.Z + offset);

                    // Toggle direction if it goes out of range
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
        }

        public override void Process()
        {
            base.Process();
            if (!OnScene & Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 10f)
            {
                //Set On Scene
                OnScene = true;

                //Show info
                Game.DisplayHelp("Press the ~y~END~w~ key to end the call at any time.");
                Game.DisplaySubtitle("~y~[INFO]~w~ Speak to the suspect to gather Info.");
            }

            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 3f)
            {
                int num = rand.Next(0, 2);
                Ped.Tasks.Clear(); // Stops Animation
                Game.LogTrivial("CampusCallouts - Drone Use - Dialogue started, response variation: " + num);


                if (num == 1 && !GatheredInfo)
                {
                    Game.DisplaySubtitle("~b~You: ~w~Hey! Are you the one flying this drone?");
                    GameFiber.Sleep(3500);
                    Game.DisplaySubtitle("~y~Student: ~w~Yeah, what about it? I have permission.");
                    GameFiber.Sleep(3500);
                    Game.DisplaySubtitle("~b~You: ~w~Can I see that permit?");
                    GameFiber.Sleep(3500);
                    Game.DisplaySubtitle("~y~Student: ~w~I don’t have it on me. My professor said it was fine.");
                    GameFiber.Sleep(3500);
                    Game.DisplaySubtitle("~b~You: ~w~Without documented permission, I’ll need you to shut it down.");
                    GameFiber.Sleep(3500);
                    Game.DisplaySubtitle("~y~Student: ~w~This is ridiculous. I won't be listening to you.");
                    GameFiber.Sleep(3500);
                    Game.DisplayNotification("Deal with the suspect as you wish.");
                    GatheredInfo = true;

                }
                else if (num == 0 && !GatheredInfo)
                {
                    Game.DisplaySubtitle("~b~You: ~w~Hey! Are you the one flying this drone?");
                    GameFiber.Sleep(3500);
                    Game.DisplaySubtitle("~y~Student: ~w~Yeah, is everything ok?");
                    GameFiber.Sleep(3500);
                    Game.DisplaySubtitle("~b~You: ~w~You’re not permitted to fly drones on school property.");
                    GameFiber.Sleep(3500);
                    Game.DisplaySubtitle("~y~Student: ~w~I get it. I’ll pack it up right now.");
                    GameFiber.Sleep(3500);

                    GatheredInfo = true;
                    End();
                }
            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Ped) || Game.IsKeyDown(System.Windows.Forms.Keys.End))
            {
                End();
            }
        }

        public override void End()
        {
            base.End();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (Drone.Exists()) { Drone.Delete(); }
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_ARE_CODE FOUR");
            Game.LogTrivial("CampusCallouts - Drone Use - Callout cleaned up.");
        }
    }
}
