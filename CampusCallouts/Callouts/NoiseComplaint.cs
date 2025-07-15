using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using NAudio.Wave;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Noise Complaint", CalloutProbability.Medium, "Neighbors report loud party activity near the dorm backyards.", "Code 2", "ULSAPD")]
    public class NoiseComplaint : Callout
    {
        private Vector3 CalloutLocation = new Vector3(-1750.335f, 365.4604f, 89.23333f);

        private Ped[] Partygoers;
        private readonly Vector3[] PedSpawns = new Vector3[]
        {
            new Vector3(-1721.035f, 366.1222f, 89.77831f),
            new Vector3(-1718.091f, 369.0178f, 89.77727f),
            new Vector3(-1715.616f, 369.4782f, 89.77764f),
            new Vector3(-1718.054f, 367.1187f, 89.7297f),
            new Vector3(-1724.905f, 368.9243f, 89.78442f)
        };

        private readonly float[] PedHeadings = new float[] { 275.85f, 227.28f, 121.82f, 74.67f, 254.49f };

        private Blip AreaBlip;
        private bool OnScene = false;
        private int DialogueStep = 0;
        private bool InDialogue = false;
        private int DialogueVariant = -1;
        private Ped Speaker;
        private readonly Random rand = new Random();

        private readonly List<Rage.Object> SpawnedProps = new List<Rage.Object>();

        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;

        private readonly string musicPath = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            @"LSPDFR\Audio\Scanner\CampusCallouts - Audio\NoiseComplaint\CC_PARTY_AUDIO.wav"
        );

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutPosition = CalloutLocation;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);

            CalloutMessage = "Noise Complaint";
            CalloutAdvisory = "Neighbors report loud party activity near one of the student dorm backyards.";

            if (Settings.UseBluelineAudio)
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("POSSIBLE_DISTURBANCE", CalloutPosition);
            else
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_POSSIBLE_DISTURBANCE IN_OR_ON_POSITION", CalloutPosition);

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_02_02");
            Game.LogTrivial("CampusCallouts - NoiseComplaint - Callout setup complete");
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Partygoers = new Ped[5];
            for (int i = 0; i < 5; i++)
            {
                Partygoers[i] = new Ped(PedSpawns[i], PedHeadings[i]);
                Partygoers[i].IsPersistent = true;
                Partygoers[i].BlockPermanentEvents = true;
                Partygoers[i].Tasks.StandStill(-1);
                Partygoers[i].Tasks.PlayAnimation("amb@world_human_partying@male@partying_beer@base", "base", 1f, AnimationFlags.Loop);
                GiveBeerBottleToPed(Partygoers[i]);
                Game.LogTrivial($"CampusCallouts - NoiseComplaint - Ped {i + 1} spawned at {PedSpawns[i]}");
            }

            Speaker = Partygoers[2];
            Game.LogTrivial("CampusCallouts - NoiseComplaint - Dialogue speaker selected");

            AreaBlip = new Blip(CalloutLocation)
            {
                Color = Color.Blue
            };
            AreaBlip.EnableRoute(Color.Blue);
            Game.DisplayHelp("Make your way to the reported location and investigate the noise complaint.");
            Game.LogTrivial("CampusCallouts - NoiseComplaint - Blip and route set");
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(CalloutLocation) <= 10f)
            {
                OnScene = true;
                AreaBlip.DisableRoute();
                AreaBlip.Delete();
                AreaBlip = new Blip(Speaker.Position)
                {
                    Color = Color.Red
                };
                DialogueVariant = rand.Next(0, 3);
                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to speak to someone. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
                Game.DisplaySubtitle("~y~[INFO]~w~ Loud music and shouting can be heard from the back of the house.");

                if (System.IO.File.Exists(musicPath))
                {
                    try
                    {
                        audioFile = new AudioFileReader(musicPath);
                        outputDevice = new WaveOutEvent();
                        outputDevice.Init(audioFile);
                        outputDevice.Volume = 0.5f;
                        outputDevice.Play();

                        outputDevice.PlaybackStopped += (s, a) =>
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
                                    Game.LogTrivial("CampusCallouts - NoiseComplaint - PlaybackStopped error: " + ex.Message);
                                }
                            }
                        };

                        Game.LogTrivial("CampusCallouts - NoiseComplaint - Music started using NAudio.");
                    }
                    catch (Exception ex)
                    {
                        Game.LogTrivial("CampusCallouts - NoiseComplaint - Error playing music: " + ex.Message);
                    }
                }
                else
                {
                    Game.LogTrivial("CampusCallouts - NoiseComplaint - Music file not found.");
                }
            }

            if (OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Speaker) <= 8f && Game.IsKeyDown(Settings.DialogueKey))
            {
                HandleDialogue();
                GameFiber.StartNew(() => GameFiber.Sleep(250));
            }

            if (Game.IsKeyDown(Settings.EndCallout))
            {
                End();
            }
        }


        private void HandleDialogue()
        {
            if (!InDialogue) { InDialogue = true; DialogueStep = 0; Speaker.Face(Game.LocalPlayer.Character); }

            string[] variant0 = new string[]
            {
                "~b~You: ~w~Evening. We've had some complaints about the noise.",
                "~y~Student: ~w~Oh! Sorry, officer. We'll turn it down.",
                "~b~You: ~w~Thanks. Try to keep it quiet going forward.",
                "~y~Student: ~w~Got it. Have a good one."
            };

            string[] variant1 = new string[]
            {
                "~b~You: ~w~Evening. Neighbors are complaining about your party.",
                "~y~Student: ~w~What? It's barely even loud!",
                "~b~You: ~w~Still, it's bothering people. Please turn it down.",
                "~y~Student: ~w~Ugh... fine. Whatever."
            };

            string[] variant2 = new string[]
            {
                "~b~You: ~w~Hey there. This party's too loud, you need to shut it down.",
                "~y~Student: ~w~Not happening. It’s the weekend, we’re celebrating.",
                "~b~You: ~w~If it doesn’t stop, we might have to escalate this.",
                "~y~Student: ~w~Do what you gotta do. We’re not done partying."
            };

            string[] selected = DialogueVariant == 0 ? variant0 : DialogueVariant == 1 ? variant1 : variant2;

            if (DialogueStep < selected.Length)
            {
                Game.DisplaySubtitle(selected[DialogueStep]);
                Game.LogTrivial($"CampusCallouts - NoiseComplaint - Dialogue step {DialogueStep} displayed (Variant {DialogueVariant})");
                DialogueStep++;
            }
            else
            {
                InDialogue = false;
                Game.LogTrivial("CampusCallouts - NoiseComplaint - Dialogue completed");

                if (DialogueVariant != 2)
                {
                    Game.DisplayNotification("The partygoers agree to quiet down. You may end the call.");
                    StopMusic();
                }
                else
                {
                    Game.DisplayNotification("The partygoers refuse to comply. Consider issuing a citation or calling backup.");
                    Game.LogTrivial("CampusCallouts - NoiseComplaint - Music continues due to non-compliance");
                }
            }
        }

        public override void End()
        {
            base.End();
            foreach (Ped p in Partygoers)
                if (p.Exists()) p.Dismiss();

            if (AreaBlip.Exists()) AreaBlip.Delete();

            foreach (var obj in SpawnedProps)
                if (obj.Exists()) obj.Delete();
            SpawnedProps.Clear();

            StopMusic();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - NoiseComplaint - Callout cleaned up");
        }

        private void GiveBeerBottleToPed(Ped ped)
        {
            if (!ped.Exists()) return;

            try
            {
                // Random bottle type (optional)
                string[] beerTypes = { "prop_beer_bottle", "prop_beer_am", "prop_beer_blr", "prop_beer_logger" };
                string chosenBottle = beerTypes[new Random().Next(beerTypes.Length)];

                // Spawn beer bottle
                Rage.Object beer = new Rage.Object(chosenBottle, ped.GetOffsetPositionUp(0.5f));
                SpawnedProps.Add(beer); // Add to spawned props for cleanup

                // Attach to LEFT hand with proper offset for partying animation
                beer.AttachTo(
                    ped,
                    ped.GetBoneIndex(PedBoneId.LeftHand),
                    new Vector3(0.1230f, -0.1010f, 0.0600f),
                    new Rotator(0f, 99.6838f, 90f)

                );

                beer.IsPersistent = true;
            }
            catch (Exception ex)
            {
                Game.LogTrivial("CampusCallouts: Failed to attach left-hand beer bottle: " + ex.Message);
            }
        }



        private void StopMusic()
        {
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

                Game.LogTrivial("CampusCallouts - NoiseComplaint - Music stopped and disposed (NAudio)");
            }
            catch (Exception ex)
            {
                Game.LogTrivial("CampusCallouts - NoiseComplaint - Error in StopMusic(): " + ex.Message);
            }
        }
    }
}
