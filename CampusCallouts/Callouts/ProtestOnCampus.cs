using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using NAudio.Wave;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Protest on Campus", CalloutProbability.Medium, "ULSA staff report a large group of students protesting. Situation may escalate.", "Code 3", "ULSAPD")]
    public class ProtestOnCampus : Callout
    {
        // Ped management
        private List<Ped> Protestors = new List<Ped>();
        private List<Ped> Hostiles = new List<Ped>();
        private Ped Teacher1;
        private Ped Teacher2;
        private Ped Dean;

        // Blips
        private Blip DeanBlip;
        private Blip routeBlip;

        // Protest location data
        private Vector3 ProtestLocation = new Vector3(-1650f, 215f, 60.5f);
        private Vector3 DeanDestination = new Vector3(-1708.795f, 77.58182f, 65.76763f);
        private Random rand = new Random();
        private int scenarioOption;
        private bool OnScene = false;
        private int DialogueStep = 0;
        private bool EscortStarted = false;

        // Protestor model pool and positions
        private readonly List<string> protestorModels = new List<string> {
            "a_m_y_soucent_01", "a_m_y_beachvesp_01", "a_f_y_hipster_01", "a_f_y_bevhills_01", "a_m_y_hipster_01",
            "a_f_y_eastsa_01", "a_f_y_soucent_01", "a_m_y_skater_01", "a_m_y_genstreet_01", "a_f_y_vinewood_01"
        };
        private Vector3[] protestorPositions = new Vector3[] {
            new Vector3(-1609.974f, 233.0879f, 59.72863f),
            new Vector3(-1606.334f, 231.8275f, 59.46855f),
            new Vector3(-1602.697f, 233.2524f, 59.32865f),
            new Vector3(-1604.24f, 236.5777f, 59.37085f),
            new Vector3(-1605.595f, 239.4767f, 59.37411f),
            new Vector3(-1602.211f, 241.1233f, 59.32285f),
            new Vector3(-1600.589f, 237.7915f, 59.27518f),
            new Vector3(-1598.847f, 234.2126f, 59.23326f),
            new Vector3(-1595.882f, 235.6555f, 59.17066f),
            new Vector3(-1597.23f, 238.5398f, 59.22754f)
        };
        private float[] headings = new float[10] { 117f, 115f, 115f, 115f, 115f, 115f, 115f, 115f, 115f, 115f };

        // NAudio fields
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private string protestMusicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            @"LSPDFR\Audio\Scanner\CampusCallouts - Audio\Protest\CC_PROTEST_AUDIO.wav");

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutPosition = ProtestLocation;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);

            CalloutMessage = "Protest on Campus";
            CalloutAdvisory = "ULSA staff report a large group of students protesting. Situation may escalate.";

            if (Settings.UseBluelineAudio)
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("POSSIBLE_DISTURBANCE", CalloutPosition);
            else
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_POSSIBLE_DISTURBANCE IN_OR_ON_POSITION", CalloutPosition);

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_03_02");
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            scenarioOption = rand.Next(1, 4);
            Vector3 deanPos = new Vector3(-1619.931f, 228.1242f, 60.18069f);

            // Spawn protestors
            Rage.Native.NativeFunction.CallByName<uint>("REQUEST_ANIM_DICT", "missheistdockssetup1leadinoutig_1");
            while (!Rage.Native.NativeFunction.CallByName<bool>("HAS_ANIM_DICT_LOADED", "missheistdockssetup1leadinoutig_1")) GameFiber.Yield();

            for (int i = 0; i < 10; i++)
            {
                Ped protestor = new Ped(protestorModels[rand.Next(protestorModels.Count)], protestorPositions[i], headings[i]);
                protestor.MakePersistent();
                protestor.BlockPermanentEvents = true;
                protestor.Tasks.PlayAnimation("missheistdockssetup1leadinoutig_1", "lsdh_ig_1_argue_les", 1f, AnimationFlags.Loop);
                Protestors.Add(protestor);
            }

            // Spawn staff
            Dean = new Ped("A_M_M_Business_01", deanPos, 296f);
            Teacher1 = new Ped("A_M_Y_Business_01", new Vector3(-1621.077f, 230.687f, 60.23051f), 293f);
            Teacher2 = new Ped("A_F_Y_Business_03", new Vector3(-1618.666f, 225.3496f, 60.11632f), 297f);
            foreach (Ped staff in new[] { Dean, Teacher1, Teacher2 }) { staff.MakePersistent(); staff.BlockPermanentEvents = true; }

            // Blip and interface
            DeanBlip = Dean.AttachBlip(); DeanBlip.Color = Color.Blue; DeanBlip.EnableRoute(Color.Blue);
            if (Main.CalloutInterface) CalloutInterfaceAPI.Functions.SendMessage(this, "Protest at ULSA campus reported. Staff are concerned about safety. Respond to scene.");
            Game.DisplayHelp("Respond to the protest location and speak to the Dean.");
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!OnScene && Dean.Exists() && Game.LocalPlayer.Character.Position.DistanceTo(Dean) < 30f)
            {
                OnScene = true;
                Dean.Face(Game.LocalPlayer.Character);
                DeanBlip.DisableRoute();
                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to advance dialogue. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");

                if (System.IO.File.Exists(protestMusicPath))
                {
                    try
                    {
                        audioFile = new AudioFileReader(protestMusicPath);
                        audioFile.Volume = 0.5f;
                        outputDevice = new WaveOutEvent();
                        outputDevice.Init(audioFile);
                        outputDevice.Play();

                        outputDevice.PlaybackStopped += (s, e) =>
                        {
                            if (audioFile != null && outputDevice != null)
                            {
                                try
                                {
                                    audioFile.Position = 0;
                                    outputDevice.Play();
                                }
                                catch (InvalidOperationException ex)
                                {
                                    Game.LogTrivial("CampusCallouts - Protest - PlaybackStopped error: " + ex.Message);
                                }
                            }
                        };

                        Game.LogTrivial("CampusCallouts - Protest - Protest music started.");
                    }
                    catch (Exception ex)
                    {
                        Game.LogTrivial("CampusCallouts - Protest - Failed to play protest music: " + ex.Message);
                    }
                }
            }

            if (OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Dean) < 4f && Game.IsKeyDown(Settings.DialogueKey) && !EscortStarted)
            {
                RunDialogue();
                GameFiber.StartNew(() => GameFiber.Sleep(300));
            }

            if (EscortStarted && Dean.Exists() && Dean.Position.DistanceTo(DeanDestination) > 15f && Hostiles.Count == 0)
            {
                int attackers = rand.Next(1, 8);
                for (int i = 0; i < attackers; i++)
                {
                    Ped attacker = Protestors[rand.Next(Protestors.Count)];
                    if (!Hostiles.Contains(attacker) && attacker.Exists() && !attacker.IsDead)
                    {
                        attacker.BlockPermanentEvents = false;
                        attacker.Tasks.FightAgainst(Dean);
                        Hostiles.Add(attacker);
                    }
                }
                Game.DisplayNotification("Some protestors are trying to harm the Dean!");
            }

            if (EscortStarted && Game.LocalPlayer.Character.Position.DistanceTo(DeanDestination) < 3f)
            {
                routeBlip?.DisableRoute();
                Game.DisplayNotification("Dean has been escorted to safety. Situation under control.");
                CalloutInterfaceAPI.Functions.SendMessage(this, "Dean escorted successfully. Callout ended.");
                End();
            }

            if (Game.IsKeyDown(Settings.EndCallout) || Dean.IsDead)
                End();
        }

        private void RunDialogue()
        {
            switch (DialogueStep++)
            {
                case 0: Game.DisplaySubtitle("~b~Dean: ~w~Thank you for coming, officer."); break;
                case 1: Game.DisplaySubtitle("~g~You: ~w~What’s going on here?"); break;
                case 2: Game.DisplaySubtitle("~b~Dean: ~w~It started as a peaceful protest, but we’re concerned about potential escalation."); break;
                case 3: Game.DisplaySubtitle("~g~You: ~w~Understood. Let me assess the situation."); break;
                case 4: HandleScenario(); break;
            }
        }

        private void HandleScenario()
        {
            switch (scenarioOption)
            {
                case 1:
                    EscortStarted = true;
                    Game.DisplaySubtitle("~b~Dean: ~w~Please escort me to a safe area.");
                    Game.DisplayNotification("Escort the Dean to the safe location marked on your GPS.");
                    Dean.Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, new Vector3(0.5f, 0.5f, 0f));
                    routeBlip = new Blip(DeanDestination) { Color = Color.Yellow };
                    routeBlip.EnableRoute(Color.Yellow); break;

                case 2:
                    Ped attacker = Protestors[rand.Next(Protestors.Count)];
                    attacker.Tasks.FightAgainst(Game.LocalPlayer.Character);
                    attacker.BlockPermanentEvents = false;
                    Game.DisplayNotification("One protestor is becoming aggressive!"); break;

                case 3:
                    int count = Math.Max(1, Protestors.Count / 5);
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

        public override void End()
        {
            base.End();

            foreach (var p in Protestors) if (p.Exists()) p.Dismiss();
            foreach (var h in Hostiles) if (h.Exists()) h.Dismiss();
            if (Teacher1.Exists()) Teacher1.Dismiss();
            if (Teacher2.Exists()) Teacher2.Dismiss();
            if (Dean.Exists()) Dean.Dismiss();
            if (DeanBlip.Exists()) DeanBlip.Delete();
            if (routeBlip.Exists()) routeBlip.Delete();

            try
            {
                if (outputDevice != null)
                {
                    outputDevice.Stop();
                    outputDevice.Dispose();
                    outputDevice = null;
                }

                if (audioFile != null)
                {
                    audioFile.Dispose();
                    audioFile = null;
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial("CampusCallouts - Protest - Error during audio cleanup: " + ex.Message);
            }

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - Protest - Callout cleaned up.");
        }
    }
}
