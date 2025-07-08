using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("[CC] Killer Clown Sighting", CalloutProbability.Medium, "911 Caller reports multiple individuals dressed as clowns behaving aggressively.", "Code 3", "ULSAPD")]
    public class KillerClown : Callout
    {
        private List<Ped> Clowns = new List<Ped>();
        private List<Blip> ClownBlips = new List<Blip>();
        private Vector3 SpawnArea;
        private Random rand = new Random();

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnArea = new Vector3(-1649.038f, 215.1764f, 60.64111f); // Clown General Spawn Area

            CalloutPosition = SpawnArea;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 40f);
            AddMinimumDistanceCheck(30f, CalloutPosition);

            CalloutMessage = "Killer Clown Sighting";
            CalloutAdvisory = "911 Caller reports multiple individuals dressed as clowns behaving aggressively.";
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_MULTIPLE_INJURIES_01 IN_OR_ON_POSITION", CalloutPosition);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("RESPOND_CODE_99");

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            int clownCount = rand.Next(4, 8);

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


                clown.RelationshipGroup = RelationshipGroup.Gang1;
                clown.RelationshipGroup.SetRelationshipWith(player.RelationshipGroup, Relationship.Hate);
                clown.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

                // Hate everyone nearby (except player and themselves)
                Ped[] allPeds = World.GetAllPeds();
                for (int j = 0; j < 25; j++)
                {
                    try
                    {
                        if (allPeds[j].Exists() && allPeds[j] != player && allPeds[j] != clown)
                        {
                            clown.RelationshipGroup.SetRelationshipWith(allPeds[j].RelationshipGroup, Relationship.Hate);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Game.LogTrivial("CampusCallouts - KillerClown - Index out of bounds on ped scan.");
                        break;
                    }
                    GameFiber.Yield(); // prevents freezing
                }

                // Attack whoever they hate
                clown.Tasks.FightAgainstClosestHatedTarget(100f);

                Blip blip = clown.AttachBlip();
                blip.Color = Color.Red;

                Clowns.Add(clown);
                ClownBlips.Add(blip);
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

            if (AllClownsNeutralized() || Game.IsKeyDown(Settings.EndCallout))
            {
                End();
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

        public override void End()
        {
            base.End();
            Cleanup();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_ARE_CODE FOUR");
            Game.LogTrivial("CampusCallouts - Killer Clown - Callout cleaned up.");
        }

        private void Cleanup()
        {
            foreach (Ped p in Clowns)
                if (p.Exists()) p.Dismiss();

            foreach (Blip b in ClownBlips)
                if (b.Exists()) b.Delete();
        }
    }
}
