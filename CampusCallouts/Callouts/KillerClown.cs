using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Killer Clown Sighting", CalloutProbability.Medium, "911 Caller reports multiple individuals dressed as clowns behaving aggressively.", "Code 99", "ULSAPD")]
    public class KillerClown : Callout
    {
        // Private References
        private List<Ped> Clowns = new List<Ped>();
        private List<Blip> ClownBlips = new List<Blip>();

        private Vector3 SpawnArea;

        private Random rand = new Random();

        private bool CombatStarted = false;
        private bool OnScene = false;
        private HashSet<Ped> ClownsInCombat = new HashSet<Ped>();


        private RelationshipGroup ClownGroup;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnArea = new Vector3(-1649.038f, 215.1764f, 60.64111f); // Clown General Spawn Area

            CalloutPosition = SpawnArea;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 40f);
            AddMinimumDistanceCheck(30f, CalloutPosition);

            CalloutMessage = "Killer Clown Sighting";
            CalloutAdvisory = "911 Caller reports multiple individuals dressed as clowns behaving aggressively.";

            if (Settings.UseBluelineAudio)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_2_45_01", CalloutPosition);
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CC_WE_HAVE CC_CRIME_CRIMINAL_ACTIVITY_05 IN_OR_ON_POSITION", CalloutPosition);
            }

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_99_03");

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            int clownCount = rand.Next(4, 8);

            ClownGroup = new RelationshipGroup("KILLERCLOWNS");
            ClownGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);
            ClownGroup.SetRelationshipWith(Game.LocalPlayer.Character.RelationshipGroup, Relationship.Hate);

            Ped player = Game.LocalPlayer.Character;

            for (int i = 0; i < clownCount; i++)
            {
                Vector3 pos = SpawnArea.Around2D(5f);
                Ped clown = new Ped("S_M_Y_Clown_01", pos, rand.Next(0, 360));
                clown.MakePersistent();
                clown.BlockPermanentEvents = true;
                clown.KeepTasks = true;

                //Gives some clowns a weapon
                string[] meleeWeapons = { "WEAPON_BAT", "WEAPON_KNIFE", "WEAPON_CROWBAR" };
                if (i % 2 == 0)
                {
                    string weapon = meleeWeapons[rand.Next(meleeWeapons.Length)];
                    clown.Inventory.GiveNewWeapon(weapon, -1, true);
                }


                clown.RelationshipGroup = ClownGroup;


                // Hate everyone nearby (except player and themselves)
                Ped[] allPeds = World.GetAllPeds();
                for (int j = 0; j < 25; j++)
                {
                    try
                    {
                        if (allPeds[j].Exists() && allPeds[j] != player && allPeds[j] != clown)
                        {
                            if (allPeds[j].Model.Name == "S_M_Y_Clown_01") continue; // skip other clowns
                            if (allPeds[j].RelationshipGroup != ClownGroup)
                            {
                                clown.RelationshipGroup.SetRelationshipWith(allPeds[j].RelationshipGroup, Relationship.Hate);
                            }
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Game.LogTrivial("CampusCallouts - KillerClown - Index out of bounds on ped scan.");
                        break;
                    }
                    GameFiber.Yield(); // prevents freezing
                }

                Blip blip = clown.AttachBlip();
                blip.Color = Color.Red;

                Clowns.Add(clown);
                ClownBlips.Add(blip);

                Game.LogTrivial($"CampusCallouts - KillerClown - Clown {i} spawned at {pos}.");

                if (i == 0) { ClownBlips[0].EnableRoute(Color.Red); }
            }

            if (Main.CalloutInterface)
            {
                CalloutInterfaceAPI.Functions.SendMessage(this, "Multiple clown suspects have been reported engaging in hostile behavior. Proceed Code 99.");
            }

            Game.DisplayHelp("Multiple individuals dressed as clowns are acting aggressively on campus. Proceed with caution.");
            Game.LogTrivial("CampusCallouts - Killer Clown - Clowns spawned and hostile.");

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            Cleanup();
        }

        public override void Process()
        {
            base.Process();

            // Start clown attacks when player is close enough
            if (!CombatStarted && Clowns.Count > 0 && Clowns[0] != null && Clowns[0].Exists() && Game.LocalPlayer.Character.Position.DistanceTo(Clowns[0]) <= 100f)
            {
                Game.LogTrivial("CampusCallouts - KillerClown - Player is within 100f. Clowns begin hostile behavior.");
                CombatStarted = true;
            }

            // Actively make clowns scan and attack closest targets repeatedly
            if (CombatStarted)
            {
                foreach (Ped clown in Clowns)
                {
                    if (clown.Exists() && !clown.IsDead && !ClownsInCombat.Contains(clown))
                    {
                        ClownsInCombat.Add(clown); // ensure only one fiber per clown
                        GameFiber.StartNew(() =>
                        {
                            AssignRelationshipsToNearbyPeds(clown);
                            GameFiber.Sleep(200); // short delay before they react
                            if (clown.Exists() && !clown.IsDead)
                            {
                                clown.Tasks.FightAgainstClosestHatedTarget(30f);
                            }
                        });
                    }
                }
            }


            if (AllClownsNeutralized() || Game.IsKeyDown(Settings.EndCallout))
            {
                End();
            }

            if (!OnScene && Game.LocalPlayer.Character.Position.DistanceTo(Clowns[0]) <= 10f)
            {
                OnScene = true;
                ClownBlips[0].DisableRoute();
                Game.DisplayHelp("Press " + Settings.EndCallout + "~w~ to end the call.");
            }
        }

        private bool AllClownsNeutralized()
        {
            foreach (Ped p in Clowns)
            {
                if (p.Exists() && !p.IsDead && !LSPD_First_Response.Mod.API.Functions.IsPedArrested(p))
                    return false;
            }
            return true;
        }

        private void AssignRelationshipsToNearbyPeds(Ped clown)
        {
            Ped[] allPeds = World.GetAllPeds();
            Ped player = Game.LocalPlayer.Character;

            for (int j = 0; j < 25; j++)
            {
                try
                {
                    if (allPeds[j].Exists() && allPeds[j] != player && allPeds[j] != clown)
                    {
                        if (allPeds[j].RelationshipGroup != ClownGroup)
                        {
                            clown.RelationshipGroup.SetRelationshipWith(allPeds[j].RelationshipGroup, Relationship.Hate);
                        }
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Game.LogTrivial("CampusCallouts - KillerClown - Index out of bounds on ped scan.");
                    break;
                }
                GameFiber.Yield(); // Prevents freezing
            }
        }


        public override void End()
        {
            base.End();
            Cleanup();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("GP_CODE4_02");
            Game.LogTrivial("CampusCallouts - Killer Clown - Callout cleaned up.");
        }

        private void Cleanup()
        {
            foreach (Ped p in Clowns)
                if (p.Exists()) p.Dismiss();

            foreach (Blip b in ClownBlips)
                if (b.Exists()) b.Delete();

            ClownsInCombat.Clear();
        }
    }
}
