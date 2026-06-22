using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace app_test.Items
{
    public class Texte : Item
    {
        private string _contenu = "";
        public string Contenu
        {
            get => _contenu;
            set
            {
                _contenu = value;
                OnPropertyChanged(nameof(Contenu));
            }
        }
        public Texte(int position, string source) : base(position, source) { }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Texte() : base()
        {
        }

    }
}
