using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

/// <summary>
/// Если вдруг, кто-то не знал (я вот только недавно узнал) namespace нужно для того, чтобы мы могли без конфликтов испльзовать функции и методы из этого файла в других. Вот все "using" это запросы к namespace других файлов.
/// На примере "public class MainForm : Form" Form является именем (в данном случае классом) namespace "System.Windows.Forms" и чтобы программа понмала о каком Form идёт речь и используется using,
/// а чтобы программа смогла найти System.Windows.Forms используется namespace.
/// </summary>

namespace Launcher
{
    public class MainForm : Form
    {
        // Короче, Меченый, эти переменные являются ссылками на UI элементы из "System.Windows.Forms". Соответственно: список модулей, кнопка Запуска, информационная строка.
        private ListView _lvModules;
        private Button _btnLaunch;
        private Label _lblInfo;

        // Ну чё тут рассказывать. Ты же гоу про экстрим мастер по CSS И ГУРУ Тильды, так что понимаешь что тут написано. А если без приколов, то это настройка окна, которое видит пользователь.
        private void InitializeComponent()
        {
            Text = "Лаунчер пу-пу-пу";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 800;
            Height = 500;
            MinimizeBox = true;
            MaximizeBox = true;

            _lvModules = new ListView
            {
                Dock = DockStyle.Top,
                Height = 360,
                FullRowSelect = true,
                MultiSelect = false,
                View = View.Details
            };

            _lvModules.Columns.Add("Название", 220);
            _lvModules.Columns.Add("Описание", 440);
            _lvModules.Columns.Add("Статус", 100);

            _btnLaunch = new Button
            {
                Text = "Запустить",
                Dock = DockStyle.Bottom,
                Height = 44,
                Enabled = false
            };

            _btnLaunch.Click += BtnLaunch_Click;

            _lblInfo = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                Text = "Выберите модуль и нажмите «Запустить».",
                TextAlign = ContentAlignment.MiddleLeft
            };

            Controls.Add(_lvModules);
            Controls.Add(_lblInfo);
            Controls.Add(_btnLaunch);

        }
        /// <summary>
        /// В общем, "SelectedIndexChanged" это событие на которое мы подписываемся (выглядит это как +=) и событие срабатывает каждый раз, когда меняется выделение (то есть выбор модуля).
        /// В обработчике у нас есть одна очень простая строка. Наша конпка становится активна только в том случае, когда хоть один элемент выбран (то есть если пользователь не кликнул ни на один из модулей, то кнопка будет не активна.)
        /// В противном случае, кнопка активируется и всё у нас прекрасно.
        /// </summary>>
        private void SelectedIndexChanged(object? sender, EventArgs e)
        {
            _btnLaunch.Enabled = _lvModules.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Вот здесь мы загружаем всю информацию о модулях (то есть о программах из заданий). Чистим список, на случай повторной активации.
        /// Потом подключаем коллекцию с информацией о модулях. А именно: название, описание и статус. После юзаем tag, который просто запоминает последний модуль на который кликнул юзер 
        /// (ну вообще не совсем так, но в нашем случае это так), после "_lvModules.Items.Add(item);" добавляет строку в список.
        /// </summary>
        private void LoadModules()
        {
            _lvModules.Items.Clear();

            foreach (var m in ModuleRegistry.GetAll())
            {
                var item = new ListViewItem(new[]
                {
                    m.Name,
                    m.Description,
                    "Готов"
                })
                {
                    Tag = m
                };

                _lvModules.Items.Add(item);
            }
        }

        private static void SetStatus(ListViewItem item, string status)
        {
            if (item.SubItems.Count == 3)
            {
                item.SubItems[2].Text = status;
            }
        }
        // Здесь как раз и происходит запуск модуля.
        private void BtnLaunch_Click(object? sender, EventArgs e)
        {
            if (_lvModules.SelectedItems.Count == 0) // Это чисто защита от дурака, у нас и так кнопка не активна (ну мало ли)
                return;
            var item = _lvModules.SelectedItems[0];

            if (item.Tag is not ModuleDescriptor m)
            {
                MessageBox.Show(
                    "Внутренняя ошибка: не удалось получить информацию о модуле", "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            try
            {
                _btnLaunch.Enabled = false;
                SetStatus(item, "Запуск...");
                var baseDir = AppContext.BaseDirectory;
                Process proc = ProcessRunner.StartFor(m, baseDir);
                SetStatus(item, $"Запущен (PID {proc.Id})");

                proc.EnableRaisingEvents = true;
                proc.Exited += (_, __) =>
                {
                    BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            int code = proc.ExitCode;
                            SetStatus(item, $"Завершён (код {code})");
                        }
                        catch
                        {
                            SetStatus(item, "Завершён");
                        }
                        finally
                        {
                            _btnLaunch.Enabled = _lvModules.SelectedItems.Count > 0;
                        }
                    }));
                };
            }
            catch (FileNotFoundException ex)
            {
                SetStatus(item, "Не найден");
                MessageBox.Show(
                    ex.Message,
                    "Файл не найден",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _btnLaunch.Enabled = _lvModules.SelectedItems.Count > 0;
            }
            catch (Exception ex)
            {
                SetStatus(item, "Ошибка");
                MessageBox.Show(
                    $"Не удалось запустить модуль:\n\n{m.Name}\n\nПричина: {ex.Message}",
                    "Ошибка запуска",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _btnLaunch.Enabled = _lvModules.SelectedItems.Count > 0;
            }
        }

        // Сначала генерируем UI "InitializeComponent()", после наполняем его данными "LoadModules()". Проще говоря, это конструктор формы.
        public MainForm()
        {
            InitializeComponent();
            LoadModules();
            _lvModules.SelectedIndexChanged += SelectedIndexChanged;
        }
    }
}
