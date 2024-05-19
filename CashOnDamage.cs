using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;

namespace CashOnDamage
{
    public class CashOnDamage : BasePlugin
    {
        public override string ModuleName => "Cash On Damage";
        public override string ModuleAuthor => "Oylsister";
        public override string ModuleDescription => "Simple plugin giving a cash to a player when they do damage.";
        public override string ModuleVersion => "1.0";

        public FakeConVar<int> CVAR_AllowTeam = new("css_cashdamage_allowteam", "Specify which team could earn cash on damage [0 = T, 1 = CT, 2 = All]", 1, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 2));
        public FakeConVar<bool> CVAR_Enabled = new("css_cashdamage_enable", "Enable Cash On Damage or not", true);

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
        }

        private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            var client = @event.Userid;
            var attacker = @event.Attacker;
            var damage = @event.DmgHealth;

            if (!CVAR_Enabled.Value)
                return HookResult.Continue;

            if (client == null || attacker == null)
                return HookResult.Continue;

            AddCash(attacker, damage);

            return HookResult.Continue;
        }

        private void AddCash(CCSPlayerController client, int damage)
        {
            var team = CVAR_AllowTeam.Value;
            var client_team = client.Team;

            if (team == 0 && client_team != CsTeam.Terrorist)
                return;

            else if (team == 1 && client_team != CsTeam.CounterTerrorist)
                return;

            var money = client.InGameMoneyServices!.Account;
            client.InGameMoneyServices.Account = money + damage;
            Utilities.SetStateChanged(client, "CCSPlayerController", "m_pInGameMoneyServices");
        }
    }
}
