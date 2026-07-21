using System;
using System.Collections.Generic;
using System.Linq;

namespace ZigdarkS.ProjectB.Infrastructure.Console
{
    public class ConsoleCommand
    {
        public string Name { get; }
        public string Description { get; }
        public Action<string[]> Action { get; }

        public ConsoleCommand(string name, string description, Action<string[]> action)
        {
            Name = name.ToLower();
            Description = description;
            Action = action;
        }
    }

    public class ConsoleService
    {
        private readonly Dictionary<string, ConsoleCommand> _commands = new();

        public ConsoleService()
        {
            // Регистрируем базовые команды
            RegisterCommand("clear", "Очистить консоль", (args) => OnClear?.Invoke());
            RegisterCommand("help", "Показать все команды", (args) => LogAllCommands());
        }

        public event Action<string> OnLogReceived;
        public event Action OnClear;

        public void RegisterCommand(string name, string description, Action<string[]> action)
        {
            var cmd = new ConsoleCommand(name, description, action);
            _commands[cmd.Name] = cmd;
        }

        public void ExecuteCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            OnLogReceived?.Invoke($"] {input}");
            
            // Парсим строку на команду и аргументы
            string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string commandName = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();

            if (_commands.TryGetValue(commandName, out var command))
            {
                try { command.Action(args); }
                catch (Exception ex) { OnLogReceived?.Invoke($"Ошибка выполнения: {ex.Message}"); }
            }
            else
            {
                OnLogReceived?.Invoke($"Unknown command: {commandName}");
            }
        }

        // Возвращает список команд, начинающихся на определенный текст (для автокомплита)
        public List<ConsoleCommand> GetSuggestions(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return new List<ConsoleCommand>();
            return _commands.Values
                .Where(c => c.Name.StartsWith(prefix.ToLower()))
                .ToList();
        }

        private void LogAllCommands()
        {
            foreach (var cmd in _commands.Values)
            {
                OnLogReceived?.Invoke($"{cmd.Name} — {cmd.Description}");
            }
        }
    }
}