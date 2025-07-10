// Enhanced MissingStudent.cs with added guidance dialogues

using System;
using System.Collections.Generic;
using System.Drawing;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using CalloutInterfaceAPI;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Missing Student", CalloutProbability.Medium, "A student has been reported missing by their roommate.", "Code 3", "ULSAPD")]
    public class MissingStudent : Callout
    {
        // Peds and Blips 
        private Ped Roommate;
        private Ped Officer;
        private Ped Student;
        private Ped Attacker;
        private Blip TargetBlip;

        // Locations 
        private Vector3 DormLocation = new Vector3(-1671.686f, 174.0843f, 61.75573f);

        private Vector3 OfficerBeach = new Vector3(0, 0, 0);
        private Vector3 OfficerObservatory = new Vector3(0, 0, 0);
        private Vector3 OfficerPier = new Vector3(0, 0, 0);

        private Vector3 StudentBeach = new Vector3(0, 0, 0);
        private Vector3 StudentObservatory = new Vector3(0, 0, 0);
        private Vector3 StudentPier = new Vector3(0, 0, 0);

        private Vector3 AttackerBeach = new Vector3(0, 0, 0);
        private Vector3 AttackerObservatory = new Vector3(0, 0, 0);
        private Vector3 AttackerPier = new Vector3(0, 0, 0);

        private enum LeadType { Beach, Observatory, Pier }
        private LeadType Scenario;

        // Checkers 
        private int DialogueStep = 0;
        private bool OnScene = false;
        private bool IsInDialogue = false;
        private bool AttackerSpawned = false;
        private bool CombatResolved = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutPosition = DormLocation;
            ShowCalloutAreaBlipBeforeAccepting(DormLocation, 30f);
            AddMinimumDistanceCheck(20f, DormLocation);

            CalloutMessage = "Missing Student";
            CalloutAdvisory = "A student has been reported missing by their roommate. Please investigate.";

            if (Settings.UseBluelineAudio)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_CIVILIAN_REQUIRING_ASSISTANCE IN_OR_ON_POSITION", CalloutPosition);
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_CIVILIAN_NEEDING_ASSISTANCE_01 IN_OR_ON_POSITION", CalloutPosition);
            }

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_03_02");
            Game.LogTrivial("CampusCallouts - Missing Student - Callout setup complete");
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Scenario = (LeadType)new Random().Next(0, 3);

            Roommate = new Ped("a_f_y_hipster_01", DormLocation, 180f);
            Roommate.BlockPermanentEvents = true;
            Roommate.IsPersistent = true;

            TargetBlip = Roommate.AttachBlip();
            TargetBlip.Color = Color.Yellow;
            TargetBlip.EnableRoute(Color.Yellow);

            Game.LogTrivial("CampusCallouts - Missing Student - Roommate created");
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (Game.LocalPlayer.Character.IsDead || Roommate.IsDead || Student.IsDead || Game.IsKeyDown(Settings.EndCallout)) End();

            if (!OnScene && Game.LocalPlayer.Character.DistanceTo(Roommate.Position) < 10f)
            {
                OnScene = true;
                TargetBlip.DisableRoute();
                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to advance dialogue. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
            }

            if (OnScene && Game.LocalPlayer.Character.DistanceTo(Roommate.Position) < 3f && Game.IsKeyDown(Settings.DialogueKey))
            {
                if (!IsInDialogue) { IsInDialogue = true; DialogueStep = 0; Roommate.Face(Game.LocalPlayer.Character); }
                RunRoommateDialogue();
            }

            if (Officer != null && DialogueStep >= 5 && Game.LocalPlayer.Character.DistanceTo(Officer.Position) < 10f)
            {
                TargetBlip.DisableRoute();
                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to talk to the officer.");

                if (Game.IsKeyDown(Settings.DialogueKey))
                {
                    Officer.Face(Game.LocalPlayer.Character);
                    RunOfficerDialogue();
                }
            }

            if (!AttackerSpawned && Student != null && Game.LocalPlayer.Character.DistanceTo(Student.Position) < 20f)
            {
                AttackerSpawned = true;
                if (Attacker != null)
                {
                    Game.DisplaySubtitle("~r~Stranger: Get lost, this isn’t your business!");
                    GameFiber.Sleep(2000);
                    Attacker.Tasks.FightAgainst(Game.LocalPlayer.Character);
                }
            }

            if (!CombatResolved && AttackerSpawned && (Attacker == null || !Attacker.Exists() || Attacker.IsDead || LSPD_First_Response.Mod.API.Functions.IsPedArrested(Attacker)))
            {
                CombatResolved = true;
                Game.DisplayNotification("Speak to the student to see if they’re alright.");
            }

            if (CombatResolved && Game.LocalPlayer.Character.DistanceTo(Student.Position) < 5f && Game.IsKeyDown(Settings.DialogueKey))
            {
                Student.Face(Game.LocalPlayer.Character);
                RunStudentDialogue();
            }
        }

        private void RunRoommateDialogue()
        {
            string[] lines = new string[] {
                "Officer, thank you for coming...",
                "My roommate hasn’t been home since last night.",
                "They were exploring somewhere before class.",
                Scenario == LeadType.Beach ? "They said they’d check out some structures near the beach." :
                Scenario == LeadType.Observatory ? "They were taking photos near the observatory." :
                "They had a group meeting at the pier.",
                "Please find them... I’m really worried."
            };

            if (DialogueStep < lines.Length) Game.DisplaySubtitle(lines[DialogueStep]);
            DialogueStep++;

            if (DialogueStep == 5)
            {
                Roommate.Dismiss();

                Vector3 pos = Scenario == LeadType.Beach ? OfficerBeach : Scenario == LeadType.Observatory ? OfficerObservatory : OfficerPier;
                Officer = new Ped("s_m_y_cop_01", pos, 0f);
                Officer.BlockPermanentEvents = true;
                Officer.IsPersistent = true;
                TargetBlip = Officer.AttachBlip();
                TargetBlip.Color = Color.Blue;
                TargetBlip.EnableRoute(Color.Blue);
                Game.DisplayNotification("A nearby officer reported seeing someone who may match the description. Go speak to them.");

                Game.LogTrivial("CampusCallouts - Missing Student - Officer spawned");
            }
        }

        private void RunOfficerDialogue()
        {
            string[] lines = new string[] {
                "You’re here for the missing student, right?",
                "Someone matching the description was nearby earlier.",
                "They looked shaken and wandered off that way.",
                "There’s a small path behind the building they might’ve taken. Go check it out and let me know what you find."
            };

            if (DialogueStep - 5 < lines.Length) Game.DisplaySubtitle(lines[DialogueStep - 5]);
            DialogueStep++;

            if (DialogueStep == 9)
            {
                Officer.Dismiss();
                TargetBlip.Delete();

                Vector3 studentPos = Scenario == LeadType.Beach ? StudentBeach : Scenario == LeadType.Observatory ? StudentObservatory : StudentPier;
                Vector3 attackerPos = Scenario == LeadType.Beach ? AttackerBeach : Scenario == LeadType.Observatory ? AttackerObservatory : AttackerPier;

                Student = new Ped("A_F_Y_StudioParty_02", studentPos, 0f);
                Student.BlockPermanentEvents = true;
                Student.IsPersistent = true;
                Student.Tasks.PlayAnimation("anim@heists@fleeca_bank@hostages@ped_d@cower", "cower", 1f, AnimationFlags.Loop);
                TargetBlip = Student.AttachBlip();
                TargetBlip.Color = Color.White;
                TargetBlip.EnableRoute(Color.White);

                if (new Random().NextDouble() < 0.5)
                {
                    Attacker = new Ped("g_m_y_lost_01", attackerPos, 0f);
                    Attacker.BlockPermanentEvents = true;
                    Attacker.IsPersistent = true;
                    Game.LogTrivial("CampusCallouts - Missing Student - Attacker spawned");
                }

                Game.DisplayNotification("Search the area ahead. Look for the missing student.");
                Game.LogTrivial("CampusCallouts - Missing Student - Student spawned");
            }
        }

        private void RunStudentDialogue()
        {
            string[] lines = new string[] {
                Attacker != null ? "You saved me... I didn’t know what to do." : "Thank you for finding me...",
                "I was just trying to clear my head. I didn’t think it’d become all this.",
                "Can you help me get home?"
            };

            if (DialogueStep - 9 < lines.Length) Game.DisplaySubtitle(lines[DialogueStep - 9]);
            DialogueStep++;

            if (DialogueStep == 12)
            {
                TargetBlip.Delete();
                Game.DisplayNotification("~y~Use Stop The Ped to call a taxi for the student.");
                Game.LogTrivial("CampusCallouts - Missing Student - Callout complete");
                End();
            }
        }

        public override void End()
        {
            Roommate?.Dismiss();
            Officer?.Dismiss();
            Student?.Dismiss();
            Attacker?.Dismiss();
            TargetBlip?.Delete();
            Game.LogTrivial("CampusCallouts - Missing Student - Cleaned up");
            base.End();
        }
    }
}
