using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Games.x15
{
    /// <summary>
    /// Чудесный контрол, в котором живут и передвигаются шашки.
    /// Содержит несколько внутренних компонент-органов, управляющих рисованием, анимацией шашек.
    /// Для внешних пользователей его богатый и неоднородный внутренний мир невидим,
    /// видны только шашки, которые можно двигать.
    /// </summary>
    public class CellPoolControl : Control
    {
        private const int WS_BORDER = 0x800000;
        private const int WS_EX_CLIENTEDGE = 0x200;

        /// <summary>
        /// Компонент, ответственный за анимацию шашек.
        /// </summary>
        private AnimationManager animationManager;

        /// <summary>
        /// Кисть для отрисовки фона. Объект-кисть кешируется
        /// во исполнение заветов несуществующего
        /// и никогда не существовавшего дедушки Ильича.
        /// </summary>
        private SolidBrush backBrush;

        /// <summary>
        /// Вид границ.
        /// </summary>
        private BorderStyle m_BorderStyle;

        /// <summary>
        /// Цвет фона игрового поля.
        /// </summary>
        private Color m_CellBackColor = SystemColors.Control;

        /// <summary>
        /// Игровое поле с шашками внутри.
        /// Логическая сторона вопроса, положения, строки, колонки.
        /// </summary>
        private CellTable m_Cells;

        private CellPlate m_movingCell;

        /// <summary>
        /// Дополнительное поле для действенной надёжной обработки нажатий мышки на шашки.
        /// </summary>
        private Point mousePos;

        /// <summary>
        /// Компонент, который берёт на себя всю отрисовку шашек.
        /// </summary>
        private CellPainter painter;

        #region Default members

        private IContainer components;

        public CellPoolControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            init();
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

        #region InitializeComponent

        private void InitializeComponent()
        {
            components = new Container();
            animationManager = new AnimationManager(components);
            painter = new CellPainter(components);
        }

        #endregion

        /// <summary>
        /// Цвет фона игрового поля.
        /// </summary>
        public Color CellBackColor
        {
            get { return m_CellBackColor; }
            set
            {
                if (value == m_CellBackColor)
                    return;
                m_CellBackColor = value;
                Invalidate();
            }
        }


        /// <summary>
        /// Игровое поле в своей идеализированной, логической ипостаси.
        /// </summary>
        public CellTable Cells
        {
            get { return m_Cells; }
        }


        /// <summary>
        /// Вид границ игрового поля.
        /// </summary>
        [DefaultValue(BorderStyle.Fixed3D)]
        public BorderStyle BorderStyle
        {
            get { return m_BorderStyle; }
            set
            {
                if (value == m_BorderStyle)
                    return;

                m_BorderStyle = value;
                UpdateStyles();
            }
        }

        /// <summary>
        /// Создаём пухленькую границу хакерскими методами.
        /// Код украден у Микрософта.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle &= ~WS_EX_CLIENTEDGE;
                cp.Style &= ~WS_BORDER;

                switch (m_BorderStyle)
                {
                    case BorderStyle.Fixed3D:
                        cp.ExStyle |= WS_EX_CLIENTEDGE;
                        break;

                    case BorderStyle.FixedSingle:
                        cp.Style |= WS_BORDER;
                        break;
                }
                return cp;
            }
        }

        #region UpdateGraphics / DisposeGraphics

        private void UpdateGraphics()
        {
            PaintUtils.Update(ref backBrush, BackColor);
        }

        private void DisposeGraphics()
        {
            PaintUtils.Dispose(ref backBrush);
        }

        #endregion

        /// <summary>
        /// Событие нажатия мышки на шашечку.
        /// </summary>
        public event CellClickEventHandler CellClick;

        /// <summary>
        /// Метод вызова события клика на шашке.
        /// </summary>
        /// <param name="e">Параметры клика (ячейка).</param>
        protected virtual void OnCellClick(CellClickEventArgs e)
        {
            if (CellClick != null)
                CellClick(this, e);
        }

        /// <summary>
        /// Метод вызывается из конструктора. Вытащен сюда,
        /// чтобы абстрагировать от InitializeComponent.
        /// </summary>
        private void init()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.DoubleBuffer
                | ControlStyles.Opaque
                | ControlStyles.ResizeRedraw
                //| ControlStyles.Selectable
                | ControlStyles.UserPaint,
                true);

            BackColor = SystemColors.Window;
            ForeColor = SystemColors.WindowText;
            BorderStyle = BorderStyle.Fixed3D;

            m_Cells = new CellTable(this);
        }


        // Стили окна для "пухлой" границы.

        // Специальные методы для кеширования графических объектов.

        /// <summary>
        /// Отрисовка игрового поля, с бортиками, направляющими,
        /// цельным боками и хорошо проклеенным дном.
        /// </summary>
        /// <param name="e">Обычный параметр. См. описание у базового класса.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            UpdateGraphics();
            e.Graphics.FillRectangle(
                backBrush,
                e.ClipRectangle);


            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingMode = CompositingMode.SourceOver;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            //e.Graphics.=CompositingMode.SourceOver;

            for (int x = 0; x < Cells.Width; x++)
                for (int y = 0; y < Cells.Height; y++)
                {
                    CellPlate cell = Cells[x, y];
                    if (cell != null && cell != m_movingCell)
                    {
                        AnimationManager.TransitionInfo transition = animationManager[cell];

                        if (transition == null)
                            painter.DrawCell(
                                e,
                                cell,
                                CellBackColor,
                                ForeColor,
                                Font);
                        else
                            painter.DrawCellTransition(
                                e,
                                cell,
                                CellBackColor,
                                ForeColor,
                                Font,
                                transition.StartPosition,
                                transition.CompletionRatio);
                    }
                }

            if (m_movingCell != null)
                painter.DrawCell(
                    e,
                    m_movingCell,
                    CellBackColor,
                    ForeColor,
                    Font);
        }

        /// <summary>
        /// Контрол в общем-то сам неплохо отрабатывает стрелочки.
        /// Если этот нехитрый код пропустить, стрелочки будут
        /// срабатывать как перемещение фокуса (TabIndex).
        /// </summary>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Down
                || keyData == Keys.Up
                || keyData == Keys.Left
                || keyData == Keys.Right)
                return false;
            return base.ProcessDialogKey(keyData);
        }

        /// <summary>
        /// Вызывает запуск анимации при помощи внутренного AnimationManager.
        /// </summary>
        /// <param name="cell">Какая ячейка должна прокатится?</param>
        /// <param name="startPosition">С какого места исходит каталово?</param>
        internal void AddTransition(
            CellPlate cell,
            Rectangle startPosition)
        {
            animationManager.StartTransition(
                cell,
                startPosition);
        }

        /// <summary>
        /// Обрабатываем клик на контроле, находим шашку и проводим по бумагам как клик на шашке.
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            mousePos = new Point(e.X, e.Y);

            base.OnMouseUp(e);

            m_Cells.IsCellRectangleValid = false;
            CellPlate movingCell = m_movingCell;
            m_movingCell = null;

            if (movingCell != null)
            {
                for (int x = 0; x < Cells.Width; x++)
                    for (int y = 0; y < Cells.Height; y++)
                    {
                        CellPlate cell = Cells[x, y];
                        if (cell == null)
                        {
                            if (m_Cells.emptyRect.Contains(mousePos))
                            {
                                Cells.InternalShiftCellRelative(movingCell, x - movingCell.X, y - movingCell.Y, true);
                                Invalidate();
                                return;
                            }
                        }
                        else if (cell.CellRectangle.Contains(mousePos))
                        {
                            if (cell == movingCell)
                            {
                                OnCellClick(new CellClickEventArgs(cell));
                            }
                            else
                            {
                                Cells.InternalShiftCellRelative(movingCell, x - movingCell.X, y - movingCell.Y, false);
                            }
                            Invalidate();
                            return;
                        }
                    }

                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            mousePos = new Point(e.X, e.Y);
            Focus();

            base.OnMouseDown(e);

            m_movingCell = null;
            for (int x = 0; x < Cells.Width; x++)
                for (int y = 0; y < Cells.Height; y++)
                {
                    CellPlate cell = Cells[x, y];
                    if (cell != null
                        && cell.CellRectangle.Contains(mousePos))
                    {
                        m_Cells.IsCellRectangleValid = true;
                        m_movingCell = cell;
                        return;
                    }
                }
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (m_movingCell != null)
            {
                Rectangle oldRect = m_movingCell.CellRectangle;
                Rectangle rect = oldRect;
                rect.Offset(e.X - mousePos.X, e.Y - mousePos.Y);
                m_movingCell.SetCellRectangle(rect);
                mousePos = new Point(e.X, e.Y);
                Invalidate(PaintUtils.Union(oldRect, rect));
            }


            base.OnMouseMove(e);
        }
    }

    /// <summary>
    /// Делегат плющива на шашку.
    /// </summary>
    public delegate void CellClickEventHandler(object Sender, CellClickEventArgs e);

    /// <summary>
    /// Подробности прессинга на чувствительные игровые шашечки.
    /// Очень удобно использовать мышку для движения шашек.
    /// </summary>
    public class CellClickEventArgs
    {
        /// <summary>
        /// Какая шашка-то прижата? Ну.
        /// </summary>
        private readonly CellPlate m_Cell;

        /// <summary>
        /// Конструктор: задаём шашку, чтобы знали, где нажим происходит.
        /// </summary>
        /// <param name="cell">Шашка, на которую действует вытесняющая сила.</param>
        public CellClickEventArgs(CellPlate cell)
        {
            if (cell == null)
                throw new ArgumentNullException("cell");
            m_Cell = cell;
        }

        /// <summary>
        /// Шашка, на которую действует сила.
        /// </summary>
        public CellPlate Cell
        {
            get { return m_Cell; }
        }
    }
}