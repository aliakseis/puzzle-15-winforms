using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Games.x15
{
    /// <summary>
    /// Рисовальщик клеток.
    /// Не понимает никакой логики, только и может, что рисовать что ему скажут.
    /// Но уж рисует как следует, в деталях и без шербинок.
    /// </summary>
    public class CellPainter : Component
    {
        #region Default members

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components;

        public CellPainter(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public CellPainter()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                DisposeGraphics();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion

        /// <summary>
        /// Кисть фона.
        /// Кешируется для того, чтобы всякие лохи не могли трындеть о тормозах GDI+.
        /// Томоза есть только в мозгах. Просто делать нужно всё как положено.
        /// </summary>
        private SolidBrush backBrush;

        /// <summary>
        /// Кисть шрифта.
        /// </summary>
        private SolidBrush captionBrush;

        /// <summary>
        /// Линер тёмный для рамок.
        /// </summary>
        private Pen gridPenDark, gridPenDark2;

        /// <summary>
        /// Линер для рамок: светлый.
        /// </summary>
        private Pen gridPenLight, gridPenLight2;

        /// <summary>
        /// Формат для вывода циферок на шашечках. Чудесным образом совмещает выравнивание по горизонтали, вертикали и т.д.
        /// </summary>
        private StringFormat m_SF;

        #region StringFormat

        private StringFormat SF
        {
            get
            {
                if (m_SF == null)
                {
                    m_SF = StringFormat.GenericTypographic.Clone() as StringFormat;
                    m_SF.Alignment = StringAlignment.Center;
                    m_SF.LineAlignment = StringAlignment.Center;
                    m_SF.FormatFlags = m_SF.FormatFlags
                                       | StringFormatFlags.FitBlackBox
                                       | StringFormatFlags.FitBlackBox;
                }
                return m_SF;
            }
        }

        #endregion

        /// <summary>
        /// Метод, который внешний код вызывает для отрисовки статической шашки (шашки, которая не движется).
        /// </summary>
        /// <param name="e">Стандартный параметр метода OnPaint любого контрола. Совмещает полотно для отрисовки и границы.</param>
        /// <param name="cell">Шашка, которую нужно нарисовать.</param>
        /// <param name="cellBackColor">В каком тоне следует рисовать шашку.</param>
        /// <param name="cellCaptionColor">В каком тоне следует рисовать циферки.</param>
        /// <param name="font">Каким вообще-то шрифтом нужно рисовать цифры.</param>
        public void DrawCell(
            PaintEventArgs e,
            CellPlate cell,
            Color cellBackColor,
            Color cellCaptionColor,
            Font font)
        {
            Rectangle cellRect = cell.CellRectangle;

            DrawCellAt(
                cellRect,
                e, cell, cellBackColor, cellCaptionColor, font);
        }

        /// <summary>
        /// Метод, который внешний код вызывает для отрисовки движущейся шашки.
        /// Класс AnimationManager отвечает за то, чтобы этот метод был вызван, но не занимается рисованием.
        /// </summary>
        /// <param name="e">Стандартный параметр метода OnPaint.</param>
        /// <param name="cell">Шашка, которая должна быть запечатлена в движении.</param>
        /// <param name="cellBackColor">Цвет фона шашки.</param>
        /// <param name="cellCaptionColor">Цвет цифер на шашке.</param>
        /// <param name="font">Шрифт для цифер. Желательно, разборчивый.</param>
        /// <param name="startPosition">Начальная позиция шашки. То место, с которого она едет.</param>
        /// <param name="completionRatio">Свой путь земной пройдя до половины...</param>
        public void DrawCellTransition(
            PaintEventArgs e,
            CellPlate cell,
            Color cellBackColor,
            Color cellCaptionColor,
            Font font,
            Rectangle startPosition,
            decimal completionRatio)
        {
            Rectangle transitionRect = PaintUtils.Transition(
                startPosition,
                cell.CellRectangle,
                completionRatio);

            DrawCellAt(
                transitionRect,
                e, cell, cellBackColor, cellCaptionColor, font);
        }

        // Методы для упрощения кешированием графических объектов.
        // Если, например, цвет кисте совпадает с требуемым, то кисть используется как есть.
        // Просто аккуратная работа.

        /// <summary>
        /// Внутренний метод класса-рисовальщика CellPainter.
        /// Рисует шашку в некоторой позиции.
        /// </summary>
        /// <param name="cellRect">Позиция, где должна быть порисована шашка.</param>
        /// <param name="e">Стандартный параметр OnPaint.</param>
        /// <param name="cell">Шашка собственной персоной.</param>
        /// <param name="cellBackColor">Цвет фона шашки.</param>
        /// <param name="cellCaptionColor">Цвет шрифта шашки.</param>
        /// <param name="font">Шрифт для циферок. Очень удобно.</param>
        private void DrawCellAt(
            Rectangle cellRect,
            PaintEventArgs e,
            CellPlate cell,
            Color cellBackColor,
            Color cellCaptionColor,
            Font font)
        {
            if (cell == null)
                throw new ArgumentNullException("cell");

            if (!e.ClipRectangle.IntersectsWith(cellRect))
                return;

            UpdateGraphics(cellBackColor, cellCaptionColor);

            cellRect.Inflate(-2, -2);

            Rectangle fillRect = cellRect;
            //fillRect.Offset(0,-1);

            e.Graphics.FillRectangle(
                backBrush,
                fillRect);

            DrawCellBorder(e, cellRect);

            // caption

            Rectangle captionRect = cellRect;
            captionRect.Inflate(-2, -2);
            captionRect.Offset(1, 1);

            e.Graphics.DrawString(
                cell.Caption,
                font,
                captionBrush,
                captionRect,
                SF);
        }


        /// <summary>
        /// Нарисовать рамку. Просто рамка.
        /// </summary>
        /// <param name="e">Стандартный параметр OnPaint.</param>
        /// <param name="cellRect">Местоположение шашки.</param>
        /// <param name="lightPen">Светлый линер для рамки (для эффекта выпуклости одна половинка рамки рисуется светлой, другая тёмной).</param>
        /// <param name="darkPen">Тёмный линер для рамки.</param>
        private static void DrawSingleCellBorder(
            PaintEventArgs e,
            Rectangle cellRect,
            Pen lightPen,
            Pen darkPen)
        {
            // light grid lines

            e.Graphics.DrawLine(
                lightPen,
                cellRect.Left + 1, cellRect.Top + 2,
                cellRect.Right, cellRect.Top + 2);

            e.Graphics.DrawLine(
                lightPen,
                cellRect.Left + 2, cellRect.Top + 2,
                cellRect.Left + 2, cellRect.Bottom);


            // dark grid lines

            e.Graphics.DrawLine(
                darkPen,
                cellRect.Right, cellRect.Top + 1,
                cellRect.Right, cellRect.Bottom);

            e.Graphics.DrawLine(
                darkPen,
                cellRect.Left + 1, cellRect.Bottom,
                cellRect.Right - 1, cellRect.Bottom);
        }

        /// <summary>
        /// Нарисовать красивую границу шашки. Вызывает отрисовку несколько раз рамки.
        /// Получается эффектная выпуклая шашка.
        /// Как и положено, выделение функциональности отрисовки позволяет
        /// извращаться таким образом без всякого зарения совести. Остальная функциональность вообще не затрагивается.
        /// </summary>
        /// <param name="e">Стандартный параметр OnPaint.</param>
        /// <param name="cellRect">Прямоугольник для отрисовки шашки.</param>
        private void DrawCellBorder(
            PaintEventArgs e,
            Rectangle cellRect)
        {
            DrawSingleCellBorder(
                e, cellRect,
                gridPenLight,
                gridPenDark);

            cellRect.Inflate(-1, -1);

            DrawSingleCellBorder(
                e, cellRect,
                gridPenLight2,
                gridPenDark2);
        }

        #region UpdateGraphics / DisposeGraphics

        private void UpdateGraphics(
            Color cellBackColor,
            Color cellCaptionColor)
        {
            PaintUtils.Update(ref backBrush, cellBackColor);
            PaintUtils.Update(ref captionBrush, cellCaptionColor);

            PaintUtils.Update(
                ref gridPenLight,
                Color.FromArgb(
                    128,
                    Color.White));
            PaintUtils.Update(
                ref gridPenLight2,
                Color.FromArgb(
                    64,
                    Color.White));
            PaintUtils.Update(
                ref gridPenDark,
                Color.FromArgb(
                    128,
                    ControlPaint.Dark(cellBackColor)));
            PaintUtils.Update(
                ref gridPenDark2,
                Color.FromArgb(
                    64,
                    ControlPaint.Dark(cellBackColor)));
        }

        private void DisposeGraphics()
        {
            PaintUtils.Dispose(ref backBrush);
            PaintUtils.Dispose(ref captionBrush);
            PaintUtils.Dispose(ref gridPenLight);
            PaintUtils.Dispose(ref gridPenDark);
            PaintUtils.Dispose(ref m_SF);
        }

        #endregion
    }
}