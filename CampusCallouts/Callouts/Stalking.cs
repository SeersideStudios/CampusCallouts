using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using CalloutInterfaceAPI;
using Rage.Native;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Stalking Report", CalloutProbability.Low, "Student reported being followed by an unknown individual on campus.", "Code 2", "ULSAPD")]
    public class StalkingReport : Callout
    {
        private Vector3 PedSpawn = new Vector3(-1819.464f, 140.2641f, 77.12f);
        private float PedHeading = 80.38177f;

        private Ped Ped;
        private Ped Stalker;
        private Blip PedBlip;

        private bool OnScene = false;
        private bool GatheredInfo = false;
        private int DialogueStep = 0;
        private bool IsInDialogue = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutPosition = PedSpawn;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);

            CalloutMessage = "Reports of a Stalking";
            CalloutAdvisory = "A student is reporting being followed by an unknown individual.";

            if (Settings.UseBluelineAudio)
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("SUSPICIOUS_PERSON_02", CalloutPosition);
            else
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_CIVILIAN_NEEDING_ASSISTANCE_01 IN_OR_ON_POSITION", CalloutPosition);

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_02_02");
            Game.LogTrivial("CampusCallouts - StalkingReport - Callout displayed.");
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Ped = new Ped(PedSpawn, PedHeading);
            if (Ped.Exists())
            {
                Ped.IsPersistent = true;
                Ped.BlockPermanentEvents = true;
                Ped.Tasks.Wander();
            }

            Stalker = new Ped(PedSpawn + new Vector3(1.5f, 0, 0), PedHeading);
            if (Stalker.Exists())
            {
                Stalker.IsPersistent = true;
                Stalker.BlockPermanentEvents = true;
                Stalker.Tasks.FollowToOffsetFromEntity(Ped, new Vector3(1.5f, 1.5f, 1.5f));
            }

            PedBlip = Ped.AttachBlip();
            PedBlip.Color = Color.Blue;
            PedBlip.EnableRoute(Color.Blue);

            Game.DisplayHelp("Speak with the student to gather details about the reported stalker.");
            Game.LogTrivial("CampusCallouts - StalkingReport - Peds spawned and logic started.");
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 10f)
            {
                OnScene = true;
                if (PedBlip.Exists()) PedBlip.DisableRoute();
                Ped.Tasks.StandStill(-1);
                NativeFunction.Natives.TASK_LOOK_AT_ENTITY(Ped, Game.LocalPlayer.Character, -1);
                Stalker.Tasks.ReactAndFlee(Game.LocalPlayer.Character);

                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey.ToString() + "~w~ to speak to the student. Press ~y~" + Settings.EndCallout.ToString() + "~w~ to end the call.");
                Game.DisplayNotification("~y~[INFO]~w~ Student appears shaken and has stopped walking.");
                Game.LogTrivial("CampusCallouts - StalkingReport - OnScene reached. Dialogue initiated.");
            }

            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 3f && Game.IsKeyDown(Settings.DialogueKey))
            {
                if (!IsInDialogue)
                {
                    IsInDialogue = true;
                    DialogueStep = 0;
                    Ped.Tasks.Clear();
                    NativeFunction.Natives.TASK_LOOK_AT_ENTITY(Ped, Game.LocalPlayer.Character, -1);
                }

                var gender = LSPD_First_Response.Mod.API.Functions.GetPersonaForPed(Stalker).Gender.ToString();
                var age = LSPD_First_Response.Mod.API.Functions.GetPersonaForPed(Stalker).ModelAge;

                string[] dialogue = new string[]
                {
                    "~b~Student: ~w~Thank you for coming, officer!",
                    "~g~You: ~w~Of course. What's going on?",
                    "~b~Student: ~w~I think someone is following me. They've been behind me for blocks.",
                    "~g~You: ~w~Do you know who it is?",
                    $"~b~Student: ~w~No, but it’s a ~y~{gender} ~w~who looks about ~y~{age}~w~.",
                    "~g~You: ~w~Alright. I’ll try to catch up to them."
                };

                if (DialogueStep < dialogue.Length)
                {
                    Game.DisplaySubtitle(dialogue[DialogueStep]);
                    DialogueStep++;
                }
                else
                {
                    LHandle pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                    if (Stalker.Exists()) LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(pursuit, Stalker);

                    LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                    LSPD_First_Response.Mod.API.Functions.SetPursuitCopsCanJoin(pursuit, true);

                    if (Main.CalloutInterface)
                    {
                        CalloutInterfaceAPI.Functions.SendMessage(this, $"Suspect identified by student:\nGender: {gender}, Approx. Age: {age}\nSuspect has fled.");
                    }

                    GatheredInfo = true;
                    IsInDialogue = false;
                    Game.LogTrivial("CampusCallouts - StalkingReport - Dialogue completed, pursuit started.");
                }

                GameFiber.StartNew(() => GameFiber.Sleep(250));
            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Stalker) || (Stalker.Exists() && Stalker.IsDead))
            {
                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspect no longer a threat. Code 4.");
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
            if (Stalker.Exists()) Stalker.Dismiss();
            if (PedBlip.Exists()) PedBlip.Delete();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - StalkingReport - Callout cleaned up.");
        }
    }
}
