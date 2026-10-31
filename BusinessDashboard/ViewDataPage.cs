
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BusinessDashboard.Data;
using Microsoft.Maui.Controls;

namespace BusinessDashboard
{
    public partial class ViewDataPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public ViewDataPage()
        {
            // Configurar cultura portuguesa e euros
            CultureInfo.DefaultThreadCurrentCulture =
                new CultureInfo("pt-PT");

            CultureInfo.DefaultThreadCurrentUICulture =
                new CultureInfo("pt-PT");

            InitializeComponent();

            _databaseService = new DatabaseService();

            LoadData();
        }

        private async void LoadData()
        {
            var data = await _databaseService.GetEntriesAsync();

            EarningsCollectionView.ItemsSource = data
                .OrderByDescending(entry => entry.Timestamp)
                .ToList();
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}
