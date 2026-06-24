using app_test.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace app_test
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            //S'avbonne à ContentFrame_Navigated; / à chauqe fois que je navigue ça execute la fonction
            ContentFrame.Navigated += ContentFrame_Navigated;
            ContentFrame.Navigate(typeof(PageProjets));

        }
        //Permet d'évaluer si le NavView.IsBackEnabled est vrai ou faux, en évaluant le ContentFrame.CanGoBack
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;
        }
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var itemClique = (NavigationViewItem)args.SelectedItem;

            string tag = itemClique.Tag.ToString();

            switch(tag)
            {
                case "Projets":
                    ContentFrame.Navigate(typeof(PageProjets));
                    break;
                case "Parametres":
                    ContentFrame.Navigate(typeof(PageSelectedProjet));
                    break;
            }
        }
        private void navigateBackButton_Click(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();

                //NavUpdate();
            }
        }
        //sert à mettre à update l'UI du menu lors du changement de page
        private void NavUpdate()
        {
            var pageActuelle = ContentFrame.CurrentSourcePageType;
            if (pageActuelle == typeof(PageProjets))
            {
                NavView.SelectedItem = NavView.MenuItems[0]; 
            }
            else if (pageActuelle == typeof(PageSelectedProjet)) 
            {
                NavView.SelectedItem = NavView.MenuItems[1];
            }
        }
    }
}
