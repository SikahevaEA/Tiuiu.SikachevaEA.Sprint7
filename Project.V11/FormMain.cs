using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using Project.V11.Lib;

namespace Project.V11
{
    public partial class FormMain : Form
    {
        private string savedFilePath = "";

        public string SavedFilePath
        {
            get { return savedFilePath; }
        }

        public FormMain()
        {
            InitializeComponent();
        }

        DataService ds = new DataService();
        private void buttonSave_SEA_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                savedFilePath = saveFileDialog.FileName;

                // Получаем данные из TextBox'ов
                string LastName = TextBoxLastName_SEA.Text;
                string Name = textBoxName_SEA.Text;
                string SurName = textBoxSurName_SEA.Text;
                string Address = textBoxAddress_SEA.Text;
                string PhoneNumber = textBoxPhoneNuber_SEA.Text;
                string Salary = textBoxSalarry_SEA.Text;
                string DateOfBirth = textBoxDateOfBirth_SEA.Text;
                string JobTitle = textBoxJobTitle_SEA.Text;

                // Проверка, что все TextBox содержат текст
                if (string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(Name) ||
                    string.IsNullOrWhiteSpace(Address) || string.IsNullOrWhiteSpace(PhoneNumber) || string.IsNullOrWhiteSpace(Salary) ||
                    string.IsNullOrWhiteSpace(JobTitle) || string.IsNullOrWhiteSpace(DateOfBirth))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля перед сохранением данных.");
                }
                else
                {
                    // Проверка, начинается ли номер телефона с '8'
                    if (!PhoneNumber.StartsWith("8"))
                    {
                        MessageBox.Show("Номер телефона должен начинаться с '8'.");
                        return;
                    }

                    // Формированиее строки для сохранения
                    string dataToSave = $"{LastName};{Name};{SurName};{Address};{PhoneNumber};{DateOfBirth};{JobTitle};{Salary}";

                    try
                    {
                        // Открываем файл для записи с кодировкой UTF-8
                        using (StreamWriter sw = new StreamWriter(savedFilePath, true, Encoding.UTF8))
                        {
                            // Если файл пуст, добавляем заголовки столбцов
                            if (new FileInfo(savedFilePath).Length == 0)
                            {
                                string header = "Фамилия;Имя;Отчество;Адрес;Номер Телефона;Дата рождения;Должность;Зарплата";
                                sw.WriteLine(header);
                            }

                            // Запись данных в новую строку
                            sw.WriteLine(dataToSave);
                        }

                        MessageBox.Show("Данные успешно сохранены!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
                    }
                }
            }
            ClearTextBoxes();

        }

        private void ClearTextBoxes()
        {
            // Очищаем все TextBox
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    (control as TextBox).Clear();
                }
            }
        }

        private void buttonShowResult_SEA_Click(object sender, EventArgs e)
        {
            // Проверяем, сохранение пути к файлу
            if (string.IsNullOrEmpty(savedFilePath))
            {
                MessageBox.Show("Пожалуйста, сначала сохраните данные.");
                return;
            }

            try
            {
                // подсчет строк в файле
                int lineCount = DataService.CountCsvLines(savedFilePath);

                // Выводим кол-во людей
                textBoxEmployees_SEA.Text = lineCount.ToString();

                // Открываем файл для чтения
                using (StreamReader sr = new StreamReader(savedFilePath, Encoding.UTF8))
                {
                    DataTable dataTable = new DataTable();

                    // Чтение заголовков из файла и добавляем столбцы в DataTable
                    string[] headers = sr.ReadLine().Split(';');
                    foreach (string header in headers)
                    {
                        dataTable.Columns.Add(header);
                    }

                    // Очистка существующих строк в DataTable
                    dataTable.Clear();

                    // Чтение заголовков из файла и добавляем строки в DataTable
                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(';');
                        dataTable.Rows.Add(rows);
                    }

                    // Заполнить DataGridView данными из DataTable
                    dataGridViewResult_SEA.DataSource = dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных из файла: {ex.Message}");
            }
        }

        private void buttonHelp_SEA_Click(object sender, EventArgs e)
        {
            Formhelp_SEA formHelp = new Formhelp_SEA();
            formHelp.ShowDialog();
        }

        private DataTable ReadDataInFile(string filePath)
        {
            DataTable dataTable = new DataTable();

            try
            {
                // Чтение всех строк из файла
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length > 0)
                {
                    // Разделение заголовков столбцов
                    string[] headers = lines[0].Split(';');
                    foreach (string header in headers)
                    {
                        dataTable.Columns.Add(header);
                    }

                    // Пропуск заголовков и чтение остальных данных
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] fields = lines[i].Split(';');
                        dataTable.Rows.Add(fields);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении данных из файла: {ex.Message}");
            }

            return dataTable;
        }

        private void buttonFindFile_SEA_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(savedFilePath))
            {
                string searchText = textBoxFind_SEA.Text;

                DataTable searchResult = SearchInFile(searchText, savedFilePath);

                dataGridViewResult_SEA.DataSource = searchResult;
            }
            else
            {
                MessageBox.Show("Выберите файл перед выполнением поиска.");
            }
        }
        private DataTable SearchInFile(string searchText, string filePath)
        {
            DataTable originalDataTable = ReadDataInFile(filePath);

            if (originalDataTable != null)
            {
                // Добавление временного столбца для хранения значения, показывающего, содержится ли искомый текст в строке
                originalDataTable.Columns.Add("__IsSearchResult", typeof(bool));

                // поиск в DataTable
                foreach (DataRow row in originalDataTable.Rows)
                {
                    bool found = false;

                    // Перебор столбцов оригинальной строки
                    foreach (DataColumn column in originalDataTable.Columns)
                    {
                        // Получаем значение из оригинальной строки
                        string cellValue = row[column].ToString();

                        // Если значение содержит искомую подстроку, устанавливаем флаг found в true
                        if (cellValue.Contains(searchText))
                        {
                            found = true;
                            break;
                        }
                    }

                    // Устанавливает значение временного столбца
                    row["__IsSearchResult"] = found;
                }

                // Сортирует по временному столбцу
                originalDataTable.DefaultView.Sort = "__IsSearchResult DESC";

                // Создает новую таблицу с теми же столбцами, что и у оригинальной таблицы
                DataTable searchResult = originalDataTable.Clone();

                // Копирует строки из отсортированного представления
                foreach (DataRowView rowView in originalDataTable.DefaultView)
                {
                    DataRow row = searchResult.NewRow();
                    row.ItemArray = rowView.Row.ItemArray;
                    searchResult.Rows.Add(row);
                }

                // Удаляет временный столбец
                searchResult.Columns.Remove("__IsSearchResult");

                return searchResult;
            }
            else
            {
                MessageBox.Show("Не удалось прочитать данные из файла.");
                return null;
            }
        }

        private DataTable ReadDataFromFile(string filePath)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string[] headers = reader.ReadLine().Split(';');

                    foreach (string header in headers)
                    {
                        dataTable.Columns.Add(header.Trim());
                    }

                    while (!reader.EndOfStream)
                    {
                        string[] values = reader.ReadLine().Split(';');
                        DataRow row = dataTable.NewRow();

                        for (int i = 0; i < values.Length; i++)
                        {
                            row[i] = values[i].Trim();
                        }

                        dataTable.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении данных из файла: {ex.Message}");
                return null;
            }

            return dataTable;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверяет, что путь к файлу сохранения установлен
                if (string.IsNullOrEmpty(savedFilePath))
                {
                    MessageBox.Show("Пожалуйста, укажите путь для сохранения данных.");
                    return;
                }

                // Получает данные из dataGridView и формирует строку для сохранения
                StringBuilder dataToSave = new StringBuilder();

                // Заголовки столбцов
                for (int i = 0; i < dataGridViewResult_SEA.Columns.Count; i++)
                {
                    dataToSave.Append(dataGridViewResult_SEA.Columns[i].HeaderText);
                    if (i < dataGridViewResult_SEA.Columns.Count - 1)
                    {
                        dataToSave.Append(";");
                    }
                }
                dataToSave.AppendLine();

                // Синхронизирует изменения из DataGridView с DataTable
                dataGridViewResult_SEA.EndEdit();
                (dataGridViewResult_SEA.DataSource as DataTable)?.AcceptChanges();

                // Данные строк
                foreach (DataRowView rowView in (dataGridViewResult_SEA.DataSource as DataTable)?.DefaultView)
                {
                    DataRow row = rowView.Row;
                    for (int i = 0; i < row.ItemArray.Length; i++)
                    {
                        dataToSave.Append(row[i]);
                        if (i < row.ItemArray.Length - 1)
                        {
                            dataToSave.Append(";");
                        }
                    }
                    dataToSave.AppendLine();
                }

                // Сохраняет данные в указанный файл
                File.WriteAllText(savedFilePath, dataToSave.ToString(), Encoding.UTF8);

                MessageBox.Show("Данные успешно сохранены в файл.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
            }
        }
        private bool IsRowEmpty(DataGridViewRow row)
        {
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.Value != null && cell.Value.ToString() != string.Empty)
                {
                    return false;
                }
            }
            return true;
        }

        private void textBoxPhoneNuber_SEA_TextChanged(object sender, EventArgs e)
        {
            // Получает текущий текст из текстового поля
            string currentText = textBoxPhoneNuber_SEA.Text;

            // Проверяет, содержит ли текст буквы
            if (currentText.Any(char.IsLetter))
            {
                MessageBox.Show("Пожалуйста, вводите только цифры.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxPhoneNuber_SEA.Clear();
                return;
            }

            // Создает новую строку, оставляя только цифры и управляющие символы
            string filteredText = new string(currentText.Where(c => char.IsDigit(c) || char.IsControl(c)).ToArray());

            // Если текст изменился, обновляем текстовое поле
            if (currentText != filteredText)
            {
                textBoxPhoneNuber_SEA.Text = filteredText;

                // Устанавливает курсор в конец текста
                textBoxPhoneNuber_SEA.SelectionStart = filteredText.Length;
            }
        }

        private void textBoxSalarry_SEA_TextChanged(object sender, EventArgs e)
        {
            // Получает текущий текст из текстового поля
            string currentText = textBoxSalarry_SEA.Text;

            // Создает новую строку, оставляя только цифры и управляющие символы
            string filteredText = new string(currentText.Where(c => char.IsDigit(c) || char.IsControl(c)).ToArray());

            // Преобразует текст в числовое значение
            if (decimal.TryParse(filteredText, out decimal salary))
            {
                // Оставляет только числа в текстовом поле
                textBoxSalarry_SEA.Text = filteredText;
            }
            else
            {
                // Если текст не может быть преобразован в число, очищаем поле
                textBoxSalarry_SEA.Text = string.Empty;
            }

            // Устанавливает курсор в конец текста
            textBoxSalarry_SEA.SelectionStart = textBoxSalarry_SEA.Text.Length;
        }

        private void textBoxDateOfBirth_SEA_TextChanged(object sender, EventArgs e)
        {
            // Получает текущий текст из текстового поля
            string currentText = textBoxDateOfBirth_SEA.Text;

            // Проверяет, соответствует ли текст формату "dd.MM.yyyy"
            if (currentText.Length == 10 && !IsValidDateFormat(currentText))
            {
                
                MessageBox.Show("Пожалуйста, введите дату в формате dd.MM.yyyy.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxDateOfBirth_SEA.Clear();
                return;
            }
        }
        private bool IsValidDateFormat(string dateText)
        {
            DateTime result;
            return DateTime.TryParseExact(dateText, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        private void buttonFilter_SEA_Click(object sender, EventArgs e)
        {
            // Проверяет, выбрана ли сортировка по зп
            if (comboBoxFilter_SEA.SelectedItem != null && comboBoxFilter_SEA.SelectedItem.ToString() == "Зарплата")
            {
                // Проводит сортировку по зп
                SortDataTableBySalary();
            }
            else
            {
                
            }
        }
            //Сортировка по зп
        private void SortDataTableBySalary()
        {
                if (dataGridViewResult_SEA.DataSource is DataTable dataTable)
                {
                    string columnNameOfSalaryColumn = "Salarry";

                    // Проверка существования столбца в DataTable
                    if (dataTable.Columns.Contains(columnNameOfSalaryColumn))
                    {
                        // Создание нового столбца с типом данных Decimal
                        DataColumn newColumn = new DataColumn(columnNameOfSalaryColumn + "_Decimal", typeof(decimal));

                        // Получение индекса столбца "Зарплата"
                        int columnIndex = dataTable.Columns.IndexOf(columnNameOfSalaryColumn);

                        // Добавление нового столбца в DataTable
                        dataTable.Columns.Add(newColumn);

                        // Заполнение нового столбца данными из старого столбца
                        foreach (DataRow row in dataTable.Rows)
                        {
                            // Парсинг данных из старого столбца и запись в новый
                            decimal salaryValue;
                            if (decimal.TryParse(row[columnNameOfSalaryColumn].ToString(), out salaryValue))
                            {
                                row[newColumn] = salaryValue;
                            }
                            else
                            {
                                // Обработка некорректных данных, например, установка значения по умолчанию
                                row[newColumn] = 0;
                            }
                        }

                        // Удаление старого столбца
                        dataTable.Columns.Remove(columnNameOfSalaryColumn);

                        // Установка позиции нового столбца на место старого
                        newColumn.SetOrdinal(columnIndex);

                        // Переименование нового столбца обратно на "Зарплата"
                        newColumn.ColumnName = columnNameOfSalaryColumn;

                        // Получение данных из DataTable
                        DataView dataView = new DataView(dataTable);

                        // Сортировка данных по 5-му столбцу
                        dataView.Sort = $"{columnNameOfSalaryColumn} ASC";

                        // Обновление источника данных в DataGridView
                        dataGridViewResult_SEA.DataSource = dataView.ToTable();


                    }
                    else
                    {
                        MessageBox.Show($"Столбец {columnNameOfSalaryColumn} не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
        }

        private void buttonAbout_SEA_Click(object sender, EventArgs e)
        {
                FormAbout_SEA formAbout = new FormAbout_SEA();
                formAbout.ShowDialog();
        }
        
    }
}




