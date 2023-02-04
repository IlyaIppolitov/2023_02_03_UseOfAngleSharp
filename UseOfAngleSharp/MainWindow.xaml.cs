using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
using libToGetVacancies;
using System.Collections.Concurrent;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Diagnostics;

namespace UseOfAngleSharp
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
        
        private async void buttonGetVac_Click(object sender, RoutedEventArgs e)
        {
            pbStatus.IsIndeterminate = true;

            // Очистка существующего списка
            dataGridVac.ItemsSource = null;
            dataGridVac.Items.Clear();
            dataGridVac.Items.Refresh();

            // Главная страница
            var uriMainPage = "https://proglib.io/vacancies/all";

            // проверка на корректность строки адреса
            if (!Uri.IsWellFormedUriString(uriMainPage, UriKind.Absolute))
            {
                MessageBox.Show("Строка запроса не соответствует адресу!");
                pbStatus.IsIndeterminate = false;
                return;
            }

            // Определение количества страниц
            var numberOfPages = await FuncToGetVacancies.getNumberOfPages(uriMainPage);

            if (numberOfPages.Item1 == false)
            {
                MessageBox.Show("Ошибка определения колчиества страниц!");
                pbStatus.IsIndeterminate = false;
                return;
            }

            // Инициализация массива с необходимым количеством элементов
            var arrOfPages = Enumerable.Range(0, numberOfPages.Item2).ToArray();

            // ConcurrentBag использовал для проерки
            // Результаты совпали, поэтому закоментировал
            var dataBag = new ConcurrentBag<VacancyData>();

            // Общая часть названия страницы
            var uri = "https://proglib.io/vacancies/all?workType=all&workPlace=all&experience=&salaryFrom=&page=";

            // Определение макимального количество параллельных потоков
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = 5
            };

            //Считывание необходимых данных с сайта параллельно
            await Parallel.ForEachAsync(arrOfPages, parallelOptions, async (i, _) =>
            {
                await FuncToGetVacancies.fillVacanciesFromPage(dataBag, uri + (i + 1));
            });

            dataGridVac.ItemsSource = dataBag;

            pbStatus.IsIndeterminate = false;
        }
        private void DG_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)e.OriginalSource;

            var psi = new ProcessStartInfo
            {
                FileName = link.NavigateUri.AbsoluteUri,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
