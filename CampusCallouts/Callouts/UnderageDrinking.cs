using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Engine.Scripting.Entities;
using System;
using System.Collections.Generic;
using CalloutInterfaceAPI;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Underage Drinking", CalloutProbability.Low, "A report of a student potentially drinking underage at a party.", "Code 2", "ULSAPD")]
    public class UnderageDrinking : Callout
    {
        //Private References
        private Vector3 PedSpawn;
        private float PedHeading;
        private Blip PedBlip;
        private Ped Ped;
        private bool OnScene = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            PedSpawn = new Vector3(-1686.5f, 372.4233f, 85.11894f);
            PedHeading = 175.6301f;

            //Set callout position
            this.CalloutPosition = PedSpawn;

            //Create Callout message
            CalloutMessage = "Underage Drinking";

            // LSPDFR
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_SUSPICIOUS_ACTIVITY_01 IN_OR_ON_POSITION", CalloutPosition);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("RESPOND_CODE_2");

            //Last Line
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            //Create Ped
            Ped = new Ped(PedSpawn, PedHeading);
            Ped.MakePersistent();

            //Set Ped Birthday
            DateTime PedBirthday = new DateTime(2005, 6, 15);

            //Set ped first name
            List<string> firstNames = new List<string>{"John", "Jane", "Michael", "Emily", "David", "Emma", "Daniel", "Olivia", "James", "Sophia"};
            Random firstNameRandom = new Random();
            int randomFirstName = firstNameRandom.Next(firstNames.Count);
            string FirstName = firstNames[randomFirstName];

            //Set ped last name
            List<string> lastNames = new List<string>{"Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Martinez", "Hernandez"};
            Random lastNameRandom = new Random();
            int randomLastName = lastNameRandom.Next(lastNames.Count);
            string LastName = lastNames[randomLastName];

            //Set ped persona
            Persona newPed = new Persona(FirstName, LastName, LSPD_First_Response.Gender.Random, PedBirthday);
            LSPD_First_Response.Mod.API.Functions.SetPersonaForPed(Ped, newPed);

            Ped.BlockPermanentEvents = true;
            Ped.Tasks.StandStill(-1);
            Ped.Tasks.PlayAnimation("amb@world_human_partying@male@partying_beer@base", "base", 1f, AnimationFlags.Loop);
            Game.LogTrivial("Ped created");

            //Create Blip
            PedBlip = Ped.AttachBlip();
            PedBlip.Sprite = BlipSprite.Friend;
            PedBlip.Color = Color.Blue;

            //Draw Route
            PedBlip.EnableRoute(Color.Blue);

            //Draw Help
            Game.DisplayHelp("Neighbors have reported underage drinking at a party. Make your way to the party and investigate.");

            //Last Line
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            //First Line
            base.OnCalloutNotAccepted();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
        }

        public override void Process()
        {
            //First Line
            base.Process();

            if (!OnScene & Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 15f)
            {
                OnScene = true;
                PedBlip.DisableRoute();
                Game.DisplayHelp("Investigate the area. Press the ~y~END~w~ key to end the call.");
                CalloutInterfaceAPI.Functions.SendMessage(this, "You have arrived at the party. Witness reportedly observed underage drinking.\nSpeak with the individual and take appropriate action.");
            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Ped))
            {
                GameFiber.Sleep(3000);
                this.End();
            }

            if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
            {
                GameFiber.Sleep(3000);
                this.End();
            }
        }

        public override void End()
        {
            //First Line
            base.End();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_ARE_CODE FOUR");
            CalloutInterfaceAPI.Functions.SendMessage(this, "The underage drinking incident has been resolved. No further action required. Code 4.");
        }
    }
}
