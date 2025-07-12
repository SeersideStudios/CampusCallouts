using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Drawing;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Active Shooter on Campus", CalloutProbability.High, "Reports of an active shooter on the ULSA campus.", "Code 99", "ULSAPD")]
    public class SchoolShooter : Callout
    {
        private Ped Shooter;

        private Blip ShooterBlip;

        private Vector3 SpawnPoint;

        private bool CombatStarted = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(-1650f, 210f, 60.6f);

            CalloutPosition = SpawnPoint;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 40f);
            AddMinimumDistanceCheck(30f, CalloutPosition);

            CalloutMessage = "Active Shooter on Campus";
            CalloutAdvisory = "911 Caller reports gunfire on the ULSA campus.";

            if (Settings.UseBluelineAudio)
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_SHOTS_FIRED_02", CalloutPosition);
            else
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_A_WEAPONS_INCIDENT_SHOTS_FIRED IN_OR_ON_POSITION", CalloutPosition);

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_99_03");

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Spawn Shooter
            Shooter = new Ped("S_M_Y_Marine_01", SpawnPoint, 180f);
            Shooter.MakePersistent();
            Shooter.BlockPermanentEvents = true;
            Shooter.KeepTasks = true;

            // Give Weapon
            Shooter.Inventory.GiveNewWeapon("WEAPON_CARBINERIFLE", 500, true);

            // Make hostile
            Shooter.RelationshipGroup = new RelationshipGroup("SHOOTER");
            Shooter.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);
            Shooter.RelationshipGroup.SetRelationshipWith(Game.LocalPlayer.Character.RelationshipGroup, Relationship.Hate);

            // Hate nearby civilians
            Ped[] allPeds = World.GetAllPeds();
            for (int i = 0; i < 25; i++)
            {
                try
                {
                    Ped civ = allPeds[i];
                    if (!civ.Exists() || civ == Shooter || civ == Game.LocalPlayer.Character || !civ.IsHuman || civ.IsPlayer || civ.RelationshipGroup == RelationshipGroup.Cop)
                        continue;

                    // Mutual hate
                    Shooter.RelationshipGroup.SetRelationshipWith(civ.RelationshipGroup, Relationship.Hate);
                    civ.RelationshipGroup.SetRelationshipWith(Shooter.RelationshipGroup, Relationship.Hate);

                    // Panic & flee
                    civ.Tasks.ReactAndFlee(Shooter);
                }
                catch
                {
                    break;
                }

                GameFiber.Yield(); // helps prevent freezing on large ped scans
            }

            // Blip
            ShooterBlip = Shooter.AttachBlip();
            ShooterBlip.Color = Color.Red;
            ShooterBlip.EnableRoute(Color.Red);

            Game.DisplayHelp("An armed suspect is reported to be opening fire on campus. Proceed Code 99.");
            Game.LogTrivial("CampusCallouts - SchoolShooter - Shooter spawned and armed.");

            if (Main.CalloutInterface)
            {
                CalloutInterfaceAPI.Functions.SendMessage(this, "Active shooter has been reported at ULSA. Officers responding Code 99.");
            }

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!CombatStarted && Game.LocalPlayer.Character.Position.DistanceTo(Shooter) <= 100f)
            {
                CombatStarted = true;
                Shooter.Tasks.FightAgainstClosestHatedTarget(50f); // medium range, allows chasing but not across map
                Game.LogTrivial("CampusCallouts - SchoolShooter - Shooter has engaged.");
                ShooterBlip.DisableRoute();
                Game.DisplayHelp("Press " + Settings.EndCallout + "~w~ to end the call.");
            }

            if (!Shooter.Exists() || Shooter.IsDead || LSPD_First_Response.Mod.API.Functions.IsPedArrested(Shooter) || Game.IsKeyDown(Settings.EndCallout))
            {
                End();
            }
        }

        public override void End()
        {
            base.End();
            if (Shooter.Exists()) Shooter.Dismiss();
            if (ShooterBlip.Exists()) ShooterBlip.Delete();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - SchoolShooter - Callout cleaned up.");
        }
    }
}