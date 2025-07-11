using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using System;
using CalloutInterfaceAPI;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Weapon Violation", CalloutProbability.Medium, "Reports of a person on campus possibly armed with a weapon.", "Code 99", "ULSAPD")]

    public class WeaponViolation : Callout
    {
        //Private References
        private Vector3 PedSpawn;
        private float PedHeading;

        private Blip PedBlip;

        private Ped Ped;

        private bool OnScene = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            PedSpawn = new Vector3(-1649.166f, 224.3113f, 60.68501f);
            PedHeading = 22.75008f;

            //Set callout position
            this.CalloutPosition = PedSpawn;

            // LSPDFR
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);

            if (Settings.UseBluelineAudio)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_SHOTS_FIRED_02", CalloutPosition);
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_ASSAULT_WITH_A_DEADLY_WEAPON_01 IN_OR_ON_POSITION", CalloutPosition);
            }

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_99_03");

            //Create Callout message
            CalloutMessage = "Reports of an individual with a weapon";
            
            //Last Line
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            //Create Peds
            Ped = new Ped(PedSpawn, PedHeading);
            Ped.MakePersistent();
            Ped.BlockPermanentEvents = true;
            Ped.Tasks.Wander();
            Game.LogTrivial("Ped created");

            //Create Blip
            PedBlip = Ped.AttachBlip();
            PedBlip.Color = Color.Orange;

            //Draw Route
            PedBlip.EnableRoute(Color.Orange);

            //Draw Help
            Game.DisplayHelp("There are reports of an individual on campus who has a firearm. Please respond and investigate.");

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
                Game.DisplayHelp("Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
                StartScenario();
            }

            if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(Ped))
            {
                this.End();
            }

            if (Ped.IsDead)
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
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - WeaponViolation callout cleaned up.");
        }

        public void StartScenario()
        {
            //Pick a random option to happen
            int result = new Random().Next(1, 3);

            if (result == 1)
            {
                //Result 1 will be the ped turns and fires at the officer
                Ped.Inventory.GiveNewWeapon(WeaponHash.APPistol, 999, true);
                Ped.Tasks.FireWeaponAt(Game.LocalPlayer.Character, -1, FiringPattern.BurstFirePistol);
                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspect brandished a firearm and opened fire on officers.");

            }
            else if (result == 2)
            {
                //Result 2 will be the ped reacts and flees, causing a pursuit
                Ped.Inventory.GiveNewWeapon(WeaponHash.APPistol, 999, true);
                Ped.Tasks.ReactAndFlee(Game.LocalPlayer.Character);
                LHandle Pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(Pursuit, Ped);
                LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                LSPD_First_Response.Mod.API.Functions.SetPursuitCopsCanJoin(Pursuit, true);
                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspect fled the scene. A foot pursuit has been initiated.");
            }
            else if (result == 3)
            {
                //Result 3 will be the ped has no weapon and it was a false call
                Ped.Inventory.Weapons.Clear();
                CalloutInterfaceAPI.Functions.SendMessage(this, "No weapon was found. Caller may have been mistaken.");
            }
            Game.LogTrivial("CampusCallouts - WeaponViolation - Ped scenario result: " + result);
        }
    }
}

