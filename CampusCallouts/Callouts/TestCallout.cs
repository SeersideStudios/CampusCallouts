using LSPD_First_Response.Mod.Callouts;
using CalloutInterfaceAPI;
using Rage;

namespace CampusCallouts.Callouts
{
    [CalloutInterface("TestCallout", CalloutProbability.Low, "A test callout for debugging.", "Code 1", "Test Department")]
    public class TestCallout : Callout
    {
        public override bool OnBeforeCalloutDisplayed()
        {
            this.CalloutPosition = Game.LocalPlayer.Character.Position.Around(30f);
            ShowCalloutAreaBlipBeforeAccepting(this.CalloutPosition, 30f);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Game.DisplayNotification("TestCallout loaded successfully.");
            return base.OnCalloutAccepted();
        }
    }
}
