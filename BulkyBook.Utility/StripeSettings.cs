using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
	public class StripeSettings
	{
        //same properties as in appsettings.json
        public string SecretKey { get; set; }
		public string PublishableKey { get; set; }
	}
}
