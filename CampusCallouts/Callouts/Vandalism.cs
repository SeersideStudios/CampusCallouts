using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Drawing;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Vandalism", CalloutProbability.Medium, "911 Caller reports of someone vandalizing university property", "Code 2", "ULSAPD")]
    public class Vandalism : Callout
    {
        // --- Entity References ---
        private Ped Suspect;
        private Blip SuspectBlip;

        // --- Spawn Settings ---
        private Vector3 SuspectSpawn;
        private float SuspectHeading;

        // --- Callout State Flags ---
        private bool OnScene = false;
        private bool GatheredInfo = false;
        private bool IsInDialogue = false;

        // --- Dialogue Handling ---
        private Random rand = new Random();
        private int DialogueStep = 0;
        private int dialogueVariant = -1; // 0 = compliant, 1 = hostile

        public override bool OnBeforeCalloutDisplayed()
        {
            // Set spawn location and heading
            SuspectSpawn = new Vector3(-1608.392f, 181.1946f, 59.55967f);
            SuspectHeading = 84.38181f;
            CalloutPosition = SuspectSpawn;

            // UI Setup
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);
            CalloutMessage = "Vandalism in Progress";
            CalloutAdvisory = "911 Caller reports of someone vandalizing university property.";

            // Scanner Audio (BlueLine or custom)
            if (Settings.UseBluelineAudio)
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_CRIMINAL_ACTIVITY_04", CalloutPosition);
            else
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_CRIMINAL_ACTIVITY_05 IN_OR_ON_POSITION", CalloutPosition);

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_02_02");

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Spawn the suspect
            Suspect = new Ped(SuspectSpawn, SuspectHeading);
            if (Suspect.Exists())
            {
                Suspect.MakePersistent();
                Suspect.BlockPermanentEvents = true;

                // Give suspect a weapon (bat)
                Suspect.Inventory.GiveNewWeapon("WEAPON_BAT", -1, true);
                Suspect.Tasks.PlayAnimation("melee@large_wpn@streamed_core", "plyr_rear_takedown_bat_r_facehit", 1.0f, AnimationFlags.Loop);

                // Create blip with route
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.Color = Color.Red;
                SuspectBlip.EnableRoute(Color.Red);

                Game.DisplayHelp("A student appears to be smashing school property with a bat. Approach and investigate.");
                Game.LogTrivial("CampusCallouts - Vandalism - Suspect spawned and swinging bat.");
            }

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (Suspect.Exists()) Suspect.Dismiss();
            if (SuspectBlip.Exists()) SuspectBlip.Delete();
        }

        public override void Process()
        {
            base.Process();

            // Player approaches the scene
            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Suspect) <= 10f)
            {
                OnScene = true;
                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to advance dialogue. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
                Game.DisplaySubtitle("~y~[INFO]~w~ Speak to the suspect to gather information.");
            }

            // Initiate dialogue if close and not already handled
            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Suspect) <= 3f)
            {
                if (!IsInDialogue)
                {
                    Suspect.Tasks.Clear();
                    Suspect.Face(Game.LocalPlayer.Character);
                    IsInDialogue = true;
                    DialogueStep = 0;
                    dialogueVariant = rand.Next(0, 2);
                    Game.LogTrivial($"CampusCallouts - Vandalism - Dialogue variation: {dialogueVariant}");
                }

                if (Game.IsKeyDown(Settings.DialogueKey))
                {
                    if (dialogueVariant == 1) // Hostile variant
                    {
                        switch (DialogueStep)
                        {
                            case 0:
                                Game.DisplaySubtitle("~b~You: ~w~Put the bat down! Step away from the flagpole!");
                                break;
                            case 1:
                                Game.DisplaySubtitle("~r~Suspect: ~w~They rejected my transfer again! This school’s a joke.");
                                break;
                            case 2:
                                Game.DisplaySubtitle("~b~You: ~w~Smashing things won’t fix that. Drop the weapon, now!");
                                break;
                            case 3:
                                Game.DisplaySubtitle("~r~Suspect: ~w~No! I’ve had it! You’re not stopping me.");
                                break;
                            case 4:
                                Game.DisplayNotification("The suspect is becoming aggressive!");
                                Suspect.BlockPermanentEvents = false;
                                Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);
                                GatheredInfo = true;
                                IsInDialogue = false;
                                break;
                        }
                    }
                    else // Compliant variant
                    {
                        switch (DialogueStep)
                        {
                            case 0:
                                Game.DisplaySubtitle("~b~You: ~w~Put the bat down. What the hell are you doing?");
                                break;
                            case 1:
                                Game.DisplaySubtitle("~r~Suspect: ~w~I... I’m sorry. I just lost it.");
                                break;
                            case 2:
                                Game.DisplaySubtitle("~b~You: ~w~You're damaging school property. That’s a serious offense.");
                                break;
                            case 3:
                                Game.DisplaySubtitle("~r~Suspect: ~w~I know. I just got an email saying I failed my final... I snapped.");
                                break;
                            case 4:
                                Game.DisplaySubtitle("~b~You: ~w~That doesn’t excuse this behavior. You’re lucky someone didn’t get hurt.");
                                break;
                            case 5:
                                Game.DisplayNotification("The suspect appears remorseful and is complying. Deal with him as you deem fit.");
                                GatheredInfo = true;
                                IsInDialogue = false;
                                break;
                        }
                    }

                    DialogueStep++;
                    GameFiber.StartNew(() => GameFiber.Sleep(200)); // Non-blocking debounce
                }
            }

            // End conditions
            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Suspect) || Game.IsKeyDown(Settings.EndCallout) || Suspect.IsDead)
            {
                End();
            }
        }

        public override void End()
        {
            base.End();
            if (Suspect.Exists()) Suspect.Dismiss();
            if (SuspectBlip.Exists()) SuspectBlip.Delete();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - Vandalism - Callout cleaned up.");
        }
    }
}
