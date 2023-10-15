using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ModShardLauncher.Mods;
using UndertaleModLib;
using UndertaleModTool;

namespace ModShardLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public UndertaleData Data => DataLoader.data;
        public object Selected;
        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(ModLoader.ModPath))
                Directory.CreateDirectory(ModLoader.ModPath);
            Instance = this;
            Dispatcher.Invoke(async () => await ModLoader.LoadAssemblies());
            ModThings.Content = new DescriptionView(Application.Current.FindResource("Welcome_Use").ToString(),
                Application.Current.FindResource("Welcome_Desc").ToString());
        }
        public async void Command_Open(object sender, ExecutedRoutedEventArgs e)
        {
            await DataLoader.DoOpenDialog();
        }
        private void Eng_Click(object sender, RoutedEventArgs e)
        {
            var resDict = Application.Current.Resources.MergedDictionaries.First(t => t.Source.OriginalString == @"Language/en-us.xaml");
            Application.Current.Resources.MergedDictionaries.Remove(resDict);
            Application.Current.Resources.MergedDictionaries.Add(resDict);
            if (ModThings.Content.GetType() == typeof(DescriptionView))
                ModThings.Content = new DescriptionView(Application.Current.FindResource("Welcome_Use").ToString(),
                Application.Current.FindResource("Welcome_Desc").ToString());
            else OpenInTab(Selected.ToString());
            ((SearchBox.Style.Resources.Values.Cast<object>().ToList()[0] as VisualBrush)
                .Visual as Label).Content = Application.Current.FindResource("SearchBoxText").ToString();
        }
        private void Chi_Click(object sender, RoutedEventArgs e)
        {
            var resDict = Application.Current.Resources.MergedDictionaries.First(t => t.Source.OriginalString == @"Language/zh-cn.xaml");
            Application.Current.Resources.MergedDictionaries.Remove(resDict);
            Application.Current.Resources.MergedDictionaries.Add(resDict);
            if (ModThings.Content.GetType() == typeof(DescriptionView))
                ModThings.Content = new DescriptionView(Application.Current.FindResource("Welcome_Use").ToString(),
                Application.Current.FindResource("Welcome_Desc").ToString());
            else OpenInTab(Selected.ToString());
            ((SearchBox.Style.Resources.Values.Cast<object>().ToList()[0] as VisualBrush)
                .Visual as Label).Content = Application.Current.FindResource("SearchBoxText").ToString();
        }
        public void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        public void MainTree_KeyDown(object sender, KeyEventArgs e)
        {

        }
        public void MainTree_KeyUp(object sender, KeyEventArgs e)
        {

        }
        public void MainTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Selected != null)
            {
                OpenInTab(Selected.ToString());
                TabLabel.Content = Selected.ToString();
            }
        }
        public void MainTree_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        public void MainTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        public void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await ModLoader.LoadAssemblies();
        }
        private void OpenInTab(string obj)
        {
            ModThings.Content = ModLoader.Mods[obj];
            ModThings.InvalidateVisual();
        }

        private void MainTree_Selected(object sender, RoutedEventArgs e)
        {
            Selected = (e.OriginalSource as TreeViewItem).DataContext;
        }

        private async void PatchButton_Click(object sender, RoutedEventArgs e)
        {
            await ModLoader.PatchFile();
        }
    }
}
