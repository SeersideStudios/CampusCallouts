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

        private int DialogueStep = 0;
        private bool IsInDialogue = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            //Set callout position
            this.CalloutPosition = PedSpawn;

            // LSPDFR
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);

            if (Settings.UseBluelineAudio)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("SUSPICIOUS_PERSON_02", CalloutPosition);
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_CIVILIAN_NEEDING_ASSISTANCE_01 IN_OR_ON_POSITION", CalloutPosition);
            }

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_02_02");

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
            base.Process();

            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 10f)
            {
                OnScene = true;

                PedBlip.DisableRoute();
                Ped.Tasks.StandStill(-1);
                Ped.Face(Game.LocalPlayer.Character);
                Stalker.Tasks.ReactAndFlee(Game.LocalPlayer.Character);

                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to advance dialogue. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
                Game.DisplayNotification("~y~[INFO]~w~ Speak to the student and gather information.");
            }

            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 3f)
            {
                if (!IsInDialogue)
                {
                    Ped.Tasks.Clear();
                    Ped.Face(Game.LocalPlayer.Character);
                    IsInDialogue = true;
                    DialogueStep = 0;
                }

                if (Game.IsKeyDown(Settings.DialogueKey))
                {
                    var gender = LSPD_First_Response.Mod.API.Functions.GetPersonaForPed(Stalker).Gender.ToString();
                    var age = LSPD_First_Response.Mod.API.Functions.GetPersonaForPed(Stalker).ModelAge;

                    switch (DialogueStep)
                    {
                        case 0:
                            Game.DisplaySubtitle("~b~Student: ~w~Thank you for coming, officer!");
                            break;
                        case 1:
                            Game.DisplaySubtitle("~g~You: ~w~Of course. What's going on?");
                            break;
                        case 2:
                            Game.DisplaySubtitle("~b~Student: ~w~I think someone is following me. They've been behind me for blocks.");
                            break;
                        case 3:
                            Game.DisplaySubtitle("~g~You: ~w~Do you know who it is?");
                            break;
                        case 4:
                            Game.DisplaySubtitle($"~b~Student: ~w~No, but it’s a ~y~{gender} ~w~who looks about ~y~{age}~w~.");
                            break;
                        case 5:
                            Game.DisplaySubtitle("~g~You: ~w~Alright. I’ll try to catch up to them.");
                            break;
                        case 6:
                            // Initiate pursuit
                            if (Main.CalloutInterface)
                            {
                                CalloutInterfaceAPI.Functions.SendMessage(this, $"Victim provided a suspect description:\nGender: {gender}\nEstimated Age: {age}\nSuspect has fled.");
                            }

                            LHandle Pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                            LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(Pursuit, Stalker);
                            LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                            LSPD_First_Response.Mod.API.Functions.SetPursuitCopsCanJoin(Pursuit, true);

                            GatheredInfo = true;
                            IsInDialogue = false;
                            break;
                    }

                    DialogueStep++;
                    GameFiber.Wait(200);
                }
            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Stalker))
            {
                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspect apprehended. Code 4.");
                this.End();
            }

            if (Stalker.IsDead)
            {
                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspect is down. Code 4.");
                this.End();
            }

            if (Game.IsKeyDown(Settings.EndCallout))
            {
                this.End();
            }
        }


        public override void End()
        {
            //First Line
            base.End();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (Stalker.Exists()) Stalker.Dismiss();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - StalkingReport - Callout cleaned up.");
        }
    }
}

