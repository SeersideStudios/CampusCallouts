using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Drawing;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Trespasser", CalloutProbability.Medium, "A trespasser has been reported on the track field at the ULSA Campus. Investigate the situation.", "Code 2", "ULSAPD")]
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
        private bool pursuitCreated = false;

        private LHandle Pursuit;

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
            FriendlyName = "trespasser";
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_TRESPASSING_01 IN_OR_ON_POSITION", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            

            // Create Peds
            Ped = new Ped(PedSpawn, PedHeading);
            Ped.MakePersistent();
            Ped.BlockPermanentEvents = true;

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
                Game.DisplaySubtitle("~y~[INFO]~w~ Speak to the Trespasser to gather Info.");
            }

            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 3f)
            {
                int num = rand.Next(0, 2);

                if (num == 1 && !pursuitCreated && !GatheredInfo)
                {
                    //Make Ped leave
                    Ped.Tasks.ReactAndFlee(Game.LocalPlayer.Character);

                    Game.DisplaySubtitle("~y~Trespasser: ~w~I'm just testing out the track! Leave me alone!");
                    GameFiber.Sleep(3500);

                    //Create Pursuit
                    Pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                    LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(Pursuit, Ped);
                    LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                    LSPD_First_Response.Mod.API.Functions.SetPursuitCopsCanJoin(Pursuit, true);
                    Game.DisplayNotification("The suspect is running away!");
                    if (Main.CalloutInterface) CalloutInterfaceAPI.Functions.SendMessage(this, "Trespasser reported running away");
                    pursuitCreated = true;
                    GatheredInfo = true;
                }
                else if (num == 0 && !GatheredInfo)
                {
                    Game.DisplaySubtitle("~b~You: ~w~Hey, what are you doing here? This area is off-limits.");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~y~Trespasser: ~w~I'm sorry, I didn't realize. I was just going for a run.");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~b~You: ~w~You need to leave immediately. I’ll have to file a report.");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~y~Trespasser: ~w~Understood, I’ll head out now.");
                    GameFiber.Sleep(3500);

                    if (Main.CalloutInterface) CalloutInterfaceAPI.Functions.SendMessage(this, "Trespasser handled, and dismissed.");

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
