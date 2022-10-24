using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCheckers
{
    public class Board
    {
        public int CountWhite;
        public int CountBlack;
        public int Size;
        public List<List<Checker>> Matrix = new List<List<Checker>>();

        public Dictionary<Checker.Colors, int> Counts = new Dictionary<Checker.Colors, int>()
        {
            [Checker.Colors.White] = -1,
            [Checker.Colors.Black] = -1,
        };

        // Создание доски
        public Board(int size, int countWhite = 0, int countBlack = 0)
        {
            Size = size;
            CountWhite = countWhite == 0 ? Size / 2 - 1 : Math.Max(1, Math.Min(countWhite, Size / 2 - 1));
            CountBlack = countBlack == 0 ? Size / 2 - 1 : Math.Max(1, Math.Min(countBlack, Size / 2 - 1));

            Counts[Checker.Colors.White] = Size / 2 * CountWhite;
            Counts[Checker.Colors.Black] = Size / 2 * CountBlack;

            CreateMatrix();
            ArrangeCheckers();
        }

        // Создание матрицы, соответствующей доске
        private void CreateMatrix()
        {
            for (var i = 0; i < Size; i++)
            {
                Matrix.Add(new List<Checker>());
                for (var j = 0; j < Size; j++)
                    Matrix[i].Add(new Checker());
            }
        }

        // Расстановка шашек
        private void ArrangeCheckers()
        {
            for (var i = 1; i <= CountWhite; i++)
                for (var j = (i + 1) % 2; j < Size; j += 2)
                    Matrix[Size - i][j] = new Checker(Checker.Colors.White, i, j);

            for (var i = 0; i < CountWhite; i++)
                for (var j = (i + 1) % 2; j < Size; j += 2)
                    Matrix[i][j] = new Checker(Checker.Colors.Black, i, j);
        }

        // Получение шашки по координатам
        public Checker GetChecker(int i, int j)
        {
            return Matrix[i][j];
        }

        // Поменять шашки местами
        public void Swap(int x1, int y1, int x2, int y2)
        {
            (Matrix[y1][x1], Matrix[y2][x2]) = (Matrix[y2][x2], Matrix[y1][x1]);
        }
    }
}
