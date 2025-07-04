using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Drawing;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Vandalism", CalloutProbability.Medium, "911 Caller reports of someone vandalising at the university", "Code 2", "ULSAPD")]
    public class Vandalism : Callout
    {
        private Ped Suspect;
        private Blip SuspectBlip;
        private Rage.Object SprayCan;
        private Rage.Object Artwork;

        private Vector3 SuspectSpawn;
        private float SuspectHeading;

        private bool OnScene = false;
        private bool GatheredInfo = false;

        private Random rand = new Random();

        public override bool OnBeforeCalloutDisplayed()
        {
            // Set spawn
            SuspectSpawn = new Vector3(1,1,1); // Set your preferred coordinates
            SuspectHeading = 1f;

            CalloutPosition = SuspectSpawn;

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);
            CalloutMessage = "Vandalism in Progress";
            CalloutAdvisory = "911 Caller reports of someone vandalising at the university.";
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

            // Spawn spray can and artwork
            SprayCan = new Rage.Object("prop_cs_spray_can", Suspect.GetOffsetPositionFront(0.3f));
            SprayCan.MakePersistent();

            float groundZ = (float)(World.GetGroundZ(Suspect.Position, true, true) ?? Suspect.Position.Z);
            Vector3 artPos = new Vector3(Suspect.Position.X + 1.0f, Suspect.Position.Y, groundZ);

            Artwork = new Rage.Object("sf_int2_art_gf_option_2", artPos);
            Artwork.Rotation = new Rotator(90f, 0f, Suspect.Heading + 180f); // makes it flat
            Artwork.MakePersistent();
            Artwork.IsPositionFrozen = true;


            // Make suspect play spray animation
            Suspect.Tasks.PlayAnimation("amb@world_human_security_shine_torch@male@idle_b", "idle_e", 1.0f, AnimationFlags.Loop);

            // Create blip
            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.Color = Color.Red;
            SuspectBlip.EnableRoute(Color.Red);

            Game.DisplayHelp("A student appears to be spray painting school property. Approach and investigate.");
            Game.LogTrivial("CampusCallouts - Vandalism - Suspect created and objects spawned.");

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (Suspect.Exists()) Suspect.Dismiss();
            if (SuspectBlip.Exists()) SuspectBlip.Delete();
            if (SprayCan.Exists()) SprayCan.Delete();
            if (Artwork.Exists()) Artwork.Delete();
        }

        public override void Process()
        {
            base.Process();

            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Suspect) <= 10f)
            {
                OnScene = true;
                Game.DisplayHelp("Press the ~y~END~w~ key to end the call at any time.");
                Game.DisplaySubtitle("~y~[INFO]~w~ Speak to the suspect to gather info.");
            }

            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Suspect) <= 3f)
            {
                Suspect.Tasks.Clear();
                int num = rand.Next(0, 2);

                Game.LogTrivial("CampusCallouts - Vandalism - Dialogue variation: {num}");

                if (num == 1)
                {
                    Game.DisplaySubtitle("~b~You: ~w~Are you defacing school property?");
                    GameFiber.Sleep(3000);
                    Game.DisplaySubtitle("~r~Suspect: ~w~So what? It's art. You pigs don't get it.");
                    GameFiber.Sleep(3000);
                    Game.DisplayNotification("Deal with the suspect as you see fit.");
                }
                else
                {
                    Game.DisplaySubtitle("~b~You: ~w~Excuse me! Stop that right now.");
                    GameFiber.Sleep(3000);
                    Game.DisplaySubtitle("~r~Suspect: ~w~Sorry! I didn’t realize this was illegal here.");
                    GameFiber.Sleep(3000);
                    Game.DisplaySubtitle("~b~You: ~w~Step away from the wall.");
                    GameFiber.Sleep(3000);
                    Game.DisplayNotification("The suspect complied. You may issue a citation or a warning.");
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
            if (SprayCan.Exists()) SprayCan.Delete();
            if (Artwork.Exists()) Artwork.Delete();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_ARE_CODE FOUR");
            Game.LogTrivial("CampusCallouts - Vandalism - Callout cleaned up.");
        }
    }
}
