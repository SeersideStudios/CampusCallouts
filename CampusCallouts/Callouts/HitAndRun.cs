using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using System.Collections.Generic;
using System;
using LSPD_First_Response.Engine.Scripting.Entities;
using CalloutInterfaceAPI;


namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Hit and Run", CalloutProbability.Medium, "A student was struck by a vehicle exiting the ULSA parking lot.", "Code 2", "ULSAPD")]
    public class HitAndRun : Callout
    {
        //Private References
        private Ped Ped;
        private Ped Suspect;

        private Vehicle SuspectCar;

        private Blip SuspectBlip;
        private Blip PedBlip;

        private Vector3 PedSpawn;
        private Vector3 SuspectCarSpawn;

        private float PedHeading;
        private float SuspectCarHeading;

        private bool PursuitAuthorized = false;
        private bool OnScene = false;
        private bool GatheredInfo = false;
        private bool PursuitDialoguePlayed = false;
        private bool TrafficStopAuthorized = false;

        private LHandle PulloverHandle;
        private LHandle Pursuit;

        private int DialogueStep = 0;
        private bool IsInDialogue = false;

        private int PulloverDialogueStep = 0;
        private bool InPulloverDialogue = false;



        private List<string> carList = new List<string>
        {
            "sentinel2",
            "oracle2",
            "jackal",
            "felon",
            "exemplar"
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            //Setting Spawn Location for Ped
            PedSpawn = new Vector3(-1708.795f, 77.58182f, 65.76763f);
            PedHeading = 234.3391f;

            //Setting the Callout location
            this.CalloutPosition = PedSpawn;


            //Setting Spawn Locations for Suspect Car
            SuspectCarSpawn = new Vector3(-1422.429f, 75.48952f, 52.06909f);
            SuspectCarHeading = 2.447696f;

            //LSPDFR Handling
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);
            CalloutMessage = "Hit and Run";

            if (Settings.UseBluelineAudio)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_PED_STRUCK_BY_VEHICLE_03");
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_HIT_AND_RUN_01 IN_OR_ON_POSITION", CalloutPosition);
            }

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_02_02");


            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            //Create Victim Ped
            Ped = new Ped(PedSpawn, PedHeading);
            Ped.MakePersistent();
            Ped.BlockPermanentEvents = true;
            Game.LogTrivial("CampusCallouts - Hit and Run - Ped created");

            //Create Blip
            PedBlip = Ped.AttachBlip();
            PedBlip.Color = Color.Blue;
            PedBlip.EnableRoute(Color.Blue);

            //Choose a random Car
            Random rand = new Random();
            int index = rand.Next(0, carList.Count); //Get random index of car choices
            string chosenVehicle = carList[index];

            //Create Vehicle
            SuspectCar = new Vehicle(chosenVehicle, SuspectCarSpawn, SuspectCarHeading);
            SuspectCar.IsPersistent = true;
            Game.LogTrivial("CampusCallouts - Hit and Run - Car created");
            GameFiber.Wait(200);

            //Create Ped for Car
            Suspect = new Ped(SuspectCar.GetOffsetPositionFront(5f));
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            Suspect.WarpIntoVehicle(SuspectCar, -1);
            Game.LogTrivial("CampusCallouts - Hit and Run - Suspect created");
            GameFiber.Wait(200);

            //Make the car start driving
            Suspect.Tasks.CruiseWithVehicle(SuspectCar, 15f, VehicleDrivingFlags.Normal);

            //Set Persona for Suspect
            // Set birthday (random between 1975–2004)
            Random ra = new Random();
            DateTime birthDate = new DateTime(ra.Next(1975, 2005), ra.Next(1, 13), ra.Next(1, 29));

            // Gender-neutral first name list
            List<string> firstNames = new List<string>
            {
                "Alex", "Taylor", "Jordan", "Casey", "Jamie",
                "Morgan", "Riley", "Quinn", "Avery", "Skyler"
            };
            string firstName = firstNames[ra.Next(firstNames.Count)];

            // Last name list
            List<string> lastNames = new List<string>
            {
                "Reed", "Walker", "Parker", "Carter", "Brooks",
                "Hayes", "Morgan", "Diaz", "Bennett", "Coleman"
            };
            string lastName = lastNames[ra.Next(lastNames.Count)];

            // Random gender
            LSPD_First_Response.Gender gender = (LSPD_First_Response.Gender)ra.Next(0, 2);

            // Create and assign persona
            Persona suspectPersona = new Persona(firstName, lastName, gender, birthDate);
            LSPD_First_Response.Mod.API.Functions.SetPersonaForPed(Suspect, suspectPersona);


            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (Ped.Exists()) { Ped.Delete(); }
            if (Suspect.Exists()) { Suspect.Delete(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (SuspectCar.Exists()) { SuspectCar.Delete(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
        }

        public override void Process()
        {
            base.Process();

            // On Scene Events
            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 10f)
            {
                OnScene = true;
                PedBlip.DisableRoute();
                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to advance dialogue. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
            }

            // Interacting with ped
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
                    switch (DialogueStep)
                    {
                        case 0:
                            Game.DisplaySubtitle("~b~Student: ~w~Officer! Thank God you're here!");
                            break;
                        case 1:
                            Game.DisplaySubtitle("~g~You: ~w~Are you okay? What happened?");
                            break;
                        case 2:
                            Game.DisplaySubtitle("~b~Student: ~w~I was crossing the street when a car came flying around the corner.");
                            break;
                        case 3:
                            Game.DisplaySubtitle("~b~Student: ~w~It hit me and just kept going! Didn’t even slow down.");
                            break;
                        case 4:
                            Game.DisplaySubtitle("~g~You: ~w~Did you get a good look at the vehicle?");
                            break;
                        case 5:
                            string model = SuspectCar.Model.Name;
                            string plate = SuspectCar.LicensePlate;
                            Game.DisplaySubtitle($"~b~Student: ~w~Yeah... it was an ~y~{model}~w~ with the plate ~y~{plate}~w~.");
                            break;
                        case 6:
                            Game.DisplaySubtitle("~g~You: ~w~Alright, I’ll see if I can find them.");
                            break;
                        case 7:
                            // Final step: cleanup and progress
                            if (Main.CalloutInterface)
                                CalloutInterfaceAPI.Functions.SendMessage(this, "Student gave a vehicle description:\nModel: " + SuspectCar.Model.Name + "\nPlate: " + SuspectCar.LicensePlate);

                            SuspectBlip = Suspect.AttachBlip();
                            SuspectBlip.Color = Color.Red;
                            SuspectBlip.EnableRoute(Color.Red);

                            Game.DisplayNotification("Find the vehicle, and interrogate the suspect.");
                            PedBlip.Delete();
                            GatheredInfo = true;
                            IsInDialogue = false;

                            PursuitAuthorized = new Random().Next(0, 2) == 0;
                            Game.LogTrivial("CampusCallouts - Hit and Run - Pursuit authorized? " + PursuitAuthorized);
                            break;
                    }

                    DialogueStep++;
                    GameFiber.Wait(200); // small debounce
                }
            }

            //Check if the Player is close enough to start the pursuit
            if (GatheredInfo && PursuitAuthorized && Game.LocalPlayer.Character.Position.DistanceTo(Suspect) <= 10f)
            {
                // Safeguard to not re-trigger
                GatheredInfo = false;
                TrafficStopAuthorized = false;
                InPulloverDialogue = false;

                //Create the pursuit
                Game.LogTrivial("CampusCallouts - Entering pursuit logic");
                Suspect.Tasks.Clear(); // before adding to pursuit
                Pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(Pursuit, Suspect);
                LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                LSPD_First_Response.Mod.API.Functions.SetPursuitCopsCanJoin(Pursuit, true);
                Game.DisplayNotification("The suspect is driving away!");
            }

            //Check if the Player is close enough to interrogate the suspect
            if (GatheredInfo && !PursuitAuthorized && !TrafficStopAuthorized && Game.LocalPlayer.Character.Position.DistanceTo(Suspect) <= 10f)
            {
                // Notify to initiate Traffic Stop
                Game.DisplayNotification("Suspect is cooperative, initiate a traffic stop and speak with them.");
                Game.DisplayHelp("Press ~y~" + Settings.DialogueKey + "~w~ to speak with the driver.");
                Game.LogTrivial("CampusCallouts - Traffic stop triggered");
                TrafficStopAuthorized = true;
            }

            //Check if the Suspect's Car is under a traffic stop and play dialogue
            if (LSPD_First_Response.Mod.API.Functions.IsPlayerPerformingPullover() && TrafficStopAuthorized)
            {
                PulloverHandle = LSPD_First_Response.Mod.API.Functions.GetCurrentPullover();

                if (!PursuitDialoguePlayed && PulloverHandle != null && LSPD_First_Response.Mod.API.Functions.GetPulloverSuspect(PulloverHandle) == Suspect && Game.LocalPlayer.Character.Position.DistanceTo(Suspect) <= 3f)
                {
                    InPulloverDialogue = true;
                }

                if (InPulloverDialogue && Game.IsKeyDown(Settings.DialogueKey))
                {
                    switch (PulloverDialogueStep)
                    {
                        case 0:
                            Game.DisplaySubtitle("~g~You: ~w~Good evening. Do you know why I stopped you?");
                            break;
                        case 1:
                            Game.DisplaySubtitle("~r~Suspect: ~w~...This about that pedestrian I almost hit?");
                            break;
                        case 2:
                            Game.DisplaySubtitle("~g~You: ~w~You didn’t *almost* hit them. You made contact and then fled the scene.");
                            break;
                        case 3:
                            Game.DisplaySubtitle("~r~Suspect: ~w~I didn’t even realize I actually hit anyone. I was scared and just kept driving...");
                            break;
                        case 4:
                            Game.DisplaySubtitle("~g~You: ~w~That’s not how this works. Step out of the car for me.");
                            Game.DisplayNotification("The suspect appears nervous but compliant. Proceed accordingly.");
                            PursuitDialoguePlayed = true;
                            InPulloverDialogue = false;
                            break;


                    }

                    PulloverDialogueStep++;
                    GameFiber.Wait(200);
                }
            }


            //Check conditions to end callout
            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Suspect) || Game.IsKeyDown(Settings.EndCallout) || Ped.IsDead)
            {
                End();
            }

            if (!Suspect.Exists() || !SuspectCar.Exists())
            {
                End();
                Game.LogTrivial("CampusCallouts - Hit and Run - Suspect or Car does not exist, ending callout.");
                return;
            }


        }

        public override void End()
        {
            base.End();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (SuspectCar.Exists()) { SuspectCar.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            Game.LogTrivial("CampusCallouts - Hit and Run cleaned up.");
        }
    }
}
