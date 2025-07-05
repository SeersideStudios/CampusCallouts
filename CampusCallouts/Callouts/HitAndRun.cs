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
        private bool DialoguePlayed = false;
        private bool TrafficStopAuthorized = false;

        private LHandle PulloverHandle;
        private LHandle Pursuit;


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
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_HIT_AND_RUN_01 IN_OR_ON_POSITION", CalloutPosition);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("RESPOND_CODE_2");


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
                Game.DisplayHelp("Press the ~y~END~w~ key to end the call at any time.");
            }

            // Interacting with ped
            if (!GatheredInfo && OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 3f)
            {
                //Get Vehicle Details
                string model = SuspectCar.Model.Name;
                string plate = SuspectCar.LicensePlate;

                Ped.Face(Game.LocalPlayer.Character);
                //Conversation
                Game.DisplaySubtitle("~b~Student: ~w~Officer! Thank God you're here!");
                GameFiber.Sleep(3500);

                Game.DisplaySubtitle("~g~You: ~w~Are you okay? What happened?");
                GameFiber.Sleep(3500);

                Game.DisplaySubtitle("~b~Student: ~w~I was crossing the street when a car came flying around the corner.");
                GameFiber.Sleep(3500);

                Game.DisplaySubtitle("~b~Student: ~w~It hit me and just kept going! Didn’t even slow down.");
                GameFiber.Sleep(3500);

                Game.DisplaySubtitle("~g~You: ~w~Did you get a good look at the vehicle?");
                GameFiber.Sleep(3500);

                Game.DisplaySubtitle("~b~Student: ~w~Yeah... it was an ~y~" + model + " ~w~with the plate ~y~" + plate + "~w~.");
                GameFiber.Sleep(4000);

                Game.DisplaySubtitle("~g~You: ~w~Alright, I’ll see if I can find them.");
                GameFiber.Sleep(3500);

                //Callout Interface
                if (Main.CalloutInterface) CalloutInterfaceAPI.Functions.SendMessage(this, "Student gave a vehicle description:\nModel: " + model + "\nPlate: " + plate);


                //Create the Blip for the suspect
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.Color = Color.Red;
                SuspectBlip.EnableRoute(Color.Red);

                Game.DisplayNotification("Find the vehicle, and interrogate the suspect.");
                PedBlip.Delete();

                GatheredInfo = true;

                Random randNum = new Random();
                PursuitAuthorized = randNum.Next(0, 2) == 0;

                Game.LogTrivial("CampusCallouts - Hit and Run - Pursuit authorized? " + PursuitAuthorized);
            }

            //Check if the Player is close enough to start the pursuit
            if (GatheredInfo && PursuitAuthorized && Game.LocalPlayer.Character.Position.DistanceTo(Suspect) <= 10f)
            {

                //Create the pursuit
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
                TrafficStopAuthorized = true;
            }

            //Check if the Suspect's Car is under a traffic stop and play dialogue
            if (LSPD_First_Response.Mod.API.Functions.IsPlayerPerformingPullover() && TrafficStopAuthorized)
            {
                //Checks who you've pulled over
                PulloverHandle = LSPD_First_Response.Mod.API.Functions.GetCurrentPullover();

                if (!DialoguePlayed && PulloverHandle != null && LSPD_First_Response.Mod.API.Functions.GetPulloverSuspect(PulloverHandle) == Suspect && Game.LocalPlayer.Character.Position.DistanceTo(Suspect) <= 3f)
                {
                    //Play the Dialogue
                    Game.DisplaySubtitle("~g~You: ~w~Mind rolling down that window? We need to talk.");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~r~Suspect: ~w~...This about what happened back by the crosswalk?");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~g~You: ~w~You hit someone and left the scene. Step out of the vehicle.");
                    GameFiber.Sleep(3500);

                    Game.DisplaySubtitle("~r~Suspect: ~w~I—I freaked out. I didn’t mean to just drive off...");
                    GameFiber.Sleep(3500);

                    DialoguePlayed = true;
                }
            }

            //Check conditions to end callout
            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Suspect) || Game.IsKeyDown(System.Windows.Forms.Keys.End))
            {
                End();
            }

        }

        public override void End()
        {
            base.End();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_ARE_CODE FOUR");
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (SuspectCar.Exists()) { SuspectCar.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            Game.LogTrivial("CampusCallouts - Hit and Run cleaned up.");
        }
    }
}
