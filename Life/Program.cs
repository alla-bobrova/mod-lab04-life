using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ScottPlot;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Text.Json;
using System.Xml.Serialization;
using ScottPlot.Drawing.Colormaps;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
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

        public class Settings
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int CellSize { get; set; }
            public double LiveDensity { get; set; }
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

        // Чтение настроек из файла
        public static Settings ReadSettingsFromFile(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName))
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                return serializer.Deserialize(file, typeof(Settings)) as Settings;
            }
        }

        public void LoadBoardFromFile(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            for (int y = 0; y < Rows && y < lines.Length; y++)
            {
                string[] values = lines[y].Split(',');
                for (int x = 0; x < Columns && x < values.Length; x++)
                {
                    int value = int.Parse(values[x]);
                    Cells[x, y].IsAlive = (value == 1);
                }
            }
        }

        public int LiveCells()
        {
            return Cells.Cast<Cell>().Count(cell => cell.IsAlive);
        }

        public int Symmetric()
        {
            int symmetricCount = 0;

            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (Cells[x, y].IsAlive == Cells[Columns - x - 1, y].IsAlive)
                    {
                        symmetricCount++;
                    }
                }
            }

            return symmetricCount;
        }

        public void Render(Board board)
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.White;

            for (int y = 0; y < board.Rows; y++)
            {
                for (int x = 0; x < board.Columns; x++)
                {
                    if (board.Cells[x, y].IsAlive)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("*");
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(".");
                    }
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }
        }
    }


    class Program
    {
        static Board board;

        static private void Reset()
        {
            board = new Board(
                width: 80,
                height: 20,
                cellSize: 1,
                liveDensity: 0.2);
        }

        static bool IsPatternOnBoard(bool[,] board, bool[,] pattern)
        {
            int boardRows = board.GetLength(0);
            int boardCols = board.GetLength(1);
            int patternRows = pattern.GetLength(0);
            int patternCols = pattern.GetLength(1);

            for (int i = 0; i <= boardRows - patternRows; i++)
            {
                for (int j = 0; j <= boardCols - patternCols; j++)
                {
                    bool isPatternOnBoard = true;

                    for (int k = 0; k < patternRows; k++)
                    {
                        for (int l = 0; l < patternCols; l++)
                        {
                            if (board[i + k, j + l] != pattern[k, l])
                            {
                                isPatternOnBoard = false;
                                break;
                            }
                        }

                        if (!isPatternOnBoard)
                        {
                            break;
                        }
                    }

                    if (isPatternOnBoard)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static void Main(string[] args)
        {

            Console.ForegroundColor = ConsoleColor.White;
            int genCount = 0;

            Console.WriteLine("1. Параметры из файла\n2. Уже заданные\n3. Проверить на наличие элементов");
            int choice = int.Parse(Console.ReadLine());

            if (choice == 1)
            {
                var settings = Board.ReadSettingsFromFile("C:\\Users\\alla_\\Desktop\\mod-lab04-life-main\\mod-lab04-life-main\\Life\\settings.json");
                board = new Board(settings.Width, settings.Height, settings.CellSize, settings.LiveDensity);
            }

            if (choice == 2)
            {
                Reset();
            }

            if (choice == 3)
            {

                // Считываем состояние доски из файла
                string boardFilePath = "gen.txt";
                string[] boardLines = File.ReadAllLines(boardFilePath);
                bool[,] board = new bool[boardLines.Length, boardLines[0].Length];

                for (int i = 0; i < boardLines.Length; i++)
                {
                    for (int j = 0; j < boardLines[i].Length; j++)
                    {
                        board[i, j] = boardLines[i][j] == '1';
                    }
                }

                // Считываем образцы фигур из файлов и проверяем, содержатся ли они на доске
                string[] patternFilePaths = new string[] { "fig1.txt", "fig2.txt", "fig3.txt" };
                foreach (var patternFilePath in patternFilePaths)
                {
                    string[] patternLines = File.ReadAllLines(patternFilePath);
                    bool[,] pattern = new bool[patternLines.Length, patternLines[0].Length];

                    for (int i = 0; i < patternLines.Length; i++)
                    {
                        for (int j = 0; j < patternLines[i].Length; j++)
                        {
                            pattern[i, j] = patternLines[i][j] == '1';
                        }
                    }

                    if (IsPatternOnBoard(board, pattern))
                    {
                        Console.WriteLine($"Образец из файла {patternFilePath} найден на доске.");
                    }
                    else
                    {
                        Console.WriteLine($"Образец из файла {patternFilePath} не найден на доске.");
                    }
                } 
            }

            if ((choice == 1) || (choice == 2))
                {
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo name = Console.ReadKey();

                        if (name.KeyChar == 'o')
                        {
                            Console.Clear();
                            string fileName = "gen.txt";
                            board.LoadBoardFromFile(fileName);
                        }

                        if (name.KeyChar == 'q')
                        {
                            break;
                        }

                        if (name.KeyChar == 's')
                        {
                            string fname = "gen-" + genCount.ToString();
                            StreamWriter writer = new(fname + ".txt");
                            var plt = new ScottPlot.Plot(800, 200);
                            double[,] data = new double[board.Rows, board.Columns];
                            for (int row = 0; row < board.Rows; row++)
                            {
                                for (int col = 0; col < board.Columns; col++)
                                {
                                    var cell = board.Cells[col, row];
                                    if (cell.IsAlive)
                                    {
                                        writer.Write('1');
                                        data[row, col] = 1;
                                    }
                                    else
                                    {
                                        writer.Write('0');
                                        data[row, col] = 0;
                                    }
                                    writer.Write(',');
                                }
                                writer.Write("\n");
                            }
                            writer.Close();
                            var hm = plt.AddHeatmap(data, lockScales: false);
                            hm.CellWidth = 10;
                            hm.CellHeight = 10;
                            plt.SaveFig(fname + ".png");
                        }
                    }
                
                Console.Clear();
                board.Render(board);
                board.Advance();
                Thread.Sleep(100);
                ++genCount;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Живых клеток: {0}", board.LiveCells());
                Console.WriteLine("Симметричных элементов: {0}", board.Symmetric());
                Console.WriteLine($"Поколение: {genCount}");
                }
            }
        }
    }

}
