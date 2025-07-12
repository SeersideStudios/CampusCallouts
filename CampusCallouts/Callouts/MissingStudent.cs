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

        // Location Vectors
        private Vector3 DormLocation = new Vector3(-1671.686f, 174.0843f, 61.75573f);

        private Vector3 OfficerBeach = new Vector3(-1312.335f, -1530.487f, 4.402397f);
        private Vector3 OfficerObservatory = new Vector3(-386.8726f, 1229.199f, 325.6562f);
        private Vector3 OfficerPier = new Vector3(-1633.238f, -1007.771f, 13.06722f);

        private Vector3 StudentBeach = new Vector3(-1488.886f, -1500.91f, 4.386162f);
        private Vector3 StudentObservatory = new Vector3(-363.62f, 1305.857f, 343.1893f);
        private Vector3 StudentPier = new Vector3(-1597.904f, -1001.693f, 7.560878f);

        private Vector3 AttackerBeach = new Vector3(-1481.177f, -1489.76f, 1.989588f);
        private Vector3 AttackerObservatory = new Vector3(-355.7941f, 1300.398f, 338.9937f);
        private Vector3 AttackerPier = new Vector3(-1599.316f, -1006.026f, 7.450534f);

        // Heading Values
        private readonly float OfficerBeachHeading = 204.5416f;
        private readonly float OfficerObservatoryHeading = 108.6074f;
        private readonly float OfficerPierHeading = 3.151609f;

        private readonly float StudentBeachHeading = 295.2511f;
        private readonly float StudentObservatoryHeading = 238.8863f;
        private readonly float StudentPierHeading = 178.0605f;

        private readonly float AttackerBeachHeading = 315.6851f;
        private readonly float AttackerObservatoryHeading = 226.0512f;
        private readonly float AttackerPierHeading = 221.3745f;

        private enum LeadType { Beach, Observatory, Pier }
        private LeadType Scenario;

        private int DialogueStep = 0;
        private int OfficerDialogueIndex = 0;
        private int StudentDialogueIndex = 0;
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
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_CIVILIAN_REQUIRING_ASSISTANCE", CalloutPosition);
            else
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_CIVILIAN_NEEDING_ASSISTANCE_01 IN_OR_ON_POSITION", CalloutPosition);

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

            if (Game.LocalPlayer.Character.IsDead ||
                (Roommate.Exists() && Roommate.IsDead) ||
                (Student.Exists() && Student.IsDead) ||
                Game.IsKeyDown(Settings.EndCallout))
            {
                End();
            }

            if (!OnScene && Roommate.Exists() && Game.LocalPlayer.Character.DistanceTo(Roommate.Position) < 10f)
            {
                OnScene = true;
                if (TargetBlip.Exists()) TargetBlip.DisableRoute();
                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to advance dialogue. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
            }

            if (OnScene && Roommate.Exists() &&
                Game.LocalPlayer.Character.DistanceTo(Roommate.Position) < 3f && Game.IsKeyDown(Settings.DialogueKey))
            {
                if (!IsInDialogue) { IsInDialogue = true; DialogueStep = 0; Roommate.Face(Game.LocalPlayer.Character); }
                RunRoommateDialogue();
            }

            if (Officer.Exists() && DialogueStep >= 6 && DialogueStep < 10)
            {
                if (Game.LocalPlayer.Character.DistanceTo(Officer.Position) < 10f)
                {
                    Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to talk to the officer.");

                    if (Game.IsKeyDown(Settings.DialogueKey))
                    {
                        Officer.Face(Game.LocalPlayer.Character);
                        RunOfficerDialogue();
                    }
                }
            }

            if (!AttackerSpawned && Student.Exists() &&
                Game.LocalPlayer.Character.DistanceTo(Student.Position) < 20f)
            {
                AttackerSpawned = true;
                if (Attacker.Exists())
                {
                    Game.DisplaySubtitle("~r~Stranger: Get lost, this isn’t your business!");
                    GameFiber.Sleep(2000);
                    Attacker.Tasks.FightAgainst(Game.LocalPlayer.Character);
                }
            }

            if (!CombatResolved && AttackerSpawned &&
                (!Attacker.Exists() || Attacker.IsDead || LSPD_First_Response.Mod.API.Functions.IsPedArrested(Attacker)))
            {
                CombatResolved = true;
                Game.DisplayNotification("Speak to the student using ~y~" + Settings.DialogueKey + "~w~ to see if they’re alright.");
            }

            if (CombatResolved && Student.Exists() &&
                Game.LocalPlayer.Character.DistanceTo(Student.Position) < 5f && Game.IsKeyDown(Settings.DialogueKey))
            {
                Student.Face(Game.LocalPlayer.Character);
                RunStudentDialogue();
            }
        }

        private void RunRoommateDialogue()
        {
            string[] lines = new string[] {
                "~y~ROOMMATE: ~w~Officer, thank you for coming...",
                "~y~ROOMMATE: ~w~My roommate hasn’t been home since last night.",
                "~y~ROOMMATE: ~w~They were exploring somewhere before class.",
                Scenario == LeadType.Beach ? "~y~STUDENT: ~w~They said they’d check out some structures near the beach." :
                Scenario == LeadType.Observatory ? "~y~STUDENT: ~w~They were taking photos near the observatory." :
                "~y~ROOMMATE: ~w~They had a group meeting at the pier.",
                "~y~ROOMMATE: ~w~Please find them... I’m really worried."
            };

            if (DialogueStep < lines.Length) Game.DisplaySubtitle(lines[DialogueStep]);
            DialogueStep++;

            if (DialogueStep == 5)
            {
                if (Roommate.Exists()) Roommate.Dismiss();

                Vector3 pos = Scenario == LeadType.Beach ? OfficerBeach :
                              Scenario == LeadType.Observatory ? OfficerObservatory : OfficerPier;
                float heading = Scenario == LeadType.Beach ? OfficerBeachHeading :
                                Scenario == LeadType.Observatory ? OfficerObservatoryHeading : OfficerPierHeading;

                Officer = new Ped("s_m_y_cop_01", pos, heading);
                Officer.BlockPermanentEvents = true;
                Officer.IsPersistent = true;

                TargetBlip = Officer.AttachBlip();
                TargetBlip.Color = Color.Blue;
                TargetBlip.EnableRoute(Color.Blue);

                DialogueStep = 6;

                Game.DisplayNotification("A nearby officer reported seeing someone who may match the description. Go speak to them.");
                Game.LogTrivial("CampusCallouts - Missing Student - Officer spawned");
            }
        }

        private void RunOfficerDialogue()
        {
            string[] lines = new string[] {
        "~b~OFFICER: ~w~You’re here for the missing student, right?",
        "~b~OFFICER: ~w~Someone matching the description was nearby earlier.",
        "~b~OFFICER: ~w~They looked shaken and wandered off that way.",
        "~b~OFFICER: ~w~There’s a small path behind the building they might’ve taken. Go check it out and let me know what you find."
    };

            if (OfficerDialogueIndex < lines.Length)
            {
                Game.DisplaySubtitle(lines[OfficerDialogueIndex]);
                OfficerDialogueIndex++;
                DialogueStep++;
            }

            if (OfficerDialogueIndex >= lines.Length)
            {
                if (Officer.Exists()) Officer.Dismiss();
                if (TargetBlip.Exists()) TargetBlip.Delete();

                Vector3 studentPos = Scenario == LeadType.Beach ? StudentBeach :
                                     Scenario == LeadType.Observatory ? StudentObservatory : StudentPier;
                float studentHeading = Scenario == LeadType.Beach ? StudentBeachHeading :
                                       Scenario == LeadType.Observatory ? StudentObservatoryHeading : StudentPierHeading;

                Vector3 attackerPos = Scenario == LeadType.Beach ? AttackerBeach :
                                      Scenario == LeadType.Observatory ? AttackerObservatory : AttackerPier;
                float attackerHeading = Scenario == LeadType.Beach ? AttackerBeachHeading :
                                         Scenario == LeadType.Observatory ? AttackerObservatoryHeading : AttackerPierHeading;

                Student = new Ped("A_F_Y_StudioParty_02", studentPos, studentHeading);
                Student.BlockPermanentEvents = true;
                Student.IsPersistent = true;
                Student.Tasks.PlayAnimation("anim@heists@fleeca_bank@hostages@ped_d@cower", "cower", 1f, AnimationFlags.Loop);

                TargetBlip = Student.AttachBlip();
                TargetBlip.Color = Color.White;
                TargetBlip.EnableRoute(Color.White);

                if (new Random().NextDouble() < 0.5)
                {
                    Attacker = new Ped("g_m_y_lost_01", attackerPos, attackerHeading);
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
            string[] dialogueWithAttacker = new string[]
            {
        "~y~STUDENT: ~w~You showed up just in time... I thought I was done for.",
        "~y~STUDENT: ~w~I didn’t expect someone to follow me out here.",
        "~y~STUDENT: ~w~Thanks for stepping in. Can you help me get back?"
            };

            string[] dialogueWithoutAttacker = new string[]
            {
        "~y~STUDENT: ~w~Hey... thank you for finding me.",
        "~y~STUDENT: ~w~I just needed space to breathe. I didn’t mean to scare anyone.",
        "~y~STUDENT: ~w~Can you help me get home?"
            };

            string[] lines = Attacker.Exists() ? dialogueWithAttacker : dialogueWithoutAttacker;

            if (StudentDialogueIndex < lines.Length)
            {
                Game.DisplaySubtitle(lines[StudentDialogueIndex]);
                StudentDialogueIndex++;
                DialogueStep++;
            }

            if (StudentDialogueIndex >= lines.Length)
            {
                if (TargetBlip.Exists()) TargetBlip.Delete();
                Game.DisplayNotification("~y~Use Stop The Ped to call a taxi for the student. Or let her be on her way.");
                Game.LogTrivial("CampusCallouts - Missing Student - Callout complete");
                End();
            }
        }



        public override void End()
        {
            base.End();
            try
            {
                if (Roommate.Exists()) Roommate.Dismiss();
                if (Officer.Exists()) Officer.Dismiss();
                if (Student.Exists()) Student.Dismiss();
                if (Attacker.Exists()) Attacker.Dismiss();
                if (TargetBlip.Exists()) TargetBlip.Delete();
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
                Game.LogTrivial("CampusCallouts - Missing Student - Cleaned up");
            }
            catch (Exception ex)
            {
                Game.LogTrivial("CampusCallouts - Missing Student - Error during End(): " + ex.ToString());
            }


        }
    }
}