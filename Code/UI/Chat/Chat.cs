using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralGame.UI
{
	public record ChatEntry( string author, string message, RealTimeSince timeSinceAdded );
}
