
using System;
using System.Globalization;
using System.Linq;
using Microcharts;
using SkiaSharp;
using Microsoft.Maui.Controls;
using BusinessDashboard.Models;
using BusinessDashboard.Data;

namespace BusinessDashboard
{
    public partial class DashboardPage : ContentPage
    {
        private const double PercentagemGanhosCaixa = 0.30;
        private const double PercentagemGanhosGorjetas = 0.25;
        private const int DiaEquilibrioFinanceiro = 15;

        private double DespesasFixasMensais = 5000.0;

        private double TotalGanhosAteAgora = 0.0;
        private double TotalDespesasAteAgora = 0.0;

        // Cultura portuguesa para apresentar valores em euros
        private readonly CultureInfo culturaEuro =
            new CultureInfo("pt-PT");

        public DashboardPage()
        {
            InitializeComponent();
        }

        private async void OnSubmitClick(object sender, EventArgs e)
        {
            // Validação das entradas
            if (!double.TryParse(CashierTotalInput.Text,
                NumberStyles.Any, culturaEuro, out double totalCaixa))
            {
                await DisplayAlert("Erro de Entrada",
                    "Por favor, insira um valor válido para o Total em Caixa.", "OK");
                return;
            }

            if (!double.TryParse(TipsInput.Text,
                NumberStyles.Any, culturaEuro, out double gorjetas))
            {
                await DisplayAlert("Erro de Entrada",
                    "Por favor, insira um valor válido para as Gorjetas.", "OK");
                return;
            }

            if (!double.TryParse(DailyExpensesInput.Text,
                NumberStyles.Any, culturaEuro, out double despesasDiarias))
            {
                await DisplayAlert("Erro de Entrada",
                    "Por favor, insira um valor válido para as Despesas Diárias.", "OK");
                return;
            }

            // Cálculo dos ganhos
            double ganhosCaixa = totalCaixa * PercentagemGanhosCaixa;
            double ganhosGorjetas = gorjetas * PercentagemGanhosGorjetas;
            double ganhosDiarios = ganhosCaixa + ganhosGorjetas;

            // Atualizar totais
            TotalGanhosAteAgora += ganhosDiarios;
            TotalDespesasAteAgora += despesasDiarias;

            // Cálculo do progresso até ao equilíbrio financeiro
            double objetivoEquilibrio =
                DespesasFixasMensais / DiaEquilibrioFinanceiro;

            double objetivoDiario =
                objetivoEquilibrio * DiaEquilibrioFinanceiro
                - TotalGanhosAteAgora;

            // Atualizar resultados na interface
            NetEarningsLabel.Text =
                ganhosDiarios.ToString("C", culturaEuro);

            BreakevenProgressLabel.Text =
                $"{TotalGanhosAteAgora.ToString("C", culturaEuro)} / " +
                $"{DespesasFixasMensais.ToString("C", culturaEuro)}";

            DailyTargetLabel.Text = objetivoDiario > 0
                ? objetivoDiario.ToString("C", culturaEuro)
                : "Meta diária atingida";

            // Criar gráfico de pizza
            var chart = new PieChart
            {
                Entries = new[]
                {
                    new Microcharts.ChartEntry((float)ganhosCaixa)
                    {
                        Label = "Ganhos Caixa",
                        ValueLabel = ganhosCaixa.ToString("C", culturaEuro),
                        Color = SKColor.Parse("#FF6347")
                    },

                    new Microcharts.ChartEntry((float)ganhosGorjetas)
                    {
                        Label = "Ganhos Gorjetas",
                        ValueLabel = ganhosGorjetas.ToString("C", culturaEuro),
                        Color = SKColor.Parse("#4682B4")
                    },

                    new Microcharts.ChartEntry((float)despesasDiarias)
                    {
                        Label = "Despesas",
                        ValueLabel = despesasDiarias.ToString("C", culturaEuro),
                        Color = SKColor.Parse("#32CD32")
                    }
                },

                // Tamanho das legendas
                LabelTextSize =23,

                // Fundo branco
                BackgroundColor = SKColors.White
            };

            // Mostrar gráfico
            PieChartView.Chart = chart;

            // Guardar dados na base de dados
            var earningsData = new EarningsEntry
            {
                Timestamp = DateTime.Now,
                CashierEarnings = ganhosCaixa,
                TipsEarnings = ganhosGorjetas,
                DailyExpenses = despesasDiarias,
                TotalEarningsSoFar = TotalGanhosAteAgora,
                TotalExpensesSoFar = TotalDespesasAteAgora
            };

            var databaseService = new DatabaseService();

            await databaseService.AddEntryAsync(earningsData);

            // Limpar campos de entrada
            CashierTotalInput.Text = string.Empty;
            TipsInput.Text = string.Empty;
            DailyExpensesInput.Text = string.Empty;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var databaseService = new DatabaseService();

            var allEntries = await databaseService.GetEntriesAsync();

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var monthlyEntries = allEntries
                .Where(e => e.Timestamp.Month == currentMonth
                         && e.Timestamp.Year == currentYear);

            TotalGanhosAteAgora = monthlyEntries.Sum(
                e => e.CashierEarnings + e.TipsEarnings);

            TotalDespesasAteAgora = monthlyEntries.Sum(
                e => e.DailyExpenses);
        }

        private async void OnViewMonthlyDataClicked(
            object sender, EventArgs e)
        {
            // Abrir página dos dados mensais
            await Navigation.PushAsync(new MonthlySummaryPage());
        }

        private async void OnViewDataClicked(
            object sender, EventArgs e)
        {
            // Abrir página de visualização dos dados
            await Navigation.PushAsync(new ViewDataPage());
        }
    }
}
