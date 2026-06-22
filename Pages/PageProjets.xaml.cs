using app_test;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace app_test.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageProjets : Page
    {
        public ObservableCollection<Projet> Projets { get; } =
            new ObservableCollection<Projet>();
        public PageProjets()
        {
            InitializeComponent();
            GetAllProjects();
        }

        private  Projet? DeserializedJSON(string ProjetJSON)
        {
            Projet? Projet = JsonSerializer.Deserialize<Projet>(ProjetJSON);
            return Projet;
        }

        private async Task GetAllProjects()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string dossier = Path.Combine(localFolder.Path, "BDD", "Projet");

            if (!Directory.Exists(dossier))
            {
                Debug.WriteLine("dossier existe pas");
                return;
            }

            //Le *.json sert a filtré pour n'avoir que les fichiers JSON
            string[] files = Directory.GetFiles(dossier, "*.json");
            foreach(string file in files)
            {
                Debug.WriteLine($"\n lecture du fichier : " + file);
                try
                {
                    string jsonContent = await System.IO.File.ReadAllTextAsync(file);
                    Projet? projet = DeserializedJSON(jsonContent);

                    if(projet != null)
                    {
                        Projets.Add(projet);
                    }
                }
                catch (Exception ex)
                {
                    // Si un fichier est bloqué ou illisible, on capture l'erreur ici pour ne pas planter la boucle
                    Console.WriteLine($"Erreur lors de la lecture de {file} : {ex.Message}");
                }
            }
        }

        public void ProjectClick(object sender, RoutedEventArgs e)
        {
            // afficher un panel, avec inputText, Nom, Description et Temps du projet (1 jours, 1 semaine ...)
            if(CreateProjectPanel.Visibility == Visibility.Collapsed)
            {
                CreateProjectPanel.Visibility = Visibility.Visible;
                ProjectGridView.Visibility = Visibility.Collapsed;
            }
            else
            {
                CreateProjectPanel.Visibility = Visibility.Collapsed;
                ProjectGridView.Visibility = Visibility.Visible;
            }

        }

        public async void CreateProject(object sender, RoutedEventArgs e)
        {
            string name = NameInput.Text;
            string description = DescriptionInput.Text;

            if (DeadlineButton.Tag != null)
            {
                string deadlineValue = DeadlineButton.Tag.ToString();
                int days = int.Parse(deadlineValue);
                DateTime Deadline = DateTime.Now.AddDays(days);
                Projet projet = new Projet(name, description, Deadline);

                await Projet.CreateProjetFileJSON(projet);

                if (CreateProjectPanel.Visibility == Visibility.Visible)
                {
                    CreateProjectPanel.Visibility = Visibility.Collapsed;
                    ProjectGridView.Visibility = Visibility.Visible;
                    GetAllProjects();
                }

            }
            else
            {
                Debug.WriteLine("Lecture de la Deadline ne fonctionne pas");
            }
            
        }

        public void OpenDeletePopUp(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("OpenDeletePopup button  called");

            var button = sender as Button;
            var stackPanel = button.Parent as StackPanel;
            var grid = stackPanel?.Parent as Grid;
            if (grid != null)
            {
                var popup = grid.FindName("DeletePopup") as Popup;
                if (popup != null && !popup.IsOpen)
                {
                    popup.IsOpen = true;
                }
            }
        }
        public void CloseDeletePopup(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("CloseDeletePopup called");
            var button = sender as Button;
            var grid = button?.Parent as Grid;
            var stackPanel = grid?.Parent as StackPanel;
            var popup = stackPanel?.Parent as Popup;

            if (popup != null && popup.IsOpen)
            {
                    popup.IsOpen = false;
            }
        }
        public void DeleteProject(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("DeleteProject button  called");
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string dossier = Path.Combine(localFolder.Path, "BDD", "Projet");

            var button = sender as Button;
            Projet projet = button.DataContext as Projet;
            Debug.WriteLine("to string du projet dans fonction deleteProject : " + projet);

            string fullPath = Path.Combine(dossier, projet.Name + ".json");
            Debug.WriteLine("Chemin utilisé pour supression du fichier : " + fullPath);

            System.IO.File.Delete(fullPath);
            Debug.WriteLine("Projet supprimé : " + projet.Name);
            Projets.Remove(projet);

        }
        public void OpenProject(object sender, ItemClickEventArgs e)
        {
            Projet projet = e.ClickedItem as Projet;
            Debug.WriteLine("item project clicked" + projet);

            if (projet != null)
            {
                this.Frame.Navigate(typeof(PageSelectedProjet), projet.Name);
            }
        }

        public void DeadlineItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem selectedItem)
            {
                string nomDuChoix = selectedItem.Text;
                string valeurDuChoix = selectedItem.Tag.ToString();

                System.Diagnostics.Debug.WriteLine($"Option choisie : {nomDuChoix}, Valeur : {valeurDuChoix}");
                DeadlineButton.Content = nomDuChoix;
                DeadlineButton.Tag = valeurDuChoix;
            }
        }

    }
}
