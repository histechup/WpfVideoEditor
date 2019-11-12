using WpfVideoEditor.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfVideoEditor.Controls
{
    /// <summary>
    /// Interaction logic for ClipsList.xaml
    /// </summary>
    public partial class ClipsListControl : UserControl
    {
        public event EventHandler<ClipEventArgs> Play;
        public event EventHandler<ClipEventArgs> Export;
        public event EventHandler<ClipEventArgs> SetBegin;
        public event EventHandler<ClipEventArgs> SetEnd;

        public ClipsListControl()
        {
            InitializeComponent();
        }

        private void Play_Click(object sender, RoutedEventArgs e) => Play?.Invoke(this, new ClipEventArgs((Clip)cDataGrid.CurrentItem));
        private void Remove_Click(object sender, RoutedEventArgs e) => ((ClipsCollection)DataContext).Remove((Clip)cDataGrid.CurrentItem);
        private void Export_Click(object sender, RoutedEventArgs e) => Export?.Invoke(this, new ClipEventArgs((Clip)cDataGrid.CurrentItem));
        private void SetBegin_Click(object sender, RoutedEventArgs e) => SetBegin?.Invoke(this, new ClipEventArgs((Clip)cDataGrid.CurrentItem));
        private void SetEnd_Click(object sender, RoutedEventArgs e) => SetEnd?.Invoke(this, new ClipEventArgs((Clip)cDataGrid.CurrentItem));
    }
}
