using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MyCheckers
{
    public partial class Game : Form
    {
        Color ColorDark = Color.Brown;
        Color ColorLight = Color.PapayaWhip;

        const int cellSize = 80;
        int size = 8;
        Board board;
        List<List<Button>> buttons;

        Checker.Colors currentPlayer;
        Button prevButton;
        bool isMoving;
        bool canEat;

        Help help = null;

        // Начало игры
        public Game()
        {
            InitializeComponent();
            this.Text = "Checkers";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.BackColor = ColorLight;

            Init();
        }

        // Начало игры
        public void Init()
        {
            this.Controls.Clear();
            CreateBoard();

            currentPlayer = (Checker.Colors)0;
            isMoving = false;
            prevButton = null;
            canEat = FindFood();
        }

        // Создание доски
        public void CreateBoard()
        {
            board = new Board(size);
            buttons = new List<List<Button>>();

            this.Width = cellSize * board.Size + cellSize / 5 + 300;
            this.Height = cellSize * board.Size + cellSize / 2;

            for (var i = 0; i < board.Size; i++)
            {
                buttons.Add(new List<Button>());
                for (var j = 0; j < board.Size; j++)
                    CreateChecker(i, j);
            }

            CreateTrackBar();
            CreateRestartButton();
            CreateHelpButton();
        }

        // Создание шашки
        public void CreateChecker(int i, int j)
        {
            var checker = board.GetChecker(i, j);

            var button = new Button();
            button.Location = new Point(j * cellSize, i * cellSize);
            button.Size = new Size(cellSize, cellSize);
            button.Click += new EventHandler(PressFigure);

            PaintDefaultColor(button, i, j);

            if (checker.Color == Checker.Colors.White)
                button.Image = new Bitmap(checker.GetImage(), new Size(cellSize - 10, cellSize - 10));

            else if (checker.Color == Checker.Colors.Black)
                button.Image = new Bitmap(checker.GetImage(), new Size(cellSize - 10, cellSize - 10));

            buttons[i].Add(button);
            this.Controls.Add(button);
        }

        // Создание ползунка
        public void CreateTrackBar()
        {
            var trackBar = new TrackBar();

            trackBar.TickStyle = TickStyle.Both;
            trackBar.SetRange(4, 5);
            trackBar.Location = new Point(cellSize * board.Size + 50, this.Height / 2 - 46);
            trackBar.Size = new Size(200, 50);
            trackBar.Value = board.Size / 2;
            trackBar.BackColor = ColorDark;
            trackBar.ForeColor = ColorLight;
            trackBar.Scroll += new EventHandler(ScrollTrackBar);

            this.Controls.Add(trackBar);
        }

        // Изменение положения ползунка
        public void ScrollTrackBar(object sender, EventArgs e)
        {
            Thread.Sleep(500);
            var trackBar = sender as TrackBar;
            size = trackBar.Value * 2;
            Init();
        }

        // Создание кнопки рестарта
        public void CreateRestartButton()
        {
            var button = new Button();

            button.Location = new Point(cellSize * board.Size + 47, this.Height / 2 - 100);
            button.Size = new Size(206, 50);
            button.Text = "Новая игра";
            button.ForeColor = ColorLight;
            button.Font = new Font(FontFamily.GenericSansSerif, 25);
            button.Click += new EventHandler(Restart);
            button.BackColor = ColorDark;

            this.Controls.Add(button);
        }

        // Нажатие на кнопку рестарта 
        public void Restart(object sender, EventArgs e) => Init();

        // Создание кнопки открытие правил
        public void CreateHelpButton()
        {
            var button = new Button();

            button.Location = new Point(cellSize * board.Size + 47, this.Height / 2 + 4);
            button.Size = new Size(206, 50);
            button.Text = "Правила";
            button.ForeColor = ColorLight;
            button.Font = new Font(FontFamily.GenericSansSerif, 25);
            button.Click += new EventHandler(ShowHelp);
            button.BackColor = ColorDark;

            this.Controls.Add(button);
        }

        // Нажатие на кнопку открытия правил
        public void ShowHelp(object sender, EventArgs e)
        {
            if (help != null)
                help.Hide();

            help = new Help();
            help.Show();
        }

        // Нажатие на фигуру
        public void PressFigure(object sender, EventArgs e)
        {
            if (prevButton != null)
            {
                var (x, y) = FindXY(prevButton);
                PaintDefaultColor(prevButton, x, y);
            }

            var pressedButton = sender as Button;

            if (CheckFigure(pressedButton))
            {
                isMoving = true;
                prevButton = pressedButton;
                PrintMoves(pressedButton, canEat);
                pressedButton.BackColor = Color.LightGreen;
            }
            else
            {
                if (isMoving)
                    Move(prevButton, pressedButton, e);
            }
        }

        // Сделать ход
        public void Move(Button prevButton, Button pressedButton, EventArgs e)
        {
            if (CheckMove(prevButton, pressedButton) && !canEat)
                Go(prevButton, pressedButton);

            else if (CheckEat(prevButton, pressedButton))
            {
                Eat(prevButton, pressedButton);
                canEat = FindFood(prevButton);
            }

            if (canEat)
            {
                PrintMoves(prevButton, canEat);
                PressFigure((object)prevButton, e);
            }

            if (isMoving == false && !canEat)
            {
                ChangePlayer();
                canEat = FindFood();
                prevButton = null;
            }

            CheckQueen(pressedButton);
            CheckEnd();
        }


        // Переместить шашку
        public void Go(Button fromButton, Button toButton)
        {
            ClearBoard();

            var (x1, y1) = FindXY(fromButton);
            var (x2, y2) = FindXY(toButton);

            board.Swap(x1, y1, x2, y2);
            SwapImage(fromButton, toButton);

            isMoving = false;
            prevButton = toButton;
        }

        // Съесть шашку
        public void Eat(Button fromButton, Button toButton)
        {
            Go(fromButton, toButton);

            var (x1, y1) = FindXY(fromButton);
            var (x2, y2) = FindXY(toButton);

            int y = y1, x = x1;

            var vectorX = (x2 - x1) / Math.Abs(x2 - x1);
            var vectorY = (y2 - y1) / Math.Abs(y2 - y1);

            for (var i = 1; i < Math.Abs(x2 - x1); i++)
            {
                y += vectorY;
                x += vectorX;
                var newChecker = board.GetChecker(y, x);

                if (newChecker.Color != currentPlayer && newChecker.Color != Checker.Colors.Empty)
                {
                    buttons[y][x].Image = null;
                    newChecker.Die(board);
                    break;
                }
            }
        }

        // Проверка правильности выбора шашки
        public bool CheckFigure(Button button)
        {
            var (x, y) = FindXY(button);
            var checker = board.GetChecker(y, x);
            return checker.Color != Checker.Colors.Empty && checker.Color == currentPlayer;
        }

        // Проверка возможности хода
        public bool CheckMove(Button fromButton, Button toButton)
        {
            var (x1, y1) = FindXY(fromButton);
            var checker = board.GetChecker(y1, x1);

            var (x2, y2) = FindXY(toButton);
            var cell = board.GetChecker(y2, x2);

            return cell.Color == Checker.Colors.Empty && 
                (checker.Color == Checker.Colors.White && (y2 - y1) == -1 && Math.Abs(x2 - x1) == 1 ||
                checker.Color == Checker.Colors.Black && (y2 - y1) == 1 && Math.Abs(x2 - x1) == 1 ||
                checker.Queen && Math.Abs(y2 - y1) == Math.Abs(x2 - x1));
        }

        // Проверка возможности поедания шашки
        public bool CheckEat(Button fromButton, Button toButton)
        {
            var (x1, y1) = FindXY(fromButton);
            var checker = board.GetChecker(y1, x1);

            var (x2, y2) = FindXY(toButton);
            var cell = board.GetChecker(y2, x2);

            if (Math.Abs(x2 - x1) != Math.Abs(y2 - y1) || x1 == x2)
                return false;

            var (myCheckerCount, otherCheckerCount) = CountCheckers(checker, x1, y1, x2, y2);

            return cell.Color == Checker.Colors.Empty && 
                myCheckerCount == 0 && otherCheckerCount == 1 &&
                (checker.Color == Checker.Colors.White && Math.Abs(y2 - y1) == 2 ||
                checker.Color == Checker.Colors.Black && Math.Abs(y2 - y1) == 2 ||
                checker.Queen);
        }

        // Подсчёт попутных шашек
        public (int, int) CountCheckers(Checker checker, int x1, int y1, int x2, int y2)
        {
            var myCheckerCount = 0;
            var otherCheckerCount = 0;

            var vectorX = (x2 - x1) / Math.Abs(x2 - x1);
            var vectorY = (y2 - y1) / Math.Abs(y2 - y1);

            for (var i = 1; i < Math.Abs(x2 - x1); i++)
            {
                var newChecker = board.GetChecker(y1 + i * vectorY, x1 + i * vectorX);

                if (newChecker.Color == checker.Color)
                    myCheckerCount++;

                else if (newChecker.Color != Checker.Colors.Empty)
                    otherCheckerCount++;
            }

            return (myCheckerCount, otherCheckerCount);
        }

        // Проверка на дамку 
        public void CheckQueen(Button button)
        {
            var (x, y) = FindXY(button);
            var checker = board.GetChecker(y, x);

            if (checker.Color == Checker.Colors.White && y == 0 ||
                checker.Color == Checker.Colors.Black && y == board.Size - 1)
            {
                checker.ToQueen();
                button.Image = new Bitmap(checker.GetImage(), new Size(cellSize - 10, cellSize - 10));
            }
        }

        // Проверка на окончание игры
        public void CheckEnd()
        {
            if (board.Counts[Checker.Colors.White] == 0 ||
                board.Counts[Checker.Colors.Black] == 0)
                Init();
        }

        // Проверка на возможность съесть шашку противника игроком
        public bool FindFood()
        {
            bool res = false;

            for (var i1 = 0; i1 < board.Size; i1++)
                for (var j1 = 0; j1 < board.Size; j1++)
                {
                    var fromButton = buttons[i1][j1];
                    if (board.GetChecker(i1, j1).Color != currentPlayer)
                        continue;

                    for (var i2 = 0; i2 < board.Size; i2++)
                        for (var j2 = 0; j2 < board.Size; j2++)
                            res = res || CheckEat(fromButton, buttons[i2][j2]);
                }

            return res;
        }

        // Проверка на возможность съесть шашку противника шашкой
        public bool FindFood(Button fromButton)
        {
            bool res = false;

            for (var i2 = 0; i2 < board.Size; i2++)
                for (var j2 = 0; j2 < board.Size; j2++)
                    res = res || CheckEat(fromButton, buttons[i2][j2]);

            return res;
        }

        // Раскраска возможных ходов
        public void PrintMoves(Button fromButton, bool canEat)
        {
            ClearBoard();

            for (var i = 0; i < board.Size; i++)
                for (var j = 0; j < board.Size; j++)
                {
                    var toButton = buttons[i][j];
                    if (!canEat && CheckMove(fromButton, toButton) || canEat && CheckEat(fromButton, toButton))
                        toButton.BackColor = Color.Coral;
                }
        }

        // Очистка доски от предыдущих раскрасок
        public void ClearBoard()
        {
            for (var i = 0; i < board.Size; i++)
                for (var j = 0; j < board.Size; j++)
                    PaintDefaultColor(buttons[i][j], i, j);
        }

        // Найти координаты шашки
        public (int, int) FindXY(Button button)
        {
            return (button.Location.X / cellSize, button.Location.Y / cellSize);
        }

        // Передвинуть ходящую шашку
        public void SwapImage(Button button1, Button button2)
        {
            (button1.Image, button2.Image) = (button2.Image, button1.Image);
        }

        // Вернуть клетке свой цвет
        public void PaintDefaultColor(Button button, int i, int j)
        {
            if (i % 2 == j % 2)
                button.BackColor = Color.BurlyWood;
            else
                button.BackColor = Color.DarkRed;
        }

        // Передать ход другому игроку
        public void ChangePlayer()
        {
            currentPlayer = (Checker.Colors)(((int)currentPlayer + 1) % 2);
        }
    }
}
