using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using WMPLib;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Protest on Campus", CalloutProbability.Medium, "ULSA staff report a large group of students protesting. Situation may escalate.", "Code 3", "ULSAPD")]
    public class ProtestOnCampus : Callout
    {
        private List<Ped> Protestors = new List<Ped>();
        private List<Ped> Hostiles = new List<Ped>();
        List<string> protestorModels = new List<string>
{
    "a_m_y_soucent_01",
    "a_m_y_beachvesp_01",
    "a_f_y_hipster_01",
    "a_f_y_bevhills_01",
    "a_m_y_hipster_01",
    "a_f_y_eastsa_01",
    "a_f_y_soucent_01",
    "a_m_y_skater_01",
    "a_m_y_genstreet_01",
    "a_f_y_vinewood_01"
};


        private Ped Teacher1;
        private Ped Teacher2;
        private Ped Dean;

        private Blip DeanBlip;
        private Blip routeBlip;
        private Vector3 ProtestLocation = new Vector3(-1650f, 215f, 60.5f);
        private Vector3 DeanDestination = new Vector3(-1708.795f, 77.58182f, 65.76763f);

        private Random rand = new Random();
        private int scenarioOption;
        private bool OnScene = false;
        private int DialogueStep = 0;
        private bool EscortStarted = false;

        private WindowsMediaPlayer protestPlayer;
        private string protestMusicPath = @"Plugins\LSPDFR\audio\scanner\CampusCallouts - Audio\Protest\CC_PROTEST_AUDIO.mp3";


        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutPosition = ProtestLocation;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);

            CalloutMessage = "Protest on Campus";
            CalloutAdvisory = "ULSA staff report a large group of students protesting. Situation may escalate.";

            if (Settings.UseBluelineAudio)
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("POSSIBLE_DISTURBANCE IN_OR_ON_POSITION", CalloutPosition);
            else
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_POSSIBLE_DISTURBANCE IN_OR_ON_POSITION", CalloutPosition);

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_03_02");

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            scenarioOption = rand.Next(1, 4); // 1, 2, or 3
            Game.LogTrivial($"CampusCallouts - Protest - Scenario selected: {scenarioOption}");

            // Fixed coordinates
            Vector3 protestorCenter = new Vector3(-1610.732f, 231.8715f, 59.78891f);
            Vector3 deanPos = new Vector3(-1622.43f, 227.5778f, 60.26468f);
            float deanHeading = 289.4158f;

            // Spawn Protestors
            int totalProtestors = rand.Next(10, 21); // 10 to 20 protestors
            for (int i = 0; i < totalProtestors; i++)
            {
                Vector3 pos = protestorCenter.Around2D(4f); // 4-meter radius
                string modelName = protestorModels[rand.Next(protestorModels.Count)];

                Ped protestor = new Ped(modelName, pos, 108.7844f);
                protestor.MakePersistent();
                protestor.BlockPermanentEvents = true;
                protestor.Tasks.PlayAnimation("missheistdockssetup1leadinoutig_1", "lsdh_ig_1_argue_les", 1f, AnimationFlags.Loop);
                Protestors.Add(protestor);
                GameFiber.Yield();
            }

            // Spawn Dean and Teachers at precise positions and heading
            Dean = new Ped("A_M_M_Business_01", deanPos, deanHeading);
            Teacher1 = new Ped("A_M_Y_Business_01", new Vector3(-1623.5f, 228.0f, 60.26468f), deanHeading);
            Teacher2 = new Ped("A_F_Y_Business_03", new Vector3(-1621.2f, 227.0f, 60.26468f), deanHeading);

            foreach (Ped staff in new[] { Dean, Teacher1, Teacher2 })
            {
                staff.MakePersistent();
                staff.BlockPermanentEvents = true;
            }

            // Blip for the Dean
            DeanBlip = Dean.AttachBlip();
            DeanBlip.Color = Color.Blue;
            DeanBlip.EnableRoute(Color.Blue);

            // Callout Interface message
            if (Main.CalloutInterface)
                CalloutInterfaceAPI.Functions.SendMessage(this, "Protest at ULSA campus reported. Staff are concerned about safety. Respond to scene.");

            Game.DisplayHelp("Respond to the protest location and speak to the Dean.");
            return base.OnCalloutAccepted();
        }


        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            foreach (var p in Protestors) if (p.Exists()) p.Dismiss();
            foreach (var h in Hostiles) if (h.Exists()) h.Dismiss();
            if (Teacher1.Exists()) Teacher1.Dismiss();
            if (Teacher2.Exists()) Teacher2.Dismiss();
            if (Dean.Exists()) Dean.Dismiss();
            if (DeanBlip.Exists()) DeanBlip.Delete();
            if (routeBlip.Exists()) routeBlip.Delete();

        }

        public override void Process()
        {
            base.Process();

            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Dean) < 15f)
            {
                OnScene = true;
                Dean.Face(Game.LocalPlayer.Character);
                DeanBlip.DisableRoute();
                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to speak to the Dean.");

                // Play custom protest music
                if (System.IO.File.Exists(protestMusicPath))
                {
                    protestPlayer = new WindowsMediaPlayer();
                    protestPlayer.URL = protestMusicPath;
                    protestPlayer.settings.setMode("loop", true); // Loop until stopped
                    protestPlayer.controls.play();
                    Game.LogTrivial("CampusCallouts - Protest music started.");
                }
                else
                {
                    Game.LogTrivial("CampusCallouts - Protest music file not found.");
                }
            }


            if (OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Dean) < 4f && Game.IsKeyDown(Settings.DialogueKey) && !EscortStarted)
            {
                RunDialogue();
                GameFiber.Sleep(300); // debounce
            }

            if (EscortStarted && Game.LocalPlayer.Character.Position.DistanceTo(DeanDestination) < 3f)
            {
                routeBlip.DisableRoute();
                Game.DisplayNotification("Dean has been escorted to safety. Situation under control.");
                CalloutInterfaceAPI.Functions.SendMessage(this, "Dean escorted successfully. Callout ended.");
                End();
            }

            if (Game.IsKeyDown(Settings.EndCallout))
            {
                End();
            }
        }

        private void RunDialogue()
        {
            switch (DialogueStep)
            {
                case 0:
                    Game.DisplaySubtitle("~b~Dean: ~w~Thank you for coming, officer.");
                    break;
                case 1:
                    Game.DisplaySubtitle("~g~You: ~w~What’s going on here?");
                    break;
                case 2:
                    Game.DisplaySubtitle("~b~Dean: ~w~It started as a peaceful protest, but we’re concerned about potential escalation.");
                    break;
                case 3:
                    Game.DisplaySubtitle("~g~You: ~w~Understood. Let me assess the situation.");
                    break;
                case 4:
                    HandleScenario();
                    return;
            }
            DialogueStep++;
        }


        private void HandleScenario()
        {
            switch (scenarioOption)
            {
                case 1:
                    Game.DisplaySubtitle("~b~Dean: ~w~Please escort me to a safe area.");
                    Game.DisplayNotification("Escort the Dean to the safe location marked on your GPS.");
                    EscortStarted = true;

                    // Make dean follow the player
                    Dean.Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, new Vector3(0.5f, 0.5f, 0f));

                    // Create a route to the safe area
                    routeBlip = new Blip(DeanDestination);
                    routeBlip.Color = Color.Yellow;
                    routeBlip.EnableRoute(Color.Yellow);
                    routeBlip.IsRouteEnabled = true;
                    routeBlip.Scale = 0.7f;
                    break;

                case 2:
                    Game.DisplaySubtitle("~r~Protestor: ~w~You pigs don’t belong here!");
                    Ped attacker = Protestors[rand.Next(Protestors.Count)];
                    attacker.Tasks.FightAgainst(Game.LocalPlayer.Character);
                    attacker.BlockPermanentEvents = false;
                    Game.DisplayNotification("One protestor is becoming aggressive!");
                    break;

                case 3:
                    int count = Math.Max(1, Protestors.Count / 5); // 5–20%
                    Game.DisplayNotification($"{count} protestors are becoming aggressive!");
                    for (int i = 0; i < count; i++)
                    {
                        Ped p = Protestors[rand.Next(Protestors.Count)];
                        if (!Hostiles.Contains(p))
                        {
                            p.BlockPermanentEvents = false;
                            p.Tasks.FightAgainst(Game.LocalPlayer.Character);
                            Hostiles.Add(p);
                        }
                    }
                    break;
            }
        }

        private Ped SpawnPed(string model, Vector3 pos)
        {
            Ped p = new Ped(model, pos, rand.Next(0, 360));
            p.MakePersistent();
            p.BlockPermanentEvents = true;
            return p;
        }


        public override void End()
        {
            base.End();
            if (protestPlayer != null)
            {
                protestPlayer.controls.stop();
                protestPlayer.close();
                protestPlayer = null;
                Game.LogTrivial("CampusCallouts - Protest music stopped.");
            }
            foreach (var p in Protestors) if (p.Exists()) p.Dismiss();
            foreach (var h in Hostiles) if (h.Exists()) h.Dismiss();
            if (Teacher1.Exists()) Teacher1.Dismiss();
            if (Teacher2.Exists()) Teacher2.Dismiss();
            if (Dean.Exists()) Dean.Dismiss();
            if (DeanBlip.Exists()) DeanBlip.Delete();
            if (routeBlip.Exists()) routeBlip.Delete();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - Protest - Callout cleaned up.");
        }
    }
}
