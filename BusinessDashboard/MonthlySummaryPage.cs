
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microcharts;
using SkiaSharp;
using Microsoft.Maui.Controls;
using BusinessDashboard.Data;

namespace BusinessDashboard
{
    public partial class MonthlySummaryPage : ContentPage
    {
        private readonly DatabaseService databaseService;

        // Formatação portuguesa e euros
        private readonly CultureInfo culturaEuro =
            new CultureInfo("pt-PT");

        public MonthlySummaryPage()
        {
            InitializeComponent();
            databaseService = new DatabaseService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var summaries = await databaseService.GetMonthlySummariesAsync();

            // Popular Picker com os meses disponíveis
            MonthPicker.ItemsSource = summaries
                .OrderByDescending(s => new DateTime(s.Year, s.Month, 1))
                .Select(s => $"{s.Month:D2}/{s.Year}")
                .ToList();

            // Selecionar mês atual por padrão
            var currentMonthString =
                $"{DateTime.Now.Month:D2}/{DateTime.Now.Year}";

            int index = MonthPicker.ItemsSource.IndexOf(currentMonthString);

            if (index >= 0)
                MonthPicker.SelectedIndex = index;

            await LoadChartData(currentMonthString);
        }

        private async void OnMonthSelected(object sender, EventArgs e)
        {
            if (MonthPicker.SelectedIndex >= 0)
            {
                string selectedMonth = MonthPicker.SelectedItem.ToString();

                await LoadChartData(selectedMonth);
            }
            else
            {
                await DisplayAlert(
                    "Seleção Inválida",
                    "Por favor, selecione um mês válido.",
                    "OK");
            }
        }

        private async Task LoadChartData(string monthYear)
        {
            const double DespesasFixasMensais = 5000.0;

            var summaries =
                await databaseService.GetMonthlySummariesAsync();

            var parts = monthYear.Split('/');

            int month = int.Parse(parts[0]);
            int year = int.Parse(parts[1]);

            var summary = summaries.FirstOrDefault(
                s => s.Month == month && s.Year == year);

            if (summary == null)
            {
                await DisplayAlert(
                    "Sem Dados",
                    "Não há dados para o mês selecionado.",
                    "OK");

                return;
            }

            // Cálculo do progresso financeiro
            double progresso = Math.Min(
                summary.TotalEarnings,
                DespesasFixasMensais);

            double restante = Math.Max(
                DespesasFixasMensais - progresso,
                0);

            // Criar gráfico
            var entries = new[]
            {
                new ChartEntry((float)progresso)
                {
                    Label = "Conquistado",
                    ValueLabel = progresso.ToString("C", culturaEuro),
                    Color = SKColor.Parse("#32CD32")
                },

                new ChartEntry((float)restante)
                {
                    Label = "Faltando",
                    ValueLabel = restante.ToString("C", culturaEuro),
                    Color = SKColor.Parse("#FF6347")
                }
            };

            // Apresentar gráfico de pizza
            PieChartView.Chart = new PieChart
            {
                Entries = entries,

                // Tamanho das legendas
                LabelTextSize = 32,

                // Fundo branco
                BackgroundColor = SKColors.White
            };
        }

        private async void OnUpdateDataClicked(object sender, EventArgs e)
        {
            if (MonthPicker.SelectedIndex >= 0)
            {
                string selectedMonth =
                    MonthPicker.SelectedItem.ToString();

                await LoadChartData(selectedMonth);
            }
            else
            {
                await DisplayAlert(
                    "Seleção Inválida",
                    "Por favor, selecione um mês válido antes de atualizar.",
                    "OK");
            }
        }
    }
}
