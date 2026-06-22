using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Storage;

namespace app_test
{
    
    //le mot partial divise une classe en deux, 
    public class Projet : ObservableObject
    {
        //L'attribut [ObservableProperty] génère automatiquement une propriété publique avec getter/setter et le mécanisme de notification
        //(INotifyPropertyChanged) à partir d'un champ privé, afin d'alléger le code." 
        //Mais la classe doit être partial pour que ça fonctionne
        //[ObservableProperty]
        private string _name;
        public string Name { 
            get => _name; 
            set => SetProperty(ref _name, value); } // change la valeur en utilisant value -> _name et notifie le changement
        private string _description;
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        private DateTime _deadLine;
        public DateTime Deadline { get => _deadLine; set => SetProperty(ref _deadLine, value); }
         
        public ObservableCollection<Item> Items { get; set; }

        public Projet(string Name, string Description, DateTime Deadline)
        {
            this.Name = Name;
            this.Description = Description;
            this.Deadline = Deadline;
            Items = new ObservableCollection<Item>();

        }

        public Projet()
        {
            Name = "basique";
            Description = "basique";
            Deadline = DateTime.MinValue;
            Items = new ObservableCollection<Item>();
        }

        public override string ToString()
        {
            Debug.WriteLine("Je suis un projet avec pour nom : " + this.Name + this.Description + this.Deadline);
            return "Je suis un projet avec pour nom : " + this.Name + this.Description + this.Deadline;
        }
        public string TimeLeft()
        {
            DateTime Today = DateTime.Now;
            TimeSpan TimeLeft = Deadline - Today;
            return TimeLeft.Days.ToString();
        }

        //option JSON WriteIndented = true permet d'avoir l'écriture indenté plus compréhensible
        public static async Task CreateProjetFileJSON(Projet projet)
        {
            //Il va falloir à partir du nom du projet, créer le fichier JSON qui va contenir les informations du projet.
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                string dossier = Path.Combine(localFolder.Path, "BDD", "Projet");

                if (!Directory.Exists(dossier))
                {
                    Directory.CreateDirectory(dossier);
                }

                //création du nom du fichier + création du chemin complettion du chemin complet en concaténant avec le chemin du sossier
                string fileName = projet.Name + ".json";
                string cheminComplet = Path.Combine(dossier, fileName);

                //creation du fichier
                await using (FileStream createStream = File.Create(cheminComplet))
                {
                    await JsonSerializer.SerializeAsync(createStream, projet);
                }

                Debug.WriteLine(File.ReadAllText(cheminComplet));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
