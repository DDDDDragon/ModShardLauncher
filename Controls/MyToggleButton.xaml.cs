﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModShardLauncher.Controls
{
    /// <summary>
    /// MyToggleButton.xaml 的交互逻辑
    /// </summary>
    public partial class MyToggleButton : UserControl
    {
        public ImageSource ImageSource { get; set; }
        public string Text { get; set; }
        [Category("Behavior")]
        public event EventHandler Checked;
        public event EventHandler Click;
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(MyToggleButton),
            new PropertyMetadata(default(string), OnTextChanged)
        );
        public MyToggleButton()
        {
            InitializeComponent();
        }
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }
        private void MyButton_Checked(object sender, RoutedEventArgs e)
        {
            Checked?.Invoke(this, e);
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}
