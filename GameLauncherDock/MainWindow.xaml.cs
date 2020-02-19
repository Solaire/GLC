using System;
using System.Collections.Generic;
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

namespace GameLauncherDock
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void button1_click(object sender, RoutedEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show("STEAM?",
										  "Confirmation",
										  MessageBoxButton.YesNo,
										  MessageBoxImage.Question);
			if(result == MessageBoxResult.Yes)
			{
				Application.Current.Shutdown();
			}
		}

		private void button2_click(object sender, RoutedEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show("GOG?",
										  "Confirmation",
										  MessageBoxButton.YesNo,
										  MessageBoxImage.Question);
			if(result == MessageBoxResult.Yes)
			{
				Application.Current.Shutdown();
			}
		}

		private void button3_click(object sender, RoutedEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show("UPLAY?",
										  "Confirmation",
										  MessageBoxButton.YesNo,
										  MessageBoxImage.Question);
			if(result == MessageBoxResult.Yes)
			{
				Application.Current.Shutdown();
			}
		}

		private void button4_click(object sender, RoutedEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show("BETHESDA?",
										  "Confirmation",
										  MessageBoxButton.YesNo,
										  MessageBoxImage.Question);
			if(result == MessageBoxResult.Yes)
			{
				Application.Current.Shutdown();
			}
		}

		private void button5_click(object sender, RoutedEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show("YES?",
										  "Confirmation",
										  MessageBoxButton.YesNo,
										  MessageBoxImage.Question);
			if(result == MessageBoxResult.Yes)
			{
				Application.Current.Shutdown();
			}
		}
	}
}
