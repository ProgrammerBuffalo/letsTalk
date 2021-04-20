using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letsTalk
{
    public class ConnectedUser
    {
        public int SqlID { get; set; }

        public string Name { get; set; }

        public IChatCallback ChatCallback { get; set; }
    }
}
