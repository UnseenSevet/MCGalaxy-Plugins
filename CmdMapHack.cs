using System;
using MCGalaxy.Commands;
using MCGalaxy.Events.PlayerEvents;

namespace MCGalaxy {
	public class CmdMaphack : Command2 {
		public override string name { get { return "MapHack"; } }
		public override string type { get { return "other"; } }
		public override CommandPerm[] ExtraPerms {
			get { return new[] { new CommandPerm(LevelPermission.Operator, "can bypass hack restrictions on all maps") }; }
		}
		
		static bool hooked;
		const string ext_allowed_key = "__MAPHACK_ALLOWED";
		
		public override void Use(Player p, string message, CommandData data) {
			if (!hooked) { // not thread-safe but meh
				OnSentMapEvent.Register(HandleOnSentMap, Priority.High);
				OnGettingMotdEvent.Register(HandleGettingMotd, Priority.High);
				hooked = true;
			}

			if (LevelInfo.IsRealmOwner(p.name, p.level.MapName) || CheckExtraPerm(p, data, 1)) {
				p.Extras[ext_allowed_key] = true;
				p.SendMapMotd();
				p.Message("&aYou are now bypassing hacks restrictions on this map");
			} else {
				p.Message("&cYou can only bypass hacks on your own realms.");
			}
		}
		
		void HandleOnSentMap(Player p, Level prevLevel, Level level) {
			if (!p.Extras.GetBoolean(ext_allowed_key)) return;
			// disable /maphack when you reload or change maps
			p.Extras[ext_allowed_key] = false;
			p.SendMapMotd();
			p.Message("%HHacks bypassing reset, use %T/MapHack %Hto turn on again");
		}
		
		void HandleGettingMotd(Player p, ref string motd) {
			if (!p.Extras.GetBoolean(ext_allowed_key)) return;
			motd = "+hax";
		}
		
		public override void Help(Player p) {
			p.Message("%T/MapHack");
			p.Message("%HLets you bypass hacks restrictions on your own map");
			p.Message("%H  (e.g. for when making a parkour map with -hax on)");
		}
	}
}
