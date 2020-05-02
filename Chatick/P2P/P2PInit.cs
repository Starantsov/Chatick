using System.Net.PeerToPeer;

namespace Chatick
{
	public class P2PInit
	{
		public PeerName PeerName { get; set; }
		public IP2PService ServiceProxy { get; set; }
		public string DisplayString { get; set; }
		public bool ButtonsEnabled { get; set; }
	}
}
