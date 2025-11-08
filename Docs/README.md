## Структура
EduSuite.sln
Launcher/
Modules/
	FileManager.Console/
	Encryptor.Console/
	Sync.Console/
Docs/
	README.md



## Сборка и запуск (VS)
1. Откройте `EduSuite.sln` в Visual Studio 2022+.
2. Установите `Launcher` как Startup Project.
3. Сборка: **Build → Build Solution**.
4. Запуск лаунчера: **Debug → Start Without Debugging**.

---

# Ключевые места кода 
  # Launcher
  `Program.cs` (лаунчер):  
  `ApplicationConfiguration.Initialize();` — включает современные настройки WinForms в .NET 8.  
  `Application.Run(new MainForm());` — стартует главное окно.

- `MainForm`:
  - Создаём `ListView` в режиме `Details` → три колонки (Название/Описание/Статус).
  - Кнопка **Запустить** отключена, пока не выбран модуль.
  - `LoadModules()` берёт данные из `ModuleRegistry` и заполняет список.
  - Обработчик кнопки пока показывает **MessageBox** (заглушка). Настоящий запуск процесса добавим позже.

- `ModuleRegistry`: статический список модулей. Пути указаны **относительно дистрибутива**:  
  `.\Modules\<Id>\<ExeName>.exe` — так мы легко перейдём к пост-build копированию/паблишу.

---

