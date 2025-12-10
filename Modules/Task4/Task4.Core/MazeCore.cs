using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Task4.Core
{
    public readonly record struct CellPosition(int X, int Y);

    public enum MazeCellType
    {
        Road = 0,
        Wall = 1,
        Entrance = 5,
        Exit = 6
    }

    public sealed class MazeGrid
    {
        private readonly int[,] _cells;

        public int Width { get; }

        public int Height { get; }

        public CellPosition Entrance { get; set; }

        public CellPosition Exit { get; set; }

        public MazeGrid(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Ширина должна быть положительным числом.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Высота должна быть положительным числом.");
            }

            Width = width;
            Height = height;
            _cells = new int[height, width];
            Entrance = new CellPosition(0, 0);
            Exit = new CellPosition(width - 1, height - 1);
        }

        public int GetCell(int x, int y)
        {
            if (!IsInside(x, y))
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Координаты выходят за границы массива.");
            }

            return _cells[y, x];
        }

        public void SetCell(int x, int y, MazeCellType cellType)
        {
            if (!IsInside(x, y))
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Координаты выходят за границы массива.");
            }

            _cells[y, x] = (int)cellType;
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsWalkable(int x, int y)
        {
            if (!IsInside(x, y))
            {
                return false;
            }

            int value = _cells[y, x];

            return value == (int)MazeCellType.Road
                   || value == (int)MazeCellType.Entrance
                   || value == (int)MazeCellType.Exit;
        }

        public int[,] Snapshot()
        {
            var copy = new int[Height, Width];
            Array.Copy(_cells, copy, _cells.Length);
            return copy;
        }
    }

    public static class MazeTextSerializer
    {
        public static string SerializeToText(MazeGrid maze)
        {
            if (maze == null)
            {
                throw new ArgumentNullException(nameof(maze));
            }

            var builder = new StringBuilder();

            for (int y = 0; y < maze.Height; y++)
            {
                for (int x = 0; x < maze.Width; x++)
                {
                    int value = maze.GetCell(x, y);
                    builder.Append(value);

                    if (x < maze.Width - 1)
                    {
                        builder.Append(' ');
                    }
                }

                if (y < maze.Height - 1)
                {
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        public static MazeGrid ParseFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Текст лабиринта пуст.", nameof(text));
            }

            string normalized = text
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            string[] rawLines = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var lines = new List<string>();

            foreach (string rawLine in rawLines)
            {
                string trimmedLine = rawLine.Trim();

                if (trimmedLine.Length > 0)
                {
                    lines.Add(trimmedLine);
                }
            }

            if (lines.Count == 0)
            {
                throw new FormatException("Не удалось найти ни одной строки лабиринта.");
            }

            var rows = new List<int[]>();
            int? expectedWidth = null;

            CellPosition? entrance = null;
            CellPosition? exit = null;

            for (int y = 0; y < lines.Count; y++)
            {
                string line = lines[y];

                string[] tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0)
                {
                    continue;
                }

                if (expectedWidth == null)
                {
                    expectedWidth = tokens.Length;
                }
                else if (tokens.Length != expectedWidth.Value)
                {
                    throw new FormatException("Все строки лабиринта должны иметь одинаковое число столбцов.");
                }

                int[] rowValues = new int[tokens.Length];

                for (int x = 0; x < tokens.Length; x++)
                {
                    if (!int.TryParse(tokens[x], out int cell))
                    {
                        throw new FormatException($"Не удалось разобрать число \"{tokens[x]}\" в строке {y + 1}.");
                    }

                    if (cell != (int)MazeCellType.Road
                        && cell != (int)MazeCellType.Wall
                        && cell != (int)MazeCellType.Entrance
                        && cell != (int)MazeCellType.Exit)
                    {
                        throw new FormatException($"Недопустимое значение клетки {cell} в строке {y + 1}. Разрешены только 0, 1, 5, 6.");
                    }

                    rowValues[x] = cell;

                    if (cell == (int)MazeCellType.Entrance && entrance == null)
                    {
                        entrance = new CellPosition(x, y);
                    }

                    if (cell == (int)MazeCellType.Exit && exit == null)
                    {
                        exit = new CellPosition(x, y);
                    }
                }

                rows.Add(rowValues);
            }

            if (expectedWidth == null || rows.Count == 0)
            {
                throw new FormatException("Лабиринт пуст.");
            }

            if (entrance == null)
            {
                throw new FormatException("В лабиринте не найден вход (клетка со значением 5).");
            }

            if (exit == null)
            {
                throw new FormatException("В лабиринте не найден выход (клетка со значением 6).");
            }

            int width = expectedWidth.Value;
            int height = rows.Count;

            var maze = new MazeGrid(width, height);

            for (int y = 0; y < height; y++)
            {
                int[] rowValues = rows[y];

                for (int x = 0; x < width; x++)
                {
                    maze.SetCell(x, y, (MazeCellType)rowValues[x]);
                }
            }

            maze.Entrance = entrance.Value;
            maze.Exit = exit.Value;

            return maze;
        }
    }

    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class MazePathFinderService
    {
        public static IReadOnlyList<MoveDirection> FindPath(MazeGrid maze)
        {
            if (maze == null)
            {
                throw new ArgumentNullException(nameof(maze));
            }

            var start = maze.Entrance;
            var goal = maze.Exit;

            var queue = new Queue<CellPosition>();
            var visited = new HashSet<CellPosition>();
            var parent = new Dictionary<CellPosition, CellPosition>();
            var moveFromParent = new Dictionary<CellPosition, MoveDirection>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current.Equals(goal))
                {
                    break;
                }

                foreach (var neighborInfo in GetNeighbors(maze, current))
                {
                    var neighbor = neighborInfo.Position;
                    var direction = neighborInfo.Direction;

                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    visited.Add(neighbor);
                    parent[neighbor] = current;
                    moveFromParent[neighbor] = direction;
                    queue.Enqueue(neighbor);
                }
            }

            if (!visited.Contains(goal))
            {
                return Array.Empty<MoveDirection>();
            }

            var reversed = new List<MoveDirection>();

            var cursor = goal;

            while (!cursor.Equals(start))
            {
                if (!moveFromParent.TryGetValue(cursor, out var direction))
                {
                    return Array.Empty<MoveDirection>();
                }

                reversed.Add(direction);

                if (!parent.TryGetValue(cursor, out var previous))
                {
                    return Array.Empty<MoveDirection>();
                }

                cursor = previous;
            }

            reversed.Reverse();

            return reversed;
        }

        private static IEnumerable<(CellPosition Position, MoveDirection Direction)> GetNeighbors(
            MazeGrid maze,
            CellPosition current)
        {
            var candidates = new (int OffsetX, int OffsetY, MoveDirection Direction)[]
            {
                (0, -1, MoveDirection.Up),
                (0, 1, MoveDirection.Down),
                (-1, 0, MoveDirection.Left),
                (1, 0, MoveDirection.Right)
            };

            foreach (var candidate in candidates)
            {
                int newX = current.X + candidate.OffsetX;
                int newY = current.Y + candidate.OffsetY;

                if (maze.IsWalkable(newX, newY))
                {
                    yield return (new CellPosition(newX, newY), candidate.Direction);
                }
            }
        }
    }

    public static class MazePathFormatter
    {
        public static string FormatDirections(IReadOnlyList<MoveDirection> directions)
        {
            if (directions == null)
            {
                throw new ArgumentNullException(nameof(directions));
            }

            if (directions.Count == 0)
            {
                return "Путь не найден.";
            }

            var builder = new StringBuilder();

            for (int i = 0; i < directions.Count; i++)
            {
                builder.Append(DirectionToRussian(directions[i]));

                if (i < directions.Count - 1)
                {
                    builder.Append(", ");
                }
            }

            return builder.ToString();
        }

        public static string DirectionToRussian(MoveDirection direction)
        {
            return direction switch
            {
                MoveDirection.Up => "вверх",
                MoveDirection.Down => "вниз",
                MoveDirection.Left => "влево",
                MoveDirection.Right => "вправо",
                _ => direction.ToString()
            };
        }
    }

    public static class MazePathCommandsParser
    {
        public static IReadOnlyList<MoveDirection> ParseCommands(string commandsText)
        {
            if (string.IsNullOrWhiteSpace(commandsText))
            {
                throw new ArgumentException("Строка команд пуста.", nameof(commandsText));
            }

            char[] separators = { ',', ';', '\n', '\r' };

            string normalized = commandsText
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            string[] parts = normalized.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            var directions = new List<MoveDirection>();

            for (int i = 0; i < parts.Length; i++)
            {
                string token = parts[i].Trim();

                if (token.Length == 0)
                {
                    continue;
                }

                MoveDirection direction = ParseSingleDirection(token, i);
                directions.Add(direction);
            }

            if (directions.Count == 0)
            {
                throw new FormatException("Не удалось разобрать ни одной команды движения.");
            }

            return directions;
        }

        private static MoveDirection ParseSingleDirection(string token, int index)
        {
            string lower = token.ToLower(new CultureInfo("ru-RU"));

            return lower switch
            {
                "вверх" or "up" => MoveDirection.Up,
                "вниз" or "down" => MoveDirection.Down,
                "влево" or "left" => MoveDirection.Left,
                "вправо" or "right" => MoveDirection.Right,
                _ => throw new FormatException($"Неизвестная команда \"{token}\" (позиция {index + 1}). Допустимо: вверх, вниз, влево, вправо.")
            };
        }
    }

    public sealed class MazePathValidationResult
    {
        public bool IsSuccessful { get; }

        public string Message { get; }

        public int StepsTaken { get; }

        public CellPosition FinalPosition { get; }

        public MazePathValidationResult(
            bool isSuccessful,
            string message,
            int stepsTaken,
            CellPosition finalPosition)
        {
            IsSuccessful = isSuccessful;
            Message = message;
            StepsTaken = stepsTaken;
            FinalPosition = finalPosition;
        }
    }

    public static class MazePathValidatorService
    {
        public static MazePathValidationResult ValidatePath(
            MazeGrid maze,
            IReadOnlyList<MoveDirection> moves)
        {
            if (maze == null)
            {
                throw new ArgumentNullException(nameof(maze));
            }

            if (moves == null)
            {
                throw new ArgumentNullException(nameof(moves));
            }

            var current = maze.Entrance;

            for (int stepIndex = 0; stepIndex < moves.Count; stepIndex++)
            {
                MoveDirection direction = moves[stepIndex];

                CellPosition next = ApplyMove(current, direction);

                if (!maze.IsInside(next.X, next.Y))
                {
                    string message = $"На шаге {stepIndex + 1} вы вышли за границы лабиринта.";
                    return new MazePathValidationResult(false, message, stepIndex, current);
                }

                int cellValue = maze.GetCell(next.X, next.Y);

                if (cellValue == (int)MazeCellType.Wall)
                {
                    string message = $"На шаге {stepIndex + 1} вы врезались в стену.";
                    return new MazePathValidationResult(false, message, stepIndex + 1, next);
                }

                current = next;
            }

            int finalValue = maze.GetCell(current.X, current.Y);

            if (finalValue == (int)MazeCellType.Exit)
            {
                string message = $"Успех! Маршрут приводит к выходу из лабиринта за {moves.Count} шаг(ов).";
                return new MazePathValidationResult(true, message, moves.Count, current);
            }

            string failMessage = "Маршрут не приводит к выходу из лабиринта. " +
                                 $"После {moves.Count} шаг(ов) вы остановились в точке ({current.X}, {current.Y}).";

            return new MazePathValidationResult(false, failMessage, moves.Count, current);
        }

        private static CellPosition ApplyMove(CellPosition position, MoveDirection direction)
        {
            return direction switch
            {
                MoveDirection.Up => new CellPosition(position.X, position.Y - 1),
                MoveDirection.Down => new CellPosition(position.X, position.Y + 1),
                MoveDirection.Left => new CellPosition(position.X - 1, position.Y),
                MoveDirection.Right => new CellPosition(position.X + 1, position.Y),
                _ => position
            };
        }
    }

    public static class MazeGeneratorService
    {
        private static readonly Random RandomGenerator = new Random();

        public static MazeGrid Generate(int requestedWidth, int requestedHeight)
        {
            int width = Math.Max(5, requestedWidth);
            int height = Math.Max(5, requestedHeight);

            width = Math.Min(width, 200);
            height = Math.Min(height, 200);

            var maze = new MazeGrid(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    maze.SetCell(x, y, MazeCellType.Wall);
                }
            }

            int innerWidth = width - 2;
            int innerHeight = height - 2;

            if (innerWidth <= 0 || innerHeight <= 0)
            {
                throw new InvalidOperationException("Слишком маленький размер лабиринта.");
            }

            int logicalWidth = innerWidth / 2;
            int logicalHeight = innerHeight / 2;

            if (logicalWidth <= 0 || logicalHeight <= 0)
            {
                throw new InvalidOperationException("Слишком маленький размер внутренней области для построения лабиринта.");
            }

            bool[,] visited = new bool[logicalWidth, logicalHeight];
            var stack = new Stack<(int X, int Y)>();

            int startX = RandomGenerator.Next(logicalWidth);
            int startY = RandomGenerator.Next(logicalHeight);

            visited[startX, startY] = true;
            stack.Push((startX, startY));
            SetLogicalCellAsRoad(maze, startX, startY);

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                var neighbors = GetUnvisitedNeighbors(
                    current.X,
                    current.Y,
                    logicalWidth,
                    logicalHeight,
                    visited);

                if (neighbors.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                int nextIndex = RandomGenerator.Next(neighbors.Count);
                var next = neighbors[nextIndex];

                CarvePassage(maze, current.X, current.Y, next.X, next.Y);

                visited[next.X, next.Y] = true;
                stack.Push(next);
            }

            EnsureColumnHasRoad(maze, 1);
            EnsureColumnHasRoad(maze, width - 2);

            int entranceY = ChooseRandomRoadYInColumn(maze, 1);
            int exitY = ChooseRandomRoadYInColumn(maze, width - 2);

            maze.SetCell(0, entranceY, MazeCellType.Entrance);
            maze.Entrance = new CellPosition(0, entranceY);

            maze.SetCell(width - 1, exitY, MazeCellType.Exit);
            maze.Exit = new CellPosition(width - 1, exitY);

            return maze;
        }

        private static void SetLogicalCellAsRoad(MazeGrid maze, int logicalX, int logicalY)
        {
            int x = 1 + logicalX * 2;
            int y = 1 + logicalY * 2;

            if (maze.IsInside(x, y))
            {
                maze.SetCell(x, y, MazeCellType.Road);
            }
        }

        private static List<(int X, int Y)> GetUnvisitedNeighbors(
            int cx,
            int cy,
            int logicalWidth,
            int logicalHeight,
            bool[,] visited)
        {
            var neighbors = new List<(int X, int Y)>();

            void TryAdd(int nx, int ny)
            {
                if (nx >= 0
                    && nx < logicalWidth
                    && ny >= 0
                    && ny < logicalHeight
                    && !visited[nx, ny])
                {
                    neighbors.Add((nx, ny));
                }
            }

            TryAdd(cx + 1, cy);
            TryAdd(cx - 1, cy);
            TryAdd(cx, cy + 1);
            TryAdd(cx, cy - 1);

            return neighbors;
        }

        private static void CarvePassage(
            MazeGrid maze,
            int cx1,
            int cy1,
            int cx2,
            int cy2)
        {
            int x1 = 1 + cx1 * 2;
            int y1 = 1 + cy1 * 2;

            int x2 = 1 + cx2 * 2;
            int y2 = 1 + cy2 * 2;

            if (maze.IsInside(x1, y1))
            {
                maze.SetCell(x1, y1, MazeCellType.Road);
            }

            if (maze.IsInside(x2, y2))
            {
                maze.SetCell(x2, y2, MazeCellType.Road);
            }

            int midX = (x1 + x2) / 2;
            int midY = (y1 + y2) / 2;

            if (maze.IsInside(midX, midY))
            {
                maze.SetCell(midX, midY, MazeCellType.Road);
            }
        }

        private static void EnsureColumnHasRoad(MazeGrid maze, int columnIndex)
        {
            int height = maze.Height;
            int width = maze.Width;

            for (int y = 1; y < height - 1; y++)
            {
                if (maze.GetCell(columnIndex, y) == (int)MazeCellType.Road)
                {
                    return;
                }
            }

            var roadCells = new List<CellPosition>();

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    if (maze.GetCell(x, y) == (int)MazeCellType.Road)
                    {
                        roadCells.Add(new CellPosition(x, y));
                    }
                }
            }

            if (roadCells.Count == 0)
            {
                return;
            }

            CellPosition chosen = roadCells[RandomGenerator.Next(roadCells.Count)];

            int step = chosen.X < columnIndex ? 1 : -1;
            int xCurrent = chosen.X;

            while (xCurrent != columnIndex)
            {
                xCurrent += step;

                if (!maze.IsInside(xCurrent, chosen.Y))
                {
                    break;
                }

                maze.SetCell(xCurrent, chosen.Y, MazeCellType.Road);
            }
        }

        private static int ChooseRandomRoadYInColumn(MazeGrid maze, int columnIndex)
        {
            int height = maze.Height;
            var candidates = new List<int>();

            for (int y = 1; y < height - 1; y++)
            {
                if (maze.GetCell(columnIndex, y) == (int)MazeCellType.Road)
                {
                    candidates.Add(y);
                }
            }

            if (candidates.Count == 0)
            {
                return height / 2;
            }

            int index = RandomGenerator.Next(candidates.Count);
            return candidates[index];
        }
    }

}
