using app_test.Collection;
using app_test.Items;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Core;
using Windows.Storage;
using Image = Microsoft.UI.Xaml.Controls.Image;

namespace app_test.Pages
{
    public sealed partial class PageSelectedProjet : Page, INotifyPropertyChanged
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public Array PriorityOptions { get; } = Enum.GetValues(typeof(PriorityList));

        private Projet _SelectedProjet;
        public Projet SelectedProjet
        {
            get => _SelectedProjet;
            set { _SelectedProjet = value; OnPropertyChanged(nameof(SelectedProjet)); }
        }

        public PageSelectedProjet()
        {
            SelectedProjet = new Projet();
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _taskTimer?.Stop();

            if (e.Parameter is Projet projet_1 && projet_1 != null)
            {
                SelectedProjet = projet_1;

                Debug.WriteLine("Projet récupéré : " + SelectedProjet.Name + SelectedProjet.Description + SelectedProjet.Deadline);

                if (SelectedProjet.Items != null)
                {
                    foreach (var item in SelectedProjet.Items)
                    {
                        if (item is Texte itemTexte && File.Exists(itemTexte.Source))
                            itemTexte.Contenu = await File.ReadAllTextAsync(itemTexte.Source);
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _taskTimer?.Stop();
            _taskTimer = null;
        }

        public static Windows.Media.Playback.IMediaPlaybackSource MettreEnMediaSource(string cheminFichier)
        {
            if (string.IsNullOrEmpty(cheminFichier) || !File.Exists(cheminFichier))
                return null;
            try
            {
                return MediaSource.CreateFromUri(new Uri(cheminFichier));
            }
            catch
            {
                return null;
            }
        }

        private async void CreateItemTexte_Click(object sender, RoutedEventArgs e)
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dossier = Path.Combine(localAppData, "MonAppTest", "BDD", "Items", SelectedProjet.Name);
            if (!Directory.Exists(dossier))
                Directory.CreateDirectory(dossier);

            Texte item = new Texte("en_cours");
            string fileName = SelectedProjet.Name + "_" + item.Id + ".txt";
            string cheminComplet = Path.Combine(dossier, fileName);

            item.Source = cheminComplet;
            SelectedProjet.Items.Add(item);
            File.Create(cheminComplet).Dispose();

            Debug.WriteLine("Fichier créé : " + cheminComplet);
        }

        private async void CreateItemImage_Click(object sender, RoutedEventArgs e)
        {
            Item item = new Items.Image("en_cours");
            SelectedProjet.Items.Add(item);
        }

        private async void CreateItemMedia_Click(object sender, RoutedEventArgs e)
        {
            Item item = new Media("en_cours");
            SelectedProjet.Items.Add(item);
        }

        private async void CreateItemDessin_Click(object sender, RoutedEventArgs e)
        {
            Item item = new Dessin("en_cours");
            SelectedProjet.Items.Add(item);
        }

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var picker = new FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);
            var file = await picker.PickSingleFileAsync();
            if (file == null) return;

            var stackPanel = button.Parent as StackPanel;
            if (stackPanel == null)
            {
                Debug.WriteLine("Erreur : parent du bouton n'est pas un StackPanel");
                return;
            }

            var target = stackPanel.FindName("TargetMedia");
            Item CurrentItem = button.DataContext as Item;
            if (CurrentItem == null) return;

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dossierItems = Path.Combine(localAppData, "MonAppTest", "BDD", "Items", SelectedProjet.Name); 
            if (!Directory.Exists(dossierItems)) Directory.CreateDirectory(dossierItems);

            switch (target)
            {
                case MediaPlayerElement player:
                    player.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(file.Path));
                    string pathDest = Path.Combine(dossierItems, $"{CurrentItem.Id}{Path.GetExtension(file.Path)}");
                    File.Copy(file.Path, pathDest, true);
                    CurrentItem.Source = pathDest;
                    break;

                case Image image:
                    var storageFile = await StorageFile.GetFileFromPathAsync(file.Path);
                    var bitmap = new BitmapImage();
                    using (var stream = await storageFile.OpenAsync(FileAccessMode.Read))
                        await bitmap.SetSourceAsync(stream);
                    image.Source = bitmap;
                    string pathDestImg = Path.Combine(dossierItems, $"{CurrentItem.Id}{Path.GetExtension(file.Path)}");
                    File.Copy(file.Path, pathDestImg, true);
                    CurrentItem.Source = pathDestImg;
                    break;

                default:
                    Debug.WriteLine("Type de média non reconnu");
                    break;
            }
        }

        private async void SaveAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            if (SelectedProjet?.Items == null) return;

            SauvegarderProjetAsync();
        }

        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var currentItem = button.DataContext as Item;
            if (currentItem == null) return;

            try
            {
                if (!string.IsNullOrEmpty(currentItem.Source) && File.Exists(currentItem.Source))
                    File.Delete(currentItem.Source);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Delete] {ex.Message}");
            }

            if (SelectedProjet?.Items != null && SelectedProjet.Items.Contains(currentItem))
                SelectedProjet.Items.Remove(currentItem);
        }

        private void CollectionsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedCollection = e.ClickedItem as Collection.Collection;
            if (selectedCollection == null) return;

            CollectionTitleTextBlock.Text = selectedCollection.Name;
            ObjetsGridView.ItemsSource = selectedCollection.Objets;
            CollectionsPanel.Visibility = Visibility.Collapsed;
            ObjetsPanel.Visibility = Visibility.Visible;
        }

        private async void BackToCollections_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async Task SauvegarderProjetAsync()
        {
            if (SelectedProjet == null) return;

            try
            {
                if (SelectedProjet.Items != null)
                {
                    foreach (var item in SelectedProjet.Items)
                    {
                        if (item is Texte itemTexte && !string.IsNullOrEmpty(itemTexte.Source))
                        {
                            await File.WriteAllTextAsync(itemTexte.Source, itemTexte.Contenu ?? string.Empty);
                        }
                    }
                }

                await Projet.CreateProjetFileJSON(SelectedProjet);
                Debug.WriteLine("Sauvegarde complète effectuée.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la sauvegarde : {ex.Message}");
            }
        }

        //Gestion du resize persistant des items
        private void Sizer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        // Pour gérer la sauvegarde du redimensionnement des items de façon pérenne
        private bool _isRestoringSize;

        private void ItemTarget_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue is Item item)
            {
                _isRestoringSize = true;

                if (item.Largeur.HasValue)
                    sender.Width = item.Largeur.Value;
                else
                    sender.ClearValue(FrameworkElement.WidthProperty);

                if (item.Hauteur.HasValue)
                    sender.Height = item.Hauteur.Value;
                else
                    sender.ClearValue(FrameworkElement.HeightProperty);

                _isRestoringSize = false;
            }
        }

        private void ItemTarget_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isRestoringSize) return;

            if (sender is FrameworkElement target && target.DataContext is Item item)
            {
                item.Largeur = e.NewSize.Width;
                item.Hauteur = e.NewSize.Height;
            }
        }

        //Task Section
        private void TaskComplete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is ProjectTask task)
            {
                task.IsComplete = true;
            }
        }
        
        private async void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskContent_TextBox.Text))
            {
                return;
            }
            string content = TaskContent_TextBox.Text;

            if (double.IsNaN(TaskDeadline_TextBox.Value))
            {
                return;
            }

            int days = (int)TaskDeadline_TextBox.Value;
            DateTime deadline = DateTime.Now.AddDays(days);

            PriorityList priority = PriorityList.Low;
            if (TaskPriority_ComboBox.SelectedItem != null)
            {
                priority = (PriorityList)TaskPriority_ComboBox.SelectedItem;
            }

            double time = double.IsNaN(TimeTask_TextBox.Value) ? 0 : TimeTask_TextBox.Value;

            if (SelectedProjet != null)
            {
                ProjectTask newTask;

                if (time != 0)
                {
                    newTask = new ProjectTask(content, priority, time, deadline);
                }
                else
                {
                    newTask = new ProjectTask(content, priority, deadline);
                }
                SelectedProjet.Tasks.Add(newTask);
                TaskContent_TextBox.Text = string.Empty;
                TaskPriority_ComboBox.SelectedItem = null;
                TaskDeadline_TextBox.Value = double.NaN;
                TimeTask_TextBox.Value = double.NaN;
            }
        }

        // Timer début 
        private DispatcherTimer _taskTimer;

        private void InitTaskTimer()
        {
            _taskTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _taskTimer.Tick += TaskTimer_Tick;
            _taskTimer.Start();
        }

        private void TaskTimer_Tick(object sender, object e)
        {
            if (SelectedProjet?.Tasks == null) return;

            foreach (var task in SelectedProjet.Tasks)
            {
                if (task.IsRunning && task.RemainingTime > TimeSpan.Zero)
                {
                    var before = task.RemainingTime;
                    task.RemainingTime -= TimeSpan.FromSeconds(1);
    
                    if (task.RemainingTime <= TimeSpan.Zero)
                    {
                        task.RemainingTime = TimeSpan.Zero;
                        task.IsRunning = false;
                    }
                }
            }
        }

        private void PlayPauseTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProjectTask task)
            {
                task.IsRunning = !task.IsRunning;
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_taskTimer == null)
            {
                InitTaskTimer();
                Debug.WriteLine("[Init] Page_Loaded - InitTaskTimer appelé");
            }
        }

        private void CopyText_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.DataContext is Texte texteCourant)
            {
                // Récuparation du texte en utilisant le DataContext de l'item
                string texteACopier = texteCourant.Contenu ?? string.Empty;

                // Copié contenu dans presse papié windows
                var dataPackage = new DataPackage();
                dataPackage.SetText(texteACopier);
                Clipboard.SetContent(dataPackage);
            }
        }
    }
        

            public class LaravelResponse<T>
            {
                [JsonPropertyName("data")]
                public T Data { get; set; }
            }
 }