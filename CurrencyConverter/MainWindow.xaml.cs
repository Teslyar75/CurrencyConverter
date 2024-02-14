using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace CurrencyConverter
{
    public partial class ResultWindow : Window
    {
       

        

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            amountInUAHTextBox.Clear();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код для кнопки "Сохранить"

            // Запрашиваем у пользователя путь для сохранения файла
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Открываем поток для записи в выбранный файл
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        // Получаем отраженную сумму после обмена
                        string result = resultTextBlock.Text;

                        // Записываем результат в файл
                        writer.WriteLine(result);

                        MessageBox.Show($"Результат успешно сохранен в файл: {saveFileDialog.FileName}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}");
                }
            }
        }


        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private const string ApiUrl = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";
        private const string CurrencyFileName = "currency.json";

        private List<Currency> currencies;

        public ResultWindow()
        {
            InitializeComponent();
            UpdateCurrencies();
            DisplayCurrencies();
        }
        private void UpdateCurrencies()
        {
            currencies = LoadCurrencies();
        }

        private List<Currency> LoadCurrencies()
        {
            if (File.Exists(CurrencyFileName) && !IsFileOutdated(CurrencyFileName))
            {
                string json = File.ReadAllText(CurrencyFileName);
                return JsonConvert.DeserializeObject<List<Currency>>(json);
            }
            else
            {
                return DownloadCurrencies();
            }
        }

        private bool IsFileOutdated(string fileName)
        {
            DateTime lastWriteTime = File.GetLastWriteTime(fileName);
            return DateTime.Now.Date > lastWriteTime.Date;
        }

        private List<Currency> DownloadCurrencies()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string json = client.GetStringAsync(ApiUrl).Result;
                    List<Currency> downloadedCurrencies = JsonConvert.DeserializeObject<List<Currency>>(json);
                    File.WriteAllText(CurrencyFileName, json);

                    return downloadedCurrencies;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке курсов валют: {ex.Message}");
                return new List<Currency>();
            }
        }

        private void DisplayCurrencies()
        {
            // Очищаем ListBox перед добавлением новых элементов
            currencyListBox.Items.Clear();

            // Выбираем только нужные валюты
            var selectedCurrencies = currencies.FindAll(c => c.Cc == "USD" || c.Cc == "EUR" || c.Cc == "GBP");

            // Отображаем валюты в ListBox
            foreach (Currency currency in selectedCurrencies)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = $"{currency.Cc} - {currency.Rate} UAH";
                currencyListBox.Items.Add(item);
            }
        }


        private void CurrencyListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Пользователь выбирает валюту
            if (currencyListBox.SelectedItem != null)
            {
                // Вызываем метод конвертации
                ConvertAndDisplayResult();
            }
        }

        private decimal ConvertCurrency(decimal amountInUAH, string selectedCurrencyCode)
        {
            Currency selectedCurrency = currencies.Find(c => c.Cc.Trim() == selectedCurrencyCode);

            if (selectedCurrency != null)
            {
                decimal rate = selectedCurrency.Rate;
                return amountInUAH / rate;
            }

            return 0.0m;
        }
        private void AmountInUAHTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Если нажата клавиша ENTER
            if (e.Key == Key.Enter)
            {
                // Вызываем метод конвертации
                ConvertAndDisplayResult();
            }
        }

      

        private void ConvertAndDisplayResult()
        {
            if (currencyListBox.SelectedItem != null)
            {
                ListBoxItem selectedItem = (ListBoxItem)currencyListBox.SelectedItem;
                string selectedCurrencyString = selectedItem.Content.ToString();
                string selectedCurrencyCode = selectedCurrencyString.Split('-')[0].Trim();

                string amountInUAHString = amountInUAHTextBox.Text;
                if (decimal.TryParse(amountInUAHString, out decimal amountInUAH))
                {
                    decimal result = ConvertCurrency(amountInUAH, selectedCurrencyCode);
                    resultTextBlock.Text = $"Отраженная сумма после обмена: {result} {selectedCurrencyCode}";
                }
                else
                {
                    MessageBox.Show("Некорректный формат введенной суммы.");
                }
            }
        }

    }
}
