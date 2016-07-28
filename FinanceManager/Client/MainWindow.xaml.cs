using Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Portfolio portfolio = PortfolioReader.ReadFromFile(ConfigurationManager.AppSettings["portfolioFile"]);
        XigniteQuoter quoter = new XigniteQuoter(ConfigurationManager.AppSettings["secret"]);

        public MainWindow()
        {
            InitializeComponent();
            this.portfolio.Update(this.quoter);
            this.DataContext = this.portfolio;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            this.portfolio.Update(this.quoter);
        }
    }
}
