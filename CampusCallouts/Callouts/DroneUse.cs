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


                //Give Ped Task
                PedBlip.DisableRoute();
                Ped.Tasks.Clear();
                Ped.Tasks.StandStill(-1);

                //Show info
                Game.DisplayHelp("Press the ~y~END~w~ key to end the call at any time.");
                Game.DisplaySubtitle("~y~[INFO]~w~ Speak to the suspect to gather Info.");
            }

            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 3f)
            {
                int num = rand.Next(0, 2);

                if (num == 1 && !GatheredInfo)
                {
                 
                }
                else if (num == 0 && !GatheredInfo)
                {

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
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_ARE_CODE FOUR");
            Game.LogTrivial("CampusCallouts - Trespassing - Callout cleaned up.");
        }
    }
}
