using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace MyCheckers
{
    public class Checker
    {
        public enum Colors
        {
            White,
            Black,
            Empty,
        }

        public Dictionary<(Colors, bool), Bitmap> Images = new Dictionary<(Colors, bool), Bitmap>()
        {
            [(Colors.White, false)] = new Bitmap("../../data/white_checker.png"),
            [(Colors.Black, false)] = new Bitmap("../../data/black_checker.png"),
            [(Colors.White, true)] = new Bitmap("../../data/white_queen_checker.png"),
            [(Colors.Black, true)] = new Bitmap("../../data/black_queen_checker.png"),
        };

        public Colors Color = Colors.Empty;
        public int X, Y;
        public bool Queen = false;

        public Checker() { }

        // Создание шашки
        public Checker(Colors color, int y, int x)
        {
            Color = color;
            X = x;
            Y = y;
        }

        // Сделать шашку дамкой
        public void ToQueen()
        {
            Queen = true;
        }

        // Смерть шашки
        public void Die(Board board)
        {
            board.Counts[Color]--;
            Color = Colors.Empty;
        }

        // Получение картинки шашки
        public Bitmap GetImage()
        {
            return Images[(Color, Queen)];
        }
    }
}
