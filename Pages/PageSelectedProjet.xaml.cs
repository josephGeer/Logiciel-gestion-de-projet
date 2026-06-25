using app_test.Items;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using System;

using System.ComponentModel;
using System.Diagnostics;
using System.IO;

using System.Text.Json;
using System.Threading.Tasks;

using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Search;
using Image = Microsoft.UI.Xaml.Controls.Image;

namespace app_test.Pages
{

    //sealed signifie classe finale, personne peux hériter de cette classe, partial signifie que la classe est partagé entre plusieru fichiers
    public sealed partial class PageSelectedProjet : Page, INotifyPropertyChanged
    {
        //Grace à ça lorsque dans OnNavigatedTo je remplis SelectedProjet, alors ça met à jour l'UI, ainsi le gridView est informé, et a enfin accès à une liste d'item.
        //Stcok vraiment le selectedProjet
        private Projet _SelectedProjet;
        public Projet SelectedProjet 
        { 
            get => _SelectedProjet;
            //Le délire value / Page.SelectedProjet = monNouveauProjet; -> compilateur prend monNouveauProjet et met dans value, puis setter prend ce qu'il y a dans value et l'ajoute à _SelectedProjet
            set { _SelectedProjet = value; OnPropertyChanged(nameof(SelectedProjet)); } 
        } 

        public PageSelectedProjet()
        {
            SelectedProjet = new Projet();
            InitializeComponent();

            this.KeyDown += PageSelectedProjet_KeyDown;

        }

        public event PropertyChangedEventHandler? PropertyChanged;

        //virtual signifie que la méthode peut être redéfinis par une sous classe, (vue que le class est sealed c'est pas possible)
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //Fonction qui est appellé lorsque dans la PageProjet je clique sur un proje / récupère en paramètre le nom du projet puis recharge les données via le fichier JSON
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Projet projet_1)
            {
                if (projet_1 != null)
                {
                    SelectedProjet = projet_1;

                    Debug.WriteLine("L'objet a été récupéré vérifions les informations : " + SelectedProjet.Name + SelectedProjet.Description + SelectedProjet.Deadline);

                    if (SelectedProjet?.Items != null)
                    {
                        foreach (var item in SelectedProjet.Items)
                        {
                            if (item is Texte itemTexte && File.Exists(itemTexte.Source))
                            {
                                itemTexte.Contenu = await File.ReadAllTextAsync(itemTexte.Source);
                            }
                        }
                    }
                }

            }
        }
       
        // Cette méthode va être appelée automatiquement par le x:Bind du MediaPlayerElement
        public static Windows.Media.Playback.IMediaPlaybackSource MettreEnMediaSource(string cheminFichier)
        {
            if (string.IsNullOrEmpty(cheminFichier) || !System.IO.File.Exists(cheminFichier))
            {
                return null;
            }
            try
            {
                // Convertit le chemin absolu du disque en Uri puis en MediaSource
                return Windows.Media.Core.MediaSource.CreateFromUri(new Uri(cheminFichier));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async void CreateItemTexte_Click(object sender, RoutedEventArgs e)
        {
            if(sender is MenuFlyoutItem name)
            {
                string text = name.Text;
                Debug.WriteLine("CreateItemTexte_Click est appelé , le event est : " + text);
            }
            int position = 0; 

            if (SelectedProjet.Items != null)
            {
                position = SelectedProjet.Items.Count + 1;
            }

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string dossier = Path.Combine(localFolder.Path, "BDD", "Items",SelectedProjet.Name);
            if (!Directory.Exists(dossier))
            {
                Directory.CreateDirectory(dossier);
            }

            Texte item = new Texte(position, "en_cours");

            string fileName = SelectedProjet.Name + "_" + item.Id + ".txt";
            string cheminComplet = Path.Combine(dossier, fileName);

            item.Source = cheminComplet;

            SelectedProjet.Items.Add(item);
            await Projet.CreateProjetFileJSON(SelectedProjet);
            //Si je veux juste créer le fichier : .Dispose permet de dire à windows, j'ai finis de créer le fichier tu peux fermé le flux et libéré le verroux
            File.Create(cheminComplet).Dispose();
            Debug.WriteLine("Le fichier " + fileName + " est créer et stocké dans : " + cheminComplet);
            // Si je veux écrire dans le fichier : 
            //using (StreamWriter sw = new StreamWriter(cheminComplet))
        }

        private async void CreateItemImage_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("CreateItemImage click activé");
            int position = 0;
            if (SelectedProjet.Items != null)
            {
                position = SelectedProjet.Items.Count + 1;
            }
            Item item = new Items.Image(position, "en_cours");
            SelectedProjet.Items.Add(item);
            await Projet.CreateProjetFileJSON(SelectedProjet);

        }

        private async void CreateItemVideo_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("CreateItemVideo click activé");
            int position = 0;

            if (SelectedProjet.Items != null)
            {
                position = SelectedProjet.Items.Count + 1;
            }

            Item item = new Video(position, "en_cours");
            SelectedProjet.Items.Add(item);
            await Projet.CreateProjetFileJSON(SelectedProjet);
        }

        private async void OpenFileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var picker = new FileOpenPicker((sender as Button).XamlRoot.ContentIslandEnvironment.AppWindowId);
            var file = await picker.PickSingleFileAsync();
            if (file == null)
                return;

            var button =  sender as Button;
            var stackPanel = button.Parent as StackPanel;
            if(stackPanel == null)
            {
                Debug.WriteLine("Erreur le parent du bouton n'est pas un StackPanel");
                return;
            }

            var target = stackPanel.FindName("TargetMedia");

            //on récupère l'item associé au bouton / Le GridView assigne automatiquement l'objet Item (de ta collection) à la propriété DataContext de ce GridViewItem.
            //En gros ça peremt de retrouvé l'objet exact avec lequel le bouton a été affiché
            Item CurrentItem = button.DataContext as Item;
            if (CurrentItem == null)
            {
                return;
            }

            string dossierItems = Path.Combine(ApplicationData.Current.LocalFolder.Path, "BDD", "Items", SelectedProjet.Name);
            if (!Directory.Exists(dossierItems)) Directory.CreateDirectory(dossierItems);

            switch (target)
            {
                case MediaPlayerElement player:
                    player.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(file.Path));

                    string extension = Path.GetExtension(file.Path);
                    string fileName = $"{CurrentItem.Id}{extension}";
                    string pathDest = Path.Combine(dossierItems, fileName);


                    File.Copy(file.Path, pathDest, true);
                    CurrentItem.Source = pathDest;

                    Debug.WriteLine("pathDest : " + pathDest);

                    await Projet.CreateProjetFileJSON(SelectedProjet);
                    break;

                case Image image:
                    var storageFile = await StorageFile.GetFileFromPathAsync(file.Path);
                    var bitmap = new BitmapImage();
                    using (var stream = await storageFile.OpenAsync(FileAccessMode.Read))
                    {
                        await bitmap.SetSourceAsync(stream);
                    }
                    image.Source = bitmap; // Affiche l'image immédiatement

                    // --- AJOUTE BIEN CE BLOC POUR SAUVEGARDER L'IMAGE ---
                    string extensionImg = Path.GetExtension(file.Path);
                    string fileNameImg = $"{CurrentItem.Id}{extensionImg}";
                    string pathDestImg = Path.Combine(dossierItems, fileNameImg);

                    // On copie physiquement le fichier dans le dossier BDD/Items/NomProjet
                    File.Copy(file.Path, pathDestImg, true);

                    // On met à jour la propriété Source de l'item (ce qui va déclencher le x:Bind)
                    CurrentItem.Source = pathDestImg;

                    // On sauvegarde le projet mis à jour dans le fichier JSON
                    await Projet.CreateProjetFileJSON(SelectedProjet);
                    break;

                case ElementSoundPlayer audio:
                    Debug.WriteLine("Audio a venir");               
                    break;

                default:
                    Debug.WriteLine("Type de média non reconnu ou non implémenté");
                    break;
            }

        }
        private async void PageSelectedProjet_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            // Vérifie si la touche Ctrl est enfoncée ET si la touche S est pressée
            var ctrlAppuye = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
            if ((ctrlAppuye & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down
                && e.Key == Windows.System.VirtualKey.S)
            {
                // Indique au système qu'on a géré l'événement (évite d'autres comportements)
                e.Handled = true;

                Debug.WriteLine("Raccourci Ctrl + S détecté ! Sauvegarde en cours...");

                if (SelectedProjet?.Items == null) return;

                try
                {
                    // On parcourt tous les items pour sauvegarder uniquement les textes
                    foreach (var item in SelectedProjet.Items)
                    {
                        if (item is Texte itemTexte)
                        {
                            // Écrit de manière asynchrone le contenu du TextBox dans le fichier correspondant
                            await File.WriteAllTextAsync(itemTexte.Source, itemTexte.Contenu);
                            Debug.WriteLine($"Fichier sauvegardé : {itemTexte.Source}");
                        }
                    }

                    Debug.WriteLine("Tous les fichiers textes ont été sauvegardés avec succès !");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur lors de la sauvegarde : {ex.Message}");
                }
            }
        }
        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            //Récupéraiton de l'item
            var button = sender as Button;
            if (button == null) return;

            var currentItem = button.DataContext as Item;
            if (currentItem == null) return;

            Debug.WriteLine($"[Delete] Demande de suppression pour l'item ID : {currentItem.Id}");
            
            //Supression du fichier associé à l'item
            try
            {
                if (!string.IsNullOrEmpty(currentItem.Source) && File.Exists(currentItem.Source))
                {
                    File.Delete(currentItem.Source);
                    Debug.WriteLine($"[Delete] Fichier supprimé sur le disque : {currentItem.Source}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Delete] Avertissement lors du File.Delete : {ex.Message}");
            }

            // Supression de l'item au niveau de la liste d'item du projet
            if (SelectedProjet?.Items != null && SelectedProjet.Items.Contains(currentItem))
            {
                SelectedProjet.Items.Remove(currentItem);
                Debug.WriteLine("[Delete] Item supprimé de l'ObservableCollection (UI mise à jour).");
            }

            //Sauvegarde de la nouvelle structure du projet dans le fichier JSON
            await Projet.CreateProjetFileJSON(SelectedProjet);
            Debug.WriteLine("[Delete] Fichier JSON du projet mis à jour.");
        }

        //La gestion de l'index est propre dans le liste view et dans la liste d'items, donc pas besoin de noter la position
        private async void ItemGridView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (SelectedProjet != null)
            {
                Debug.WriteLine("[DragAndDrop] L'ordre des éléments a changé. Sauvegarde du fichier JSON...");
                await Projet.CreateProjetFileJSON(SelectedProjet);
                Debug.WriteLine("[DragAndDrop] Fichier JSON mis à jour avec le nouvel ordre.");
            }
        }
    }


}
