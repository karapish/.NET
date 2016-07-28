using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public partial class Portfolio
    {
        public void Update(XigniteQuoter quoter)
        {
            this.Positions.All(item => { item.Update(quoter); return true; });
        }
    }
}
