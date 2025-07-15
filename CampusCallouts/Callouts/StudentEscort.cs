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
        private Vector3 CarSpawn = new Vector3(-1685.103f, 78.41851f, 63.9855f);
        private Vector3 PedSpawn = new Vector3(-1684.251f, 77.36402f, 64.39139f);
        private Vector3 Destination = new Vector3(-1671.686f, 174.0843f, 61.75573f);
        private float CarHeading = 112.0373f;
        private float PedHeading = 111.9856f;

        private Ped Ped;
        private Vehicle Car;
        private Blip PedBlip;
        private Blip DestinationBlip;

        private bool OnScene = false;
        private bool EscortStarted = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutPosition = PedSpawn;

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);

            CalloutMessage = "Student Escort";
            CalloutAdvisory = "Student has requested to be escorted across campus. Meet them at the parking lot.";

            if (Settings.UseBluelineAudio)
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_CIVILIAN_NEEDING_ASSISTANCE_01", CalloutPosition);
            else
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_CIVILIAN_NEEDING_ASSISTANCE_01 IN_OR_ON_POSITION", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Car = new Vehicle("elegy2", CarSpawn, CarHeading);
            if (Car.Exists()) Car.MakePersistent();

            Ped = new Ped(PedSpawn, PedHeading);
            if (Ped.Exists())
            {
                Ped.MakePersistent();
                Ped.BlockPermanentEvents = true;
                Ped.Tasks.EnterVehicle(Car, -1);
                PedBlip = Ped.AttachBlip();
                PedBlip.Color = Color.Orange;
                PedBlip.EnableRoute(Color.Orange);
            }

            Game.DisplayHelp("Head to the student and prepare to escort them across campus.");
            Game.LogTrivial("CampusCallouts - StudentEscort - Ped and Car spawned.");
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Car) <= 15f)
            {
                OnScene = true;
                CalloutInterfaceAPI.Functions.SendMessage(this, "Officer arrived on scene. The student is waiting.");
                if (PedBlip.Exists()) PedBlip.DisableRoute();

                if (Ped.Exists())
                {
                    Ped.Tasks.LeaveVehicle(Car, LeaveVehicleFlags.None);
                    Ped.Tasks.StandStill(-1);
                    Ped.Face(Game.LocalPlayer.Character);
                }

                GameFiber.StartNew(() =>
                {
                    GameFiber.Sleep(5000);

                    if (Ped.Exists())
                    {
                        Game.DisplayNotification("~y~[INFO]~w~ Walk the student safely to their destination.");
                        DestinationBlip = new Blip(Destination)
                        {
                            Color = Color.Blue
                        };

                        Ped.Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, new Vector3(0.25f, 0.25f, 0));
                        EscortStarted = true;
                    }
                });
            }

            if (EscortStarted && Game.LocalPlayer.Character.Position.DistanceTo(Destination) <= 5f)
            {

                    if (Ped.Exists()) Ped.Tasks.StandStill(-1);
                    CalloutInterfaceAPI.Functions.SendMessage(this, "Escort complete. Student arrived safely.");
                    Game.DisplaySubtitle("~b~Student: ~w~Thank you!");
                    Game.DisplayNotification("~y~[INFO]~w~ The student has arrived safely.");
                    End();
            }

            if (Game.IsKeyDown(Settings.EndCallout))
            {
                    End();
            }
        }

        public override void End()
        {
            base.End();

            if (Ped.Exists()) Ped.Dismiss();
            if (Car.Exists()) Car.Dismiss();
            if (PedBlip.Exists()) PedBlip.Delete();
            if (DestinationBlip.Exists()) DestinationBlip.Delete();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - StudentEscort - Callout cleaned up.");
        }
    }
}
