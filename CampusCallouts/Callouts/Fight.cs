﻿using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Engine;
using CalloutInterfaceAPI;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Students Fighting", CalloutProbability.Medium, "Two students are reportedly fighting on campus.", "Code 3", "ULSAPD")]
    public class StudentsFighting : Callout
    {
        //Private References
        private Vector3 PedSpawn;
        private float PedHeading;
        private Blip PedBlip;
        private Ped Ped;
        private Blip PedBlip2;
        private Ped Ped2;
        private bool OnScene = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            PedSpawn = new Vector3(-1649.166f, 224.3113f, 60.68501f);
            PedHeading = 22.75008f;

            //Set callout position
            CalloutPosition = PedSpawn;

            // LSPDFR
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);

            if (Settings.UseBluelineAudio)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_2_40_01", CalloutPosition);
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_ASSAULT_01 IN_OR_ON_POSITION", CalloutPosition);
            }

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_03_02");

            //Create Callout message
            CalloutMessage = "Students Fighting Reported"; // This will be displayed in the callout log. I prefer to have reported after to make it more realistic //

            //Last Line
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            //Create Peds
            Ped = new Ped(PedSpawn, PedHeading);
            Ped2 = new Ped(PedSpawn + new Vector3(1f, 0f, 0f), PedHeading);
            Ped.MakePersistent();
            Ped2.MakePersistent();
            Ped.BlockPermanentEvents = true;
            Ped2.BlockPermanentEvents = true;
            Game.LogTrivial("Peds created");

            //Create Blip
            PedBlip = Ped.AttachBlip();
            PedBlip.Color = Color.Orange;
            PedBlip2 = Ped2.AttachBlip();
            PedBlip2.Color = Color.Yellow;

            //Draw Route
            PedBlip.EnableRoute(Color.Orange);

            //Draw Help
            Game.DisplayHelp("There are reports of students fighting. Please respond and handle the situation.");

            //Last Line
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            //First Line
            base.OnCalloutNotAccepted();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (Ped2.Exists()) { Ped2.Dismiss(); }
            if (PedBlip2.Exists()) { PedBlip2.Delete(); }
        }

        public override void Process()
        {
            //First Line
            base.Process();

            if (!OnScene && Ped && Ped.Exists() && Ped2 && Ped2.Exists() && Game.LocalPlayer.Character.Position.DistanceTo(Ped) <= 15f)
            {
                OnScene = true;
                Ped.Tasks.FightAgainst(Ped2, -1);
                Ped2.Tasks.FightAgainst(Ped, -1);
                CalloutInterfaceAPI.Functions.SendMessage(this, "Two students are actively fighting.\nSeparate and detain both individuals if necessary.");
                PedBlip.DisableRoute();
                Game.DisplayHelp("Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Ped) || Ped.IsDead)
            {
                    this.End();

            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Ped2) || Ped2.IsDead)
            {

                    this.End();


            }

            if (Game.IsKeyDown(Settings.EndCallout))
            {
                    this.End();

            }
        }

        public override void End()
        {
            //First Line
            base.End();
            if (Ped.Exists()) { Ped.Dismiss(); }
            if (PedBlip.Exists()) { PedBlip.Delete(); }
            if (Ped2.Exists()) { Ped2.Dismiss(); }
            if (PedBlip2.Exists()) { PedBlip2.Delete(); }
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            CalloutInterfaceAPI.Functions.SendMessage(this, "Situation handled. Both individuals are no longer a threat. Code 4.");
        }
    }
}

