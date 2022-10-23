using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MyCheckers
{
    public partial class Game : Form
    {
        const int cellSize = 80;
        Board board;
        List<List<Button>> buttons = new List<List<Button>>();

        Checker.Colors currentPlayer;
        Button prevButton;
        bool isMoving;
        bool canEat;

        public Game()
        {
            InitializeComponent();
            this.Text = "Checkers";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            Init();
        }

        // Начало игры
        public void Init()
        {
            CreateBoard();

            currentPlayer = (Checker.Colors)0;
            isMoving = false;
            prevButton = null;
            canEat = FindFood();
        }

        // Создание доски
        public void CreateBoard()
        {
            board = new Board();

            this.Width = cellSize * board.Size + cellSize / 5;
            this.Height = cellSize * board.Size + cellSize / 2;

            for (var i = 0; i < board.Size; i++)
            {
                buttons.Add(new List<Button>());
                for (var j = 0; j < board.Size; j++)
                    CreateButton(i, j);
            }
        }

        // Создание шашки
        public void CreateButton(int i, int j)
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

        // Нажать на фигуру
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
                pressedButton.BackColor = Color.Red;
                isMoving = true;
                prevButton = pressedButton;
                PrintMoves(pressedButton, canEat);
            }
            else
            {
                if (isMoving)
                {
                    if (CheckMove(prevButton, pressedButton) && !canEat)
                        Move(prevButton, pressedButton);

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
            }
        }

        // Сделать ход
        public void Move(Button fromButton, Button toButton)
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
            Move(fromButton, toButton);

            var (x1, y1) = FindXY(fromButton);
            var (x2, y2) = FindXY(toButton);

            Checker dieChecker = null;
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

            return cell.Color == Checker.Colors.Empty && 
                myCheckerCount == 0 && otherCheckerCount == 1 &&
                (checker.Color == Checker.Colors.White && Math.Abs(y2 - y1) == 2 ||
                checker.Color == Checker.Colors.Black && Math.Abs(y2 - y1) == 2 ||
                checker.Queen);
        }

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

        public void CheckEnd()
        {
            if( board.Counts[Checker.Colors.White] == 0 ||
                board.Counts[Checker.Colors.Black] == 0)
                this.Close();
        }

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

        public bool FindFood(Button fromButton)
        {
            bool res = false;

            for (var i2 = 0; i2 < board.Size; i2++)
                for (var j2 = 0; j2 < board.Size; j2++)
                    res = res || CheckEat(fromButton, buttons[i2][j2]);

            return res;
        }

        public void PrintMoves(Button fromButton, bool canEat)
        {
            ClearBoard();

            for (var i = 0; i < board.Size; i++)
                for (var j = 0; j < board.Size; j++)
                {
                    var toButton = buttons[i][j];
                    if (!canEat && CheckMove(fromButton, toButton) || canEat && CheckEat(fromButton, toButton))
                        toButton.BackColor = Color.Yellow;
                }
        }

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
                button.BackColor = Color.White;
            else
                button.BackColor = Color.Gray;
        }

        // Передать ход другому игроку
        public void ChangePlayer()
        {
            currentPlayer = (Checker.Colors)(((int)currentPlayer + 1) % 2);
        }
    }
}
