using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Drawing;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Intoxicated Student", CalloutProbability.Medium, "Reports of a possibly intoxicated student wandering on campus.", "Code 2", "ULSAPD")]
    public class IntoxicatedStudent : Callout
    {
        private Ped Student;
        private Ped attacker;
        private Blip attackerBlip;
        private Ped attacker2;
        private Blip attackerBlip2;
        private Ped attacker3;
        private Blip attackerBlip3;
        private Blip StudentBlip;

        private Vector3 StudentSpawn;
        private float StudentHeading;

        private Vector3 AttackerSpawn;
        private float AttackerHeading;

        private bool OnScene = false;
        private bool GatheredInfo = false;

        private Random rand = new Random();

        private int DialogueStep = 0;
        private bool IsInDialogue = false;
        private int DialogueVariant = -1;


        public override bool OnBeforeCalloutDisplayed()
        {
            // Set spawn location
            StudentSpawn = new Vector3(-1649.038f, 215.1764f, 60.64111f);
            StudentHeading = 165.2702f;

            AttackerHeading = 299.1626f; // Attacker 1 heading
            AttackerSpawn = new Vector3(-1658.924f, 249.8324f, 62.39095f); // Attacker 1 spawn

            CalloutPosition = StudentSpawn;

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);
            CalloutMessage = "Intoxicated Student";
            CalloutAdvisory = "911 Caller reports of a possibly intoxicated student wandering on campus.";

            if (Settings.UseBluelineAudio)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_DISTURBING_THE_PEACE_02 IN_OR_ON_POSITION", CalloutPosition);
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_CIVILIAN_NEEDING_ASSISTANCE_01 IN_OR_ON_POSITION", CalloutPosition);
            }

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_02_02");

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Spawn student
            Student = new Ped(StudentSpawn, StudentHeading);
            Student.MakePersistent();
            Student.BlockPermanentEvents = true;

            // Make the student walk around aimlessly or appear drunk
            Student.Tasks.Wander();
            Student.Tasks.PlayAnimation("move_m@drunk@verydrunk_idles@", "fidget_07", 1.0f, AnimationFlags.Loop); // drunk sway idle

            // Create blip
            StudentBlip = Student.AttachBlip();
            StudentBlip.Color = Color.Yellow;
            StudentBlip.EnableRoute(Color.Yellow);

            Game.DisplayHelp("Locate the intoxicated student and assess the situation.");
            Game.LogTrivial("CampusCallouts - IntoxicatedStudent - Student spawned and wandering.");

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (Student.Exists()) Student.Dismiss();
            if (StudentBlip.Exists()) StudentBlip.Delete();
            if (attackerBlip.Exists()) attackerBlip.Delete();
            if (attacker.Exists()) attacker.Dismiss();
            if (attacker2.Exists()) attacker2.Dismiss();
            if (attackerBlip2.Exists()) attackerBlip2.Delete();
            if (attacker3.Exists()) attacker3.Dismiss();
            if (attackerBlip3.Exists()) attackerBlip3.Delete();
        }

        public override void Process()
        {
            base.Process();

            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Student) <= 10f)
            {
                OnScene = true;
                Game.DisplaySubtitle("~y~[INFO]~w~ Approach and speak to the student.");
            }

            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Student) <= 3f)
            {
                if (!IsInDialogue)
                {
                    Student.Tasks.Clear();
                    Student.Face(Game.LocalPlayer.Character);
                    DialogueVariant = rand.Next(0, 2); // Randomize once
                    DialogueStep = 0;
                    IsInDialogue = true;
                    Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to advance dialogue. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
                }

                if (Game.IsKeyDown(Settings.DialogueKey))
                {
                    GameFiber.Wait(200); // Debounce
                    switch (DialogueVariant)
                    {
                        case 0:
                            switch (DialogueStep)
                            {
                                case 0:
                                    Game.DisplaySubtitle("~b~You: ~w~Hey, are you alright? You look pretty out of it.");
                                    break;
                                case 1:
                                    Game.DisplaySubtitle("~r~Student: ~w~Yeah... no. I just got into a stupid fight with my partner.");
                                    break;
                                case 2:
                                    Game.DisplaySubtitle("~r~Student: ~w~We’d been drinking. I said things I didn’t mean. It got... bad.");
                                    break;
                                case 3:
                                    Game.DisplaySubtitle("~b~You: ~w~Alright. Just stay calm. We can figure this out.");
                                    break;
                                case 4:
                                    Game.DisplaySubtitle("~r~Student: ~w~Wait... oh no. That’s them coming now...");
                                    break;
                                case 5:
                                    // Spawn hostile group (significant other + friends)
                                    Vector3 spawnPos = Student.GetOffsetPositionFront(7f);
                                    attacker = new Ped(AttackerSpawn, AttackerHeading);
                                    attacker.MakePersistent();
                                    attacker.BlockPermanentEvents = true;
                                    attacker.Inventory.GiveNewWeapon("WEAPON_BAT", -1, true);
                                    attacker.Tasks.FightAgainst(Student);
                                    attackerBlip = new Blip(attacker) { Color = Color.Red };

                                    attacker2 = new Ped(attacker.GetOffsetPositionFront(1.5f), attacker.Heading);
                                    attacker2.MakePersistent();
                                    attacker2.BlockPermanentEvents = true;
                                    attacker2.Inventory.GiveNewWeapon("WEAPON_BAT", -1, true);
                                    attacker2.Tasks.FightAgainst(Student);
                                    attackerBlip2 = new Blip(attacker2) { Color = Color.Red };

                                    attacker3 = new Ped(attacker.GetOffsetPositionFront(-1.5f), attacker.Heading);
                                    attacker3.MakePersistent();
                                    attacker3.BlockPermanentEvents = true;
                                    attacker3.Inventory.GiveNewWeapon("WEAPON_BAT", -1, true);
                                    attacker3.Tasks.FightAgainst(Student);
                                    attackerBlip3 = new Blip(attacker3) { Color = Color.Red };

                                    Game.DisplayNotification("The situation has escalated! Additional parties have arrived with weapons.");
                                    Game.LogTrivial("CampusCallouts - Intoxicated Student - Hostile group spawned.");
                                    GatheredInfo = true;
                                    IsInDialogue = false;
                                    break;
                            }
                            DialogueStep++;
                            break;

                        case 1:
                            switch (DialogueStep)
                            {
                                case 0:
                                    Game.DisplaySubtitle("~b~You: ~w~Hey. Stop right there.");
                                    break;
                                case 1:
                                    Game.DisplaySubtitle("~r~Student: ~w~Wh-whaaa? I’m juss walkin’ man… chillll.");
                                    break;
                                case 2:
                                    Game.DisplaySubtitle("~b~You: ~w~You’ve been reported for being drunk on campus. You're clearly not okay.");
                                    break;
                                case 3:
                                    Game.DisplaySubtitle("~r~Student: ~w~I ain’t hurtin’ no one… I jus' needed fresh airrr…");
                                    break;
                                case 4:
                                    Game.DisplaySubtitle("~b~You: ~w~You're stumbling around drunk in public. That’s a safety issue.");
                                    break;
                                case 5:
                                    Game.DisplaySubtitle("~r~Student: ~w~This school’s a joke anyway… nobody even cares.");
                                    break;
                                case 6:
                                    Game.DisplayNotification("The student is clearly intoxicated but not combative. Handle accordingly.");
                                    GatheredInfo = true;
                                    IsInDialogue = false;
                                    break;
                            }
                            DialogueStep++;
                            break;
                    }
                }
            }


            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Student) || Game.IsKeyDown(Settings.EndCallout) || Student.IsDead)
            {
                End();
            }
        }

        public override void End()
        {
            base.End();
            if (Student.Exists()) Student.Dismiss();
            if (StudentBlip.Exists()) StudentBlip.Delete();
            if (attackerBlip.Exists()) attackerBlip.Delete();
            if (attacker.Exists()) attacker.Dismiss();
            if (attacker2.Exists()) attacker2.Dismiss();
            if (attackerBlip2.Exists()) attackerBlip2.Delete();
            if (attacker3.Exists()) attacker3.Dismiss();
            if (attackerBlip3.Exists()) attackerBlip3.Delete();


            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - IntoxicatedStudent - Callout cleaned up.");
        }
    }
}
