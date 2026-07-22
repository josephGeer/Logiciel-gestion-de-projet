using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace app_test.Items
{
    public class Dessin : Item
    {
        public List<TraitDessin> Traits { get; set; } = new List<TraitDessin>();
        public Dessin(string source) : base(source)
        {
        }

        public Dessin() : base()
        {
        }

        public class TraitDessin
        {
            public List<Point> ListePoint { get; set; } = new List<Point>();
            public string Color { get; set; }
            public double Thickness { get; set; }
            public Boolean IsEraser { get; set; }

            public TraitDessin(List<Point> listePoint, Windows.UI.Color color, double thickness, Boolean isEraser)
            {
                this.ListePoint = listePoint;
                this.Color = color.ToString();
                this.Thickness = thickness;
                this.IsEraser = isEraser;
            }
            public TraitDessin()
            {
            }
        }

    }
}
