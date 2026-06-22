using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app_test.Items
{
    public class Audio : Item
    {
        public Audio(int position, string source) : base(position, source)
        {

        }

        public Audio() : base()
        {
        }
    }
}
