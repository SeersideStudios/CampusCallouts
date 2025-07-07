using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Drawing;
using System.Xml.Linq;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Noise Complaint", CalloutProbability.Medium, "Neighbors report loud party activity near the dorm driveways.", "Code 1", "ULSAPD")]
    public class NoiseComplaint : Callout
    {
        //Private References
        private Vector3 DrivewayLocation;

        private Vector3 PedSpawn;
        private Vector3 PedSpawn2;
        private Vector3 PedSpawn3;
        private Vector3 PedSpawn4;
        private Vector3 PedSpawn5;
        private float PedHeading;
        private float PedHeading2;
        private float PedHeading3;
        private float PedHeading4;
        private float PedHeading5;

        private Ped Ped1;
        private Ped Ped2;
        private Ped Ped3;
        private Ped Ped4;
        private Ped Ped5;

        private Blip DrivewayBlip;

        private bool OnScene = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            DrivewayLocation = new Vector3(-1750.335f, 365.4604f, 89.23333f);

            PedSpawn = new Vector3(-1721.035f, 366.1222f, 89.77831f);
            PedHeading = 275.8509f;

            PedSpawn2 = new Vector3(-1718.091f, 369.0178f, 89.77727f);
            PedHeading2 = 227.2813f;

            PedSpawn3 = new Vector3(-1715.616f, 369.4782f, 89.77764f);
            PedHeading3 = 121.8256f;

            PedSpawn4 = new Vector3(-1718.054f, 367.1187f, 89.7297f);
            PedHeading4 = 74.67124f;

            PedSpawn5 = new Vector3(-1724.905f, 368.9243f, 89.78442f);
            PedHeading5 = 254.4968f;

            //Set callout position
            this.CalloutPosition = PedSpawn;

            //LSPDFR
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            AddMinimumDistanceCheck(20f, CalloutPosition);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_CIVIL_DISTURBANCE_01 IN_OR_ON_POSITION", CalloutPosition);

            //Create Callout message
            CalloutMessage = "Noise Complaint";

            //Last Line
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            //Create Peds
            Ped1 = new Ped(PedSpawn, PedHeading);
            Ped1.MakePersistent();
            Ped1.BlockPermanentEvents = true;
            Ped1.Tasks.StandStill(-1);
            Ped1.Tasks.PlayAnimation("amb@world_human_partying@male@partying_beer@base", "base", 1f, AnimationFlags.Loop);

            Ped2 = new Ped(PedSpawn2, PedHeading2);
            Ped2.MakePersistent();
            Ped2.BlockPermanentEvents = true;
            Ped2.Tasks.StandStill(-1);
            Ped2.Tasks.PlayAnimation("amb@world_human_partying@male@partying_beer@base", "base", 1f, AnimationFlags.Loop);

            Ped3 = new Ped(PedSpawn3, PedHeading3);
            Ped3.MakePersistent();
            Ped3.BlockPermanentEvents = true;
            Ped3.Tasks.StandStill(-1);
            Ped3.Tasks.PlayAnimation("amb@world_human_partying@male@partying_beer@base", "base", 1f, AnimationFlags.Loop);

            Ped4 = new Ped(PedSpawn4, PedHeading4);
            Ped4.MakePersistent();
            Ped4.BlockPermanentEvents = true;
            Ped4.Tasks.StandStill(-1);
            Ped4.Tasks.PlayAnimation("amb@world_human_partying@male@partying_beer@base", "base", 1f, AnimationFlags.Loop);

            Ped5 = new Ped(PedSpawn5, PedHeading5);
            Ped5.MakePersistent();
            Ped5.BlockPermanentEvents = true;
            Ped5.Tasks.StandStill(-1);
            Ped5.Tasks.PlayAnimation("amb@world_human_partying@male@partying_beer@base", "base", 1f, AnimationFlags.Loop);

            Game.LogTrivial("Peds created");

            //Create Blip
            DrivewayBlip = new Blip(DrivewayLocation);
            DrivewayBlip.Color = Color.Blue;

            //Draw Route
            DrivewayBlip.EnableRoute(Color.Blue);

            //Draw Help
            Game.DisplayHelp("Neighbors have reported a noise complaint. Make your way to the area and investigate.");

            //Last Line
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            //First Line
            base.OnCalloutNotAccepted();
            if (Ped1.Exists()) { Ped1.Dismiss(); }
            if (DrivewayBlip.Exists()) { DrivewayBlip.Delete(); }
        }

        public override void Process()
        {
            //First Line
            base.Process();

            if (!OnScene & Game.LocalPlayer.Character.Position.DistanceTo(DrivewayLocation) <= 10f)
            {
                OnScene = true;
                DrivewayBlip.DisableRoute();
                CalloutInterfaceAPI.Functions.SendMessage(this, "You have arrived at the reported location.\nInvestigate the group and speak to individuals.");
                Game.DisplayHelp("Investigate the area, (StopThePed reccomended). Press ~y~" + Settings.EndCallout + "~w~ to end the call.");
                Game.DisplayNotification("~y~[INFO]~w~ Be sure to check ID's of anyone drinking.");
            }
            
            if (Game.IsKeyDown(Settings.EndCallout))
            {
                GameFiber.Sleep(3000);
                this.End();
            }
        }

        public override void End()
        {
            //First Line
            base.End();
            if (Ped1.Exists()) { Ped1.Dismiss(); }
            if (DrivewayBlip.Exists()) { DrivewayBlip.Delete(); }
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_ARE_CODE FOUR");
            CalloutInterfaceAPI.Functions.SendMessage(this, "Noise complaint resolved. Units are Code 4.");
        }
    }
}
