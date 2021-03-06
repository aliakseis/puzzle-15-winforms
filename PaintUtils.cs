using System;
using System.Drawing;

namespace Games.x15
{
    /// <summary>
    /// Вспомогательные чудесные методы для управления и кеширования графических объектов.
    /// И другие мелкие графические утилитки.
    /// </summary>
    internal static class PaintUtils
    {
        #region Update objects

        public static void Update(ref SolidBrush brush, Color brushColor)
        {
            if (brush == null)
                brush = new SolidBrush(brushColor);
            else if (brush.Color != brushColor)
                brush.Color = brushColor;
        }

        public static void Update(ref Pen pen, Color color)
        {
            Update(ref pen, color, 1);
        }

        public static void Update(ref Pen pen, Color penColor, float width)
        {
            if (pen == null)
                pen = new Pen(new SolidBrush(penColor), width);
            else
            {
                if (((SolidBrush) pen.Brush).Color != penColor)
                    ((SolidBrush) pen.Brush).Color = penColor;
                if (pen.Width != width)
                    pen.Width = width;
            }
        }

        #endregion

        #region Dispose objects

        private static void InternalDispose(IDisposable obj)
        {
            if (obj != null)
                obj.Dispose();
        }

        public static void Dispose(ref SolidBrush brush)
        {
            InternalDispose(brush);
            brush = null;
        }

        public static void Dispose(ref Pen pen)
        {
            InternalDispose(pen);
            pen = null;
        }

        public static void Dispose(ref StringFormat sf)
        {
            InternalDispose(sf);
            sf = null;
        }

        #endregion

        /// <summary>
        /// Минимальный прямоугольник, включающий два данных,
        /// со сторонами параллельными осям.
        /// </summary>
        public static Rectangle Union(Rectangle r1, Rectangle r2)
        {
            Rectangle result = new Rectangle(
                Math.Min(r1.Left, r2.Left), Math.Min(r1.Top, r2.Top),
                0, 0);

            result.Width = Math.Max(r1.Right, r2.Right) - result.Left;
            result.Height = Math.Max(r1.Bottom, r2.Bottom) - result.Top;

            return result;
        }

        /// <summary>
        /// Смещение точки на пути из пункта 'start' в пункт 'end'
        /// из расчёта того, что пройдено 'completionRatio' пути.
        /// </summary>
        private static int Transition(int start, int end, decimal completionRatio)
        {
            return (int) (start + (end - start)*completionRatio);
        }

        /// <summary>
        /// Превращение прямоугольника из одной позиции в другую,
        /// из расчёта того, что преобразование завершено на 'completionRatio' часть.
        /// </summary>
        public static Rectangle Transition(Rectangle start, Rectangle end, decimal completionRatio)
        {
            return new Rectangle(
                Transition(start.Left, end.Left, completionRatio), Transition(start.Top, end.Top, completionRatio),
                Transition(start.Width, end.Width, completionRatio),
                Transition(start.Height, end.Height, completionRatio));
        }
    }
}