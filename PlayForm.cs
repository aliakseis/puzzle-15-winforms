using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Timer=System.Windows.Forms.Timer;

namespace Games.x15
{
    public enum MoveDirection
    {
        Right = 0,
        Down = 1,
        Left = 2,
        Up = 3,
    }

    /// <summary>
    /// Форма для игры в осьмушки.
    /// Можно управлять клавишами со стрелакам,
    /// а можно взять в руки мышку и устроить монстрам настоящую резню.
    /// </summary>
    public class PlayForm : Form
    {
        /// <summary>
        /// Контрол для игры в осьмушки.
        /// </summary>
        private CellPoolControl gamePool;

        /// <summary>
        /// Таймер, запускающийся для решения головоломки.
        /// </summary>
        private Timer helpPlayTimer;

        /// <summary>
        /// Стратегия решения. Каждая ячейка соответствует уникальному состоянию поля.
        /// В ячейке хранится смещение, необходимое в этой ситуации для продвижения к выигрышу.
        /// </summary>
        private IEnumerator<byte> solution;

        #region Default members

        private IContainer components;

        public PlayForm()
        {
            //
            // Required for Windows Form Designer support
            //
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
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlayForm));
            this.helpPlayTimer = new System.Windows.Forms.Timer(this.components);
            this.gamePool = new Games.x15.CellPoolControl();
            this.SuspendLayout();
            // 
            // helpPlayTimer
            // 
            this.helpPlayTimer.Interval = 1200;
            this.helpPlayTimer.Tick += new System.EventHandler(this.helpPlayTimer_Tick);
            // 
            // gamePool
            // 
            this.gamePool.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gamePool.BackColor = System.Drawing.Color.MidnightBlue;
            this.gamePool.CellBackColor = System.Drawing.Color.Tan;
            this.gamePool.Font = new System.Drawing.Font("Verdana", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.gamePool.ForeColor = System.Drawing.Color.White;
            this.gamePool.Location = new System.Drawing.Point(16, 16);
            this.gamePool.Name = "gamePool";
            this.gamePool.Size = new System.Drawing.Size(376, 380);
            this.gamePool.TabIndex = 2;
            this.gamePool.CellClick += new Games.x15.CellClickEventHandler(this.gamePool_CellClick);
            this.gamePool.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PlayForm_KeyDown);
            // 
            // PlayForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(412, 418);
            this.Controls.Add(this.gamePool);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "PlayForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Осьмушки";
            this.TransparencyKey = System.Drawing.Color.MidnightBlue;
            this.Load += new System.EventHandler(this.PlayForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// Смещение клетки. Вернее, смещение клетки, соседней с пустой.
        /// "Обратный" подход избран разработчиком для усложнения игры.
        /// </summary>
        private void shiftEmptyCell(Point shift)
        {
            Point moveCell = new Point(
                gamePool.Cells.emptyCell.X - shift.X,
                gamePool.Cells.emptyCell.Y - shift.Y);

            if (moveCell.X < 0 || moveCell.X >= gamePool.Cells.Width
                || moveCell.Y < 0 || moveCell.Y >= gamePool.Cells.Height)
                return;

            gamePool.Cells[moveCell].ShiftRelative(shift);
        }

        /// <summary>
        /// Смещение клетки.
        /// </summary>
        private void shiftEmptyCell(int x, int y)
        {
            shiftEmptyCell(new Point(x, y));
        }

        /// <summary>
        /// Заполнение игрового поля циферками от 1 до 8.
        /// </summary>
        private void initCells()
        {
            gamePool.Cells.emptyCell = new Point(
                gamePool.Cells.Width - 1,
                gamePool.Cells.Height - 1);

            for (int x = 0; x < gamePool.Cells.Width; x++)
                for (int y = 0; y < gamePool.Cells.Height; y++)
                {
                    if (x != gamePool.Cells.emptyCell.X
                        || y != gamePool.Cells.emptyCell.Y)
                    {
                        gamePool.Cells.CreateCell(x, y).Caption = (1 + x + y*gamePool.Cells.Width).ToString();
                    }
                }
        }

        /// <summary>
        /// При запуске формы заполнять игровое поле.
        /// </summary>
        private void PlayForm_Load(object sender, EventArgs e)
        {
            initCells();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        [DllImport("solver", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int Solve([MarshalAs(UnmanagedType.LPArray)] byte[] pInput,
                                        [Out] [MarshalAs(UnmanagedType.LPArray, SizeConst = 80)] byte[] pResult);

        /// <summary>
        /// По клавишам со стрелками шашки перемещаются на пустую позицию.
        /// По клавише F12 запускается решение головоломки. Любая другая клавиша останавливает решение.
        /// </summary>
        private void PlayForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (solution != null)
            {
                solution = null;
                helpPlayTimer.Enabled = false;
            }

            switch (e.KeyData)
            {
                case Keys.Down:
                    shiftEmptyCell(0, 1);
                    break;

                case Keys.Up:
                    shiftEmptyCell(0, -1);
                    break;

                case Keys.Left:
                    shiftEmptyCell(-1, 0);
                    break;

                case Keys.Right:
                    shiftEmptyCell(1, 0);
                    break;

                case Keys.F12:

                    Application.UseWaitCursor = true;
                    SendMessage(Handle, 0x20, Handle, (IntPtr)1);

                    gamePool.Enabled = false;

                    Thread thread = new Thread(FindSolution);
                    thread.IsBackground = true;
                    thread.Start(GetNormalized());

                    break;
            }
        }

        private void FindSolution(object data)
        {
            byte[] normalized = (byte[]) data;
            byte[] strategy = new byte[80];
            int result = Solve(normalized, strategy);

            List<byte> list = null;
            if (result > 0)
            {
                list = new List<byte>(strategy);
                list.RemoveRange(result, list.Count - result);
            }

            BeginInvoke(new StartAutoPlayDelegate(StartAutoPlay), new object[] {list});
        }

        private void StartAutoPlay(IList<byte> list)
        {
            if (list != null)
            {
                solution = list.GetEnumerator();
                helpPlayTimer.Enabled = true;
                stepAutoResolve();
            }

            gamePool.Enabled = true;
            Application.UseWaitCursor = false;
            SendMessage(Handle, 0x20, Handle, (IntPtr)1);
            gamePool.Focus();
        }

        /// <summary>
        /// Получить нормализованое (закодированое) состояние игрового поля.
        /// По этому значению как по индексу выбирается следующий ход в массиве стратегии.
        /// </summary>
        private byte[] GetNormalized()
        {
            byte[] digitPos = new byte[16];
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    byte digit;

                    if (gamePool.Cells[x, y] == null
                        || (gamePool.Cells[x, y].Caption + "").Trim() == "")
                        digit = 0;
                    else
                        digit = byte.Parse(gamePool.Cells[x, y].Caption);

                    digitPos[y*4 + x] = digit;
                }
            }

            return digitPos;
        }

        /// <summary>
        /// По нажатию на клавишу она стремится передвинутся на пустую клетку.
        /// </summary>
        private void gamePool_CellClick(object Sender, CellClickEventArgs e)
        {
            if (solution != null)
            {
                solution = null;
                helpPlayTimer.Enabled = false;
            }

            // check if 4 near cells of emptyCell
            if (Math.Abs(e.Cell.X - gamePool.Cells.emptyCell.X) + Math.Abs(e.Cell.Y - gamePool.Cells.emptyCell.Y) == 1)
            {
                shiftEmptyCell(
                    gamePool.Cells.emptyCell.X - e.Cell.X,
                    gamePool.Cells.emptyCell.Y - e.Cell.Y);
            }
        }

        /// <summary>
        /// Выполнить следующий шаг для решения головоломки (вызывается при необходимости, по таймеру).
        /// </summary>
        private void stepAutoResolve()
        {
            if (!solution.MoveNext())
            {
                solution = null;
                helpPlayTimer.Enabled = false;
                return;
            }

            int nextMove = solution.Current;

            MoveDirection nextDir = (MoveDirection) (nextMove);

            switch (nextDir)
            {
                case MoveDirection.Up:
                    shiftEmptyCell(0, 1);
                    break;

                case MoveDirection.Down:
                    shiftEmptyCell(0, -1);

                    break;

                case MoveDirection.Left:
                    shiftEmptyCell(1, 0);

                    break;

                case MoveDirection.Right:
                    shiftEmptyCell(-1, 0);

                    break;
            }
        }

        /// <summary>
        /// Обработчик таймера. Запуск таймера по клавише F12 приводит в действие механизм решения головоломки.
        /// </summary>
        private void helpPlayTimer_Tick(object sender, EventArgs e)
        {
            stepAutoResolve();
        }

        #region Nested type: StartAutoPlayDelegate

        private delegate void StartAutoPlayDelegate(IList<byte> list);

        #endregion
    }
}