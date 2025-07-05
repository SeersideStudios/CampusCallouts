using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using CalloutInterfaceAPI;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Stalking Report", CalloutProbability.Low, "Student reported being followed by an unknown individual on campus.", "Code 2", "ULSAPD")]

    public class StalkingReport : Callout
    {
        //Private References
        private Vector3 PedSpawn = new Vector3(-1819.464f, 140.2641f, 77.12f);
        private Vector3 Destination = new Vector3(-1551.276f, 210.0268f, 58.8561f);

        private float PedHeading = 80.38177f;

        private Blip PedBlip;

        private Ped Ped;
        private Ped Stalker;

        private bool GatheredInfo = false;
        private bool OnScene = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            //Set callout position
            this.CalloutPosition = PedSpawn;

            // LSPDFR
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_SUSPICIOUS_ACTIVITY_01 IN_OR_ON_POSITION", CalloutPosition);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("RESPOND_CODE_2");

            //Create Callout message
            CalloutMessage = "Reports of a Stalking";

            //Last Line
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            //Create Peds
            Ped = new Ped(PedSpawn, PedHeading);
            Ped.MakePersistent();
            Ped.BlockPermanentEvents = true;

            Stalker = new Ped(PedSpawn, PedHeading);
            Stalker.MakePersistent();
            Stalker.BlockPermanentEvents = true;

            //Log
            Game.LogTrivial("Peds created");
            
            //Create Blips
            PedBlip = Ped.AttachBlip();
            PedBlip.Sprite = BlipSprite.Friend;
            PedBlip.Color = Color.Blue;

            //Draw Route
            PedBlip.EnableRoute(Color.Red);

            //Draw Help
            Game.DisplayHelp("A student has reported that they are being stalked. Please investigate.");

            //Make ped go to their destination
            Ped.Tasks.Wander();

            //Make other ped follow
            Stalker.Tasks.FollowToOffsetFromEntity(Ped, new Vector3(1.5f, 1.5f, 1.5f));

            //Last Line
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            //First Line
            base.OnCalloutNotAccepted();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (Stalker.Exists()) { Stalker.Dismiss(); }
        }

        public override void Process()
        {
            //First Line
            base.Process();

            if (!OnScene & Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 10f)
            {
                //Set On Scene
                OnScene = true;

                //Give Ped Task
                PedBlip.DisableRoute();
                Ped.Tasks.StandStill(-1);
                Ped.Face(Game.LocalPlayer.Character);

                //Show info
                Game.DisplayHelp("Press the ~y~END~w~ key to end the call at any time.");
                Game.DisplayNotification("~y~[INFO]~w~ Speak to the student and gather information");

                //Make stalker leave
                Stalker.Tasks.ReactAndFlee(Game.LocalPlayer.Character);
            }

            if (!GatheredInfo & OnScene & Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 3f)
            {
                //Get stalker details
                var gender = LSPD_First_Response.Mod.API.Functions.GetPersonaForPed(Stalker).Gender;
                var age = LSPD_First_Response.Mod.API.Functions.GetPersonaForPed(Stalker).ModelAge;

                //Conversation
                Game.DisplaySubtitle("~b~Student: ~w~Thank you for coming officer!");
                GameFiber.Sleep(3500);
                Game.DisplaySubtitle("~g~You: ~w~Of course! What's going on?");
                GameFiber.Sleep(3500);
                Game.DisplaySubtitle("~b~Student: ~w~I believe someone is following me.");
                GameFiber.Sleep(3500);
                Game.DisplaySubtitle("~g~You: ~w~Can you describe the person?");
                GameFiber.Sleep(3500);
                Game.DisplaySubtitle("~b~Student: ~w~Yes, it is a ~y~" + gender + " ~w~who is ~y~" + age);
                GameFiber.Sleep(5000);
                Game.DisplaySubtitle("~g~You: ~w~Okay! I will see if I can locate them!");
                GameFiber.Sleep(3500);

                //Create Pursuit
                CalloutInterfaceAPI.Functions.SendMessage(this, "Victim provided a suspect description:\nGender: " + gender + "\nEstimated Age: " + age + "\nSuspect has fled.");
                LHandle Pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(Pursuit, Stalker);
                LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                LSPD_First_Response.Mod.API.Functions.SetPursuitCopsCanJoin(Pursuit, true);

                GatheredInfo = true;
            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Stalker))
            {
                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspect apprehended. Code 4.");
                this.End();
            }

            if (Stalker.IsDead)
            {
                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspect is down. Code 4.");
                if (Stalker.Exists()) { Stalker.Dismiss(); }
                this.End();
            }

            if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
            {
                if (Stalker.Exists()) { Stalker.Dismiss(); }
                this.End();
            }
        }

        public override void End()
        {
            //First Line
            base.End();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_ARE_CODE FOUR");
            Game.LogTrivial("CampusCallouts - StalkingReport - Callout cleaned up.");
        }
    }
}

