using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace app_test.Items
{
    public class Video : Item
    {
        public Video(int position, string source) : base(position, source)
        {

        }

        public Video() : base()
        {
        }

    }
}
