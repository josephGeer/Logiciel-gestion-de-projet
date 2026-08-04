using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private ObservableCollection<ProjectTask> _tasks;
        public ObservableCollection<ProjectTask> Tasks
        {
            get => _tasks;
            set
            {
                if (_tasks != null)
                    _tasks.CollectionChanged -= Tasks_CollectionChanged;

                _tasks = value ?? new ObservableCollection<ProjectTask>();
                _tasks.CollectionChanged += Tasks_CollectionChanged;

                RebuildTaskViews(); 
            }
        }

        [JsonIgnore]
        public ObservableCollection<ProjectTask> ActiveTasks { get; } = new();
        [JsonIgnore]
        public ObservableCollection<ProjectTask> CompletedTasks { get; } = new();

        public void NotifyProgressChanged()
        {
            OnPropertyChanged(nameof(ProgressPercentage));
        }

        public Projet(string Name, string Description, DateTime Deadline)
        {
            this.Name = Name;
            this.Description = Description;
            this.Deadline = Deadline;
            Items = new ObservableCollection<Item>();
            Tasks = new ObservableCollection<ProjectTask>();
        }

        public Projet()
        {
            Name = "basique";
            Description = "basique";
            Deadline = DateTime.MinValue;
            Items = new ObservableCollection<Item>();
            Tasks = new ObservableCollection<ProjectTask>();
        }

        // Notre projet s'est abonné à tasks.CollectionChanged sur la liste de Tasks, ainsi lorsque une tache est ajouté ou supprimé, cela va appelé la fonction ci dessous
        private void Tasks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //Pour chaque tache supprimé, on se désabonne de l'event Task_PropertyChanged
            if (e.OldItems != null)
                foreach (ProjectTask t in e.OldItems)
                {
                    t.PropertyChanged -= Task_PropertyChanged;
                    ActiveTasks.Remove(t);
                    CompletedTasks.Remove(t);
                }
            //Pour chaque tache ajouté, on s'abonne à l'event Task_PropertyChanged, si une propriété de la tache change, alors Task_PropertyChanged est appellé
            // Donc si t.IsComplete change, la fonctoin est appelé et on peux réalisé le traitement que l'on veut, ajouté dans la liste active ou retiré de la liste active et ajouté à completedTask
            if (e.NewItems != null)
                foreach (ProjectTask t in e.NewItems)
                {
                    t.PropertyChanged += Task_PropertyChanged;
                    (t.IsComplete ? CompletedTasks : ActiveTasks).Add(t);
                }

            OnPropertyChanged(nameof(ProgressPercentage));
        }

        private void Task_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ProjectTask.IsComplete)) return;
            var t = (ProjectTask)sender;

            if (t.IsComplete)
            {
                ActiveTasks.Remove(t);
                if (!CompletedTasks.Contains(t)) CompletedTasks.Add(t);
            }
            else
            {
                CompletedTasks.Remove(t);
                if (!ActiveTasks.Contains(t)) ActiveTasks.Add(t);
            }

            OnPropertyChanged(nameof(ProgressPercentage));
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
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string dossier = Path.Combine(localAppData, "MonAppTest", "BDD", "Projet");

                if (!Directory.Exists(dossier))
                {
                    Directory.CreateDirectory(dossier);
                }

                string fileName = projet.Name + ".json";
                string cheminComplet = Path.Combine(dossier, fileName);

                await using (FileStream createStream = File.Create(cheminComplet))
                {
                    await JsonSerializer.SerializeAsync(createStream, projet);
                }

                Debug.WriteLine(File.ReadAllText(cheminComplet));
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERREUR LORS DE LA CRÉATION : " + e.Message);
            }
        }

        public double ProgressPercentage
        {
            get
            {
                int total = Tasks?.Count ?? 0;
                if (total == 0) return 0.0;
                return CompletedTasks.Count * 100.0 / total;
            }
        }

        public void RebuildTaskViews()
        {
            ActiveTasks.Clear();
            CompletedTasks.Clear();

            foreach (var t in Tasks)
            {
                t.PropertyChanged -= Task_PropertyChanged;
                t.PropertyChanged += Task_PropertyChanged;
                (t.IsComplete ? CompletedTasks : ActiveTasks).Add(t);
            }
        }

    }
}
