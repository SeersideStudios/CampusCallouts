using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using CalloutInterfaceAPI;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Student Escort", CalloutProbability.Low, "A student has requested an escort across campus.", "Code 1", "ULSAPD")]
    public class StudentEscort : Callout
    {
        //Private References
        private Vector3 CarSpawn;
        private Vector3 PedSpawn;
        private Vector3 Destination;

        private float PedHeading;
        private float CarHeading;

        private Blip PedBlip;
        private Blip DestinationBlip;
        private Ped Ped;
        private Vehicle Car;

        private bool OnScene = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            CarSpawn = new Vector3(-1685.103f, 78.41851f, 63.9855f);
            CarHeading = 112.0373f;

            PedSpawn = new Vector3(-1684.251f, 77.36402f, 64.39139f);
            PedHeading = 111.9856f;

            Destination = new Vector3(-1671.686f, 174.0843f, 61.75573f);

            //Set callout position
            this.CalloutPosition = PedSpawn;

            // LSPDFR
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);

            if (Settings.UseBluelineAudio)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_CIVILIAN_NEEDING_ASSISTANCE_01 IN_OR_ON_POSITION", CalloutPosition);
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_CIVILIAN_NEEDING_ASSISTANCE_01 IN_OR_ON_POSITION", CalloutPosition);
            }

            //Create Callout message
            CalloutMessage = "Student Escort";

            //Last Line
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            //Create Vehicle
            Car = new Vehicle("elegy2", CarSpawn, CarHeading);
            Car.MakePersistent();

            //Create Peds
            Ped = new Ped(PedSpawn, PedHeading);
            Ped.MakePersistent();
            Ped.BlockPermanentEvents = true;
            Ped.Tasks.EnterVehicle(Car, -1);
            Game.LogTrivial("CampusCallouts - StudentEscort - Ped created and sent to vehicle.");

            //Create Blip
            PedBlip = Ped.AttachBlip();
            PedBlip.Color = Color.Blue;

            //Draw Route
            PedBlip.EnableRoute(Color.Orange);

            //Draw Help
            Game.DisplayHelp("A student has requested an escort on campus, please respond and escort the student.");

            //Last Line
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            //First Line
            base.OnCalloutNotAccepted();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (DestinationBlip.Exists()) { DestinationBlip.Delete(); }
            if (Car.Exists()) { Car.Dismiss(); }
        }

        public override void Process()
        {
            //First Line
            base.Process();

            if (!OnScene & Game.LocalPlayer.Character.Position.DistanceTo(Car) <= 15f)
            {
                OnScene = true;
                CalloutInterfaceAPI.Functions.SendMessage(this, "Officer has arrived on scene. Student is requesting an escort.");
                PedBlip.DisableRoute();
                Ped.Tasks.LeaveVehicle(Car, LeaveVehicleFlags.None);
                Ped.Tasks.StandStill(-1);
                Game.DisplayHelp("Press ~y~" + Settings.EndCallout + "~w~ to end the call.");

                //DRAW BLIP
                DestinationBlip = new Blip(Destination);
                DestinationBlip.Color = Color.Blue;

                //Wait 5 Seconds
                GameFiber.Sleep(5000);

                //Instructions
                Game.DisplayNotification("~y~[INFO]~w~ Walk the ped to their destination.");

                //MAKE PED FOLLOW
                Ped.Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, new Vector3(0.25f, 0.25f, 0.25f)); 
            }

            if (OnScene & Game.LocalPlayer.Character.Position.DistanceTo(Destination) <= 5f)
            {
                GameFiber.Sleep(3000);
                Game.DisplayNotification("~y~[INFO]~w~ The student has been escorted safely");
                CalloutInterfaceAPI.Functions.SendMessage(this, "Escort complete. Student safely arrived at their destination.");
                Ped.Dismiss();
                Car.Dismiss();
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_ARE_CODE FOUR");
                this.End();
            }

            if (Game.IsKeyDown(Settings.EndCallout))
            {
                GameFiber.Sleep(3000);
                this.End();
            }
        }

        public override void End()
        {
            //First Line
            base.End();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (DestinationBlip.Exists()) { DestinationBlip.Delete(); }
            if (Car.Exists()) { Car.Dismiss(); }
            Game.LogTrivial("CampusCallouts - StudentEscort - Callout ended and entities cleaned.");
        }
    }
}

