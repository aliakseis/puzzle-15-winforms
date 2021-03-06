using System;
using System.Drawing;

namespace Games.x15
{
    /// <summary>
    /// Игровое поле для игры в осьмушки.
    /// Представляет собой идеальную модель реальной деревянной коробки 3x3 см.
    /// </summary>
    public class CellTable
    {
        /// <summary>
        /// Ссылка на реальный контрол, в котором воплощена модель.
        /// </summary>
        private readonly CellPoolControl m_Pool;

        /// <summary>
        /// Здесь запоминаем положение пустой ячейки, чтобы каждый раз не искать.
        /// </summary>
        internal Point emptyCell;

        internal Rectangle emptyRect;

        /// <summary>
        /// Чтобы Джеффри Рихтер был спокоен, мы не станем использовать многомерных массивов.
        /// </summary>
        private CellPlate[] internalPlateArray;


        /// <summary>
        /// Специальный флажок для отложенного пересчёта размера клеток.
        /// При изменении размера контрола всё будет пересчитано, но не сразу.
        /// Так сказать, гром не грянет — мужик не перекрестится. А зачем зря крестится, ведь верно?
        /// </summary>
        internal bool IsCellRectangleValid;

        /// <summary>
        /// Высота игрового поля. Истинное дао в отсутсвии дао. Истинная свобода в отсутствии свободы.
        /// </summary>
        private int m_Height;

        /// <summary>
        /// Ширина игрового поля. Не представляет коммерческого интереса,
        /// так как значительная часть программы заточена под жёсткое значение 3x3,
        /// Но для демонстрации незаангажированности и политической нейтральности этот параметр всё-таки заведён.
        /// </summary>
        private int m_Width;


        /// <summary>
        /// Игровое поле создаётся соответствующими органами по внутренней инструкции и штатному расписанию.
        /// Но, в общем, этот путь проходит здесь.
        /// </summary>
        /// <param name="pool">Контрол, к которому будет подвязано игровое поле.</param>
        internal CellTable(CellPoolControl pool)
        {
            if (pool == null) throw new ArgumentNullException("pool");

            m_Pool = pool;
            m_Pool.SizeChanged += m_Pool_SizeChanged;

            Relocate(4, 4);
        }

        /// <summary>
        /// Контрол, к которому подвязано игровое поле.
        /// Связь не видна рядовому обывателю, только своим.
        /// </summary>
        internal CellPoolControl Pool
        {
            get { return m_Pool; }
        }

        /// <summary>
        /// Ширина поля в клеточках. По традиции равна трём.
        /// </summary>
        public int Width
        {
            get { return m_Width; }
        }

        /// <summary>
        /// Высота поля.
        /// </summary>
        public int Height
        {
            get { return m_Height; }
        }

        /// <summary>
        /// Индексер для доступа к шашкам.
        /// </summary>
        public CellPlate this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= Width)
                    throw new ArgumentOutOfRangeException("x", x, "Column index of range.");
                if (y < 0 || y >= Height)
                    throw new ArgumentOutOfRangeException("x", x, "Row index of range.");

                return internalPlateArray[y*Width + x];
            }
        }

        /// <summary>
        /// Индексер для доступа к шашкам.
        /// </summary>
        public CellPlate this[Point coord]
        {
            get { return this[coord.X, coord.Y]; }
        }

        /// <summary>
        /// Чтобы расставить шашки, есть только один путь.
        /// </summary>
        /// <param name="x">Координата шашки (колонка).</param>
        /// <param name="y">Координата (строка).</param>
        /// <returns>Созданная и расставленная шашка.</returns>
        public CellPlate CreateCell(int x, int y)
        {
            if (this[x, y] != null)
                throw new ArgumentException("Cell at this point is already present.");

            CellPlate result = new CellPlate(this, x, y);
            internalPlateArray[y*Width + x] = result;

            result.Invalidate();

            return result;
        }

        /// <summary>
        /// Нужно рассчитать положения шашек в точных пикселах.
        /// Пока ничего не двигается, специальный флажок "запирает" пересчёт,
        /// то есть насчитанные прямоугольники используются.
        /// При изменении пересчёт вызывается один раз для всего поля.
        /// </summary>
        internal void NeedCellRectangle()
        {
            if (IsCellRectangleValid)
                return;

            Rectangle tableRect = new Rectangle(
                Point.Empty,
                Pool.ClientRectangle.Size);

            tableRect.Inflate(-6, -6);

            Size cellSize = new Size(
                tableRect.Width/Width,
                tableRect.Height/Height);

            int adjX = tableRect.Width - cellSize.Width*Width;
            int adjY = tableRect.Height - cellSize.Height*Height;

            tableRect.Offset(adjX/2, adjY/2);
            tableRect.Width -= adjX - adjX/2;
            tableRect.Height -= adjY - adjY/2;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    CellPlate cell = this[x, y];
                    Rectangle rect = new Rectangle(
                        tableRect.X + cellSize.Width*x,
                        tableRect.Y + cellSize.Height*y,
                        cellSize.Width,
                        cellSize.Height);
                    if (cell != null)
                        cell.SetCellRectangle(rect);
                    else
                        emptyRect = rect;
                }
        }

        /// <summary>
        /// Сместить шашку.
        /// </summary>
        /// <param name="cell">Шашка.</param>
        /// <param name="relativeX">Координата (колонка).</param>
        /// <param name="relativeY">Координата (строка).</param>
        /// <param name="toEmpty"></param>
        internal void InternalShiftCellRelative(CellPlate cell, int relativeX, int relativeY, bool toEmpty)
        {
            CellPlate oldCell = this[cell.X + relativeX, cell.Y + relativeY];
            if (oldCell != null)
            {
                if (toEmpty)
                    throw new InvalidOperationException("Cannot shift, position is not empty.");
                oldCell.SetXY(cell.X, cell.Y);
            }
            else
            {
                emptyCell.Offset(-relativeX, -relativeY);
            }
            internalPlateArray[cell.Y*Width + cell.X] = oldCell;
            cell.SetXY(cell.X + relativeX, cell.Y + relativeY);
            internalPlateArray[cell.Y*Width + cell.X] = cell;
        }

        /// <summary>
        /// Выделить внутренний массив под игровое поле.
        /// </summary>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        private void Relocate(int newWidth, int newHeight)
        {
            if (newWidth < 0)
                throw new ArgumentOutOfRangeException("newWidth", newWidth, "Negative width is not allowed.");
            if (newHeight < 0)
                throw new ArgumentOutOfRangeException("newHeight", newHeight, "Negative height is not allowed.");

            if (m_Width > 0 && m_Height > 0)
            {
                CellPlate[] newArray = new CellPlate[newWidth*newHeight];
                for (int x = 0; x < m_Width && x < newWidth; x++)
                    for (int y = 0; y < m_Height && y < newHeight; y++)
                        newArray[y*newWidth + x] = internalPlateArray[y*m_Width + x];

                m_Width = newWidth;
                m_Height = newHeight;
                internalPlateArray = newArray;
            }
            else
            {
                m_Width = newWidth;
                m_Height = newHeight;
                internalPlateArray = new CellPlate[m_Width*m_Height];
            }
        }

        /// <summary>
        /// При создании игрового поля автоматически цепляется обработчик на изменение размеров контрола.
        /// Это нужно для правильного расчёта координат шашек.
        /// </summary>
        private void m_Pool_SizeChanged(object sender, EventArgs e)
        {
            IsCellRectangleValid = false;
        }
    }
}