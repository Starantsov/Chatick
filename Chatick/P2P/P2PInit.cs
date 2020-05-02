using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.PeerToPeer;

namespace Chatick
{
	class P2PInit
	{
		public PeerName PeerName { get; set; }
		public IP2PService ServiceProxy { get; set; }
		public string DisplayString { get; set; }
		public bool ButtonsEnabled { get; set; }
	}
}
