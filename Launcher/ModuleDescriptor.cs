// Важный момент. Имя exe по умолчанию совпадает с именем проекта. Если переименуешь проект — путь надо обновить.
namespace Launcher
{
    /// <summary>
    /// Это наш шаблон (если по крутому, то это DTO. Объект для передачи данных), по кторому мы будем обрабатывать данные модулей. Штучка по имени get нужна для иммутабельности кода, чтобы нельзя было изменить данные вводимые в него.
    /// Точнее, get может только возвращать значение свойства (принимать значение). Обычно в связке с ним используют команду set (она как раз нужна, чтобы присвоить новое значение), но нам это не нужно.
    /// </summary>
    public class ModuleDescriptor
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string RelativeExePath { get; }

        public string SolutionPathProj { get; }

        public ModuleDescriptor(string id, string name, string description, string relativeExePath, string solutionPathProj)
        {
            Id = id;
            Name = name;
            Description = description;
            RelativeExePath = relativeExePath;
            SolutionPathProj = solutionPathProj;
        }
    }
}
