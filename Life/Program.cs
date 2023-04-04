using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.IO;

namespace cli_life
{
    public class GameSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int CellSize { get; set; }
        public double LiveDensity { get; set; }
    }
    public class Cell
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        public bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public void ClassifyElements()
        {
            int rows = Rows;
            int cols = Columns;
            bool[,] classification = new bool[rows, cols];

            bool[,] block = new bool[2, 2] { { true, true }, { true, true } };

            for (int r = 0; r < rows - block.GetLength(0); r++)
            {
                for (int c = 0; c < cols - block.GetLength(1); c++)
                {
                    bool match = true;
                    for (int br = 0; br < block.GetLength(0); br++)
                    {
                        for (int bc = 0; bc < block.GetLength(1); bc++)
                        {
                            if (Cells[r + br, c + bc].IsAlive != block[br, bc])
                            {
                                match = false;
                                break;
                            }
                        }
                        if (!match)
                            break;
                    }
                    if (match)
                    {
                        for (int br = 0; br < block.GetLength(0); br++)
                        {
                            for (int bc = 0; bc < block.GetLength(1); bc++)
                            {
                                classification[r + br, c + bc] = true;
                            }
                        }
                    }
                }
            }

            for (int r = 0; r < rows; r++)
            {
                StringBuilder sb = new StringBuilder();
                for (int c = 0; c < cols; c++)
                {
                    if (classification[r, c])
                    {
                        sb.Append("*");
                    }
                    else
                    {
                        sb.Append(Cells[r, c].IsAlive ? "O" : ".");
                    }
                }
                Console.WriteLine(sb.ToString());
            }
        }
        public int GetTotalElementCount()
        {
            return Cells.Length + (Cells.GetLength(0) - 1) * (Cells.GetLength(1) - 1);
        }
        public void SaveToFile(string fileName)
        {
            var lines = new List<string>();
            for (int row = 0; row < Rows; row++)
            {
                var line = new StringBuilder();
                for (int col = 0; col < Columns; col++)
                {
                    var cell = Cells[col, row];
                    line.Append(cell.IsAlive ? '*' : ' ');
                }
                lines.Add(line.ToString());
            }
            File.WriteAllLines(fileName, lines);
        }

        public void LoadFromFile(string fileName)
        {
            var lines = File.ReadAllLines(fileName);
            for (int row = 0; row < Rows; row++)
            {
                var line = lines[row];
                for (int col = 0; col < Columns; col++)
                {
                    var cell = Cells[col, row];
                    cell.IsAlive = line[col] == '*';
                }
            }
        }
        public void LoadColonyFromFile(string fileName, int x, int y)
        {
            var lines = File.ReadAllLines(fileName);
            for (int row = 0; row < lines.Length; row++)
            {
                var line = lines[row];
                for (int col = 0; col < line.Length; col++)
                {
                    var cell = Cells[x + col, y + row];
                    cell.IsAlive = line[col] == '*';
                }
            }
        }

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
        public int GetStablePhaseTime(int maxIterations)
        {
            int sumIterations = 0;
            int countIterations = 0;

            while (countIterations < maxIterations)
            {
                RunSimulation();
                int stablePhaseTime = CalculateStablePhaseTime();

                if (stablePhaseTime > 0)
                {
                    sumIterations += stablePhaseTime;
                    countIterations++;
                }
            }

            return countIterations > 0 ? sumIterations / countIterations : 0;
        }

        private void RunSimulation()
        {
            var allCells = Cells.Cast<Cell>();
            foreach (var cell in allCells)
            {
                cell.neighbors.Clear();
                int x = cell.ColumnIndex;
                int y = cell.RowIndex;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && ny >= 0 && nx < Columns && ny < Rows)
                        {
                            cell.neighbors.Add(Cells[nx, ny]);
                        }
                    }
                }
            }

            allCells.ToList().ForEach(c => c.DetermineNextLiveState());
            allCells.ToList().ForEach(c => c.Advance());
        }

        private int CalculateStablePhaseTime()
        {
            var boardState = JsonConvert.SerializeObject(Cells);
            int iterations = 0;

            while (true)
            {
                RunSimulation();
                string newBoardState = JsonConvert.SerializeObject(Cells);

                if (newBoardState == boardState)
                {
                    return iterations;
                }

                boardState = newBoardState;
                iterations++;
            }
        }
        public int GetSymmetricElementCount()
        {
            int count = 0;
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (Cells[r, c].IsAlive && Cells[Rows - 1 - r, Columns - 1 - c].IsAlive)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public void ExploreSymmetry(int numGenerations)
        {
            var board1 = this.Clone();
            var board2 = this.Clone();
            for (int i = 0; i < numGenerations; i++)
            {
                board1.Advance();
                board2.Advance();
                board2.Mirror();
                if (board1.Equals(board2))
                {
                    Console.WriteLine($"System is symmetric after {i + 1} generations.");
                    return;
                }
            }
            Console.WriteLine($"System is asymmetric after {numGenerations} generations.");
        }

        private void Mirror()
        {
            int rows = Rows;
            int cols = Columns;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols / 2; c++)
                {
                    bool temp = Cells[r, c].IsAlive;
                    Cells[r, c].IsAlive = Cells[r, cols - 1 - c].IsAlive;
                    Cells[r, cols - 1 - c].IsAlive = temp;
                }
            }
        }

        public Board Clone()
        {
            var clone = new Board(Width, Height, CellSize);
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    clone.Cells[x, y].IsAlive = Cells[x, y].IsAlive;
            return clone;
        }
    }
    class Program
    {
        static Board board;
        static private void Reset()
        {
            string settingsJson = File.ReadAllText("settings.json");
            GameSettings settings = JsonConvert.DeserializeObject<GameSettings>(settingsJson);

            board = new Board(
                width: settings.Width,
                height: settings.Height,
                cellSize: settings.CellSize,
                liveDensity: settings.LiveDensity);

            board.LoadColonyFromFile("colony5.txt", 10, 10);
        }
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)   
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }
        static void Main(string[] args)
        {
            Reset();

            bool shouldContinue = true;
            while (shouldContinue)
            {
                Console.Clear();
                Render();
                board.Advance();

                Console.WriteLine("S - save, L - load, or any other key to continue.");
                var key = Console.ReadKey().Key;
                if (key == ConsoleKey.S)
                {
                    Console.WriteLine("\nSaving to file...");
                    board.SaveToFile("game_state.txt");
                }
                else if (key == ConsoleKey.L)
                {
                    Console.WriteLine("\nLoading from file...");
                    board.LoadFromFile("game_state.txt");
                }
                else
                {
                    shouldContinue = true;
                }

                Thread.Sleep(1000);
            }
        }
    }
}
