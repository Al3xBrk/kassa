using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;

namespace Kassa
{
    public partial class AdminProfitChartPage : Page
    {
        private readonly KassaContext _context = new KassaContext();
        public AdminProfitChartPage()
        {
            InitializeComponent();
            InitProfitChartControls();
        }

        private void InitProfitChartControls()
        {
            var years = _context.Orders.Where(o => o.StatusId == 2).Select(o => o.OrderDate.Year).Distinct().OrderBy(y => y).ToList();
            YearComboBox.ItemsSource = years;
            if (years.Count > 0)
                YearComboBox.SelectedItem = years.Last();
            MonthComboBox.ItemsSource = Enumerable.Range(1, 12).Select(m => new DateTime(2000, m, 1).ToString("MMMM"));
            MonthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            UpdateProfitChart();
        }

        private void YearOrMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProfitChart();
        }
        private void UpdateProfitChart()
        {
            if (YearComboBox.SelectedItem == null || MonthComboBox.SelectedIndex == -1)
                return;
            int year = (int)YearComboBox.SelectedItem;
            int month = MonthComboBox.SelectedIndex + 1;
            var orders = _context.Orders
                .Where(o => o.StatusId == 2 && o.OrderDate.Year == year && o.OrderDate.Month == month)
                .ToList();
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var dailySums = new decimal[daysInMonth];
            foreach (var order in orders)
            {
                int day = order.OrderDate.Day;
                dailySums[day - 1] += order.TotalAmount;
            }
            ProfitChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Прибыль",
                    Values = new ChartValues<decimal>(dailySums)
                }
            };
            ProfitChart.AxisX[0].Labels = Enumerable.Range(1, daysInMonth).Select(d => d.ToString()).ToArray();

            // Устанавливаем диапазон Y-оси для корректного отображения
            var maxValue = dailySums.Max();
            if (maxValue == 0)
            {
                // Если нет данных, устанавливаем диапазон 0-1000
                ProfitChart.AxisY[0].MinValue = 0;
                ProfitChart.AxisY[0].MaxValue = 1000;
            }
            else
            {
                // Если есть данные, устанавливаем автоматический диапазон
                ProfitChart.AxisY[0].MinValue = double.NaN;
                ProfitChart.AxisY[0].MaxValue = double.NaN;
            }
        }
    }
}
