﻿using System.Drawing;
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

        private readonly List<Rage.Object> SpawnedProps = new List<Rage.Object>();

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

            if (Settings.UseBluelineAudio)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("POSSIBLE_DISTURBANCE", CalloutPosition);
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_POSSIBLE_DISTURBANCE IN_OR_ON_POSITION", CalloutPosition);
            }

            //Last Line
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            //Create Ped
            Ped = new Ped(PedSpawn, PedHeading);
            Ped.IsPersistent = true;

            //Set Ped Birthday
            DateTime PedBirthday = new DateTime(2005, 6, 15);

            //Set ped first name
            List<string> firstNames = new List<string> { "John", "Jane", "Michael", "Emily", "David", "Emma", "Daniel", "Olivia", "James", "Sophia" };
            Random firstNameRandom = new Random();
            int randomFirstName = firstNameRandom.Next(firstNames.Count);
            string FirstName = firstNames[randomFirstName];

            //Set ped last name
            List<string> lastNames = new List<string> { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Martinez", "Hernandez" };
            Random lastNameRandom = new Random();
            int randomLastName = lastNameRandom.Next(lastNames.Count);
            string LastName = lastNames[randomLastName];

            //Set ped persona
            Persona newPed = new Persona(FirstName, LastName, LSPD_First_Response.Gender.Random, PedBirthday);
            LSPD_First_Response.Mod.API.Functions.SetPersonaForPed(Ped, newPed);

            Ped.BlockPermanentEvents = true;
            Ped.Tasks.StandStill(-1);
            Ped.Tasks.PlayAnimation("amb@world_human_partying@male@partying_beer@base", "base", 1f, AnimationFlags.Loop);
            GiveBeerBottleToPed(Ped); // Give the beer bottle to the ped
            Game.LogTrivial("Ped created");

            //Create Blip
            PedBlip = Ped.AttachBlip();
            PedBlip.Color = Color.Blue;

            //Draw Route
            PedBlip.EnableRoute(Color.Blue);

            //Draw Help
            Game.DisplayHelp("Neighbors have reported underage drinking in the open. Make your way to the house and investigate.");

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
                Game.DisplayHelp("Use StopThePed for this callout. Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
                CalloutInterfaceAPI.Functions.SendMessage(this, "You have arrived at the party. Witness reportedly observed underage drinking.\nDeal with the situation how you feel fit.");
            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Ped))
            {
                GameFiber.Sleep(3000);
                this.End();
            }

            if (Game.IsKeyDown(Settings.EndCallout))
            {
                GameFiber.Sleep(3000);
                this.End();
            }
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

               

                beer.IsPersistent = true; // Make sure the beer bottle stays in the world
            }
            catch (Exception ex)
            {
                Game.LogTrivial("CampusCallouts: Failed to attach left-hand beer bottle: " + ex.Message);
            }
        }



        public override void End()
        {
            //First Line
            base.End();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            foreach (var obj in SpawnedProps)
                if (obj.Exists()) obj.Delete();
            SpawnedProps.Clear();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            CalloutInterfaceAPI.Functions.SendMessage(this, "The underage drinking incident has been resolved. No further action required. Code 4.");
        }
    }
}