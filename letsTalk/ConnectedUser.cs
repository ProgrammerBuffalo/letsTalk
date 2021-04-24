using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace letsTalk
{
    public class ConnectedUser
    {
        public int SqlID { get; set; }

        public string Name { get; set; }

        public OperationContext UserContext { get; set; }
    }
}
