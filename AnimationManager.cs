using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Games.x15
{
    /// <summary>
    /// Класс, управляющий анимацией поползновений шашек.
    /// Позволяет в классе-контроле CellPoolControl заниматься только самой отрисовкой.
    /// Вся логика, касающаяся движений и вызовов отрисовки, упрятана от людских глаз в AnimationManager.
    /// </summary>
    public class AnimationManager : Component
    {
        /// <summary>
        /// Одновременно может происходить несколько анимаций (transitions).
        /// </summary>
        private readonly Hashtable transitionByCell = new Hashtable();

        /// <summary>
        /// Запускается только для анимации.
        /// </summary>
        private Timer transitionTimer;

        #region Default members

        private IContainer components;

        public AnimationManager(IContainer container)
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

        public AnimationManager()
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
            this.components = new System.ComponentModel.Container();
            this.transitionTimer = new System.Windows.Forms.Timer(this.components);
            // 
            // transitionTimer
            // 
            this.transitionTimer.Interval = 5;
            this.transitionTimer.Tick += new System.EventHandler(this.transitionTimer_Tick);
        }

        #endregion

        /// <summary>
        /// Получение информации о шашке: движется ли она и в какой точке пути.
        /// Оракул раскрывает судьбу.
        /// </summary>
        public TransitionInfo this[CellPlate cell]
        {
            get { return transitionByCell[cell] as TransitionInfo; }
        }

        /// <summary>
        /// Запуск анимации ячейки. Пройдёт время и анимация закончится, не стоит об этом беспокоиться.
        /// Всё работу берёт на себя AnimationManager.
        /// </summary>
        /// <param name="cell">Шашечка, которая поедет в дальний край. Шашка официально должна находится на финальной позиции.</param>
        /// <param name="startPosition">Позиция, с которой начинается движение.</param>
        public void StartTransition(
            CellPlate cell,
            Rectangle startPosition)
        {
            TransitionInfo transition = transitionByCell[cell] as TransitionInfo;

            if (transition != null)
            {
                transition.InvalidateTransitionArea();
                transitionByCell.Remove(cell);
            }

            transition = new TransitionInfo(
                cell,
                startPosition);

            transitionByCell[cell] = transition;

            UpdateTimer();
        }

        /// <summary>
        /// Не пора ли отключить таймер:
        /// Этот метод вызывается оттуда и отсюда, на всякий случай, чтобы таймер не очень-то тикал.
        /// </summary>
        private void UpdateTimer()
        {
            transitionTimer.Enabled = transitionByCell.Count > 0;
        }

        /// <summary>
        /// Обработчик таймера
        /// </summary>
        private void transitionTimer_Tick(object sender, EventArgs e)
        {
            ArrayList dropTransitions = new ArrayList(transitionByCell.Count);

            foreach (TransitionInfo transition in transitionByCell.Values)
            {
                if (transition.IsAlive)
                    transition.ProcessTick();
                else
                {
                    transition.Finish();
                    dropTransitions.Add(transition);
                }
            }

            foreach (TransitionInfo dropped in dropTransitions)
                transitionByCell.Remove(dropped.Cell);

            UpdateTimer();
        }

        #region Nested type: TransitionInfo

        /// <summary>
        /// В этом классе хранится информация по чудесному превращению шашки по дороге жизни.
        /// Так сказать, в этих сухих цифрах закодирована текущая судьба этой цифровой инфузории.
        /// </summary>
        public class TransitionInfo
        {
            /// <summary>
            /// Какое время отводится на движение. После завершения этого периода наступает полное обездвиживание:ъ
            /// шашка приехала в пункт назначения, "судьба свершилась".
            /// Статическая видимость только для чтения означает всеобщую карму
            /// шашек как вида в рамках вселенной. Осмысленная жизнь шашки длится 1 секунду.
            /// </summary>
            private static readonly TimeSpan TransitionTime = TimeSpan.FromSeconds(1);

            /// <summary>
            /// Ссылка на шашку, которая управляется этой "судьбой".
            /// </summary>
            public readonly CellPlate Cell;

            /// <summary>
            /// Исходная позиция. Шашка с начала движения на самом деле находится уже в конце пути.
            /// Но иллюзорный мир цифр заставляет её думать, что она движется.
            /// </summary>
            public readonly Rectangle StartPosition;

            /// <summary>
            /// Время начала движения, день рождения.
            /// </summary>
            public readonly DateTime StartTime;


            /// <summary>
            /// Область, внутри которой происходит движение шашки. По Эйнштену это называется "горизонт событий".
            /// Это значение нужно для того, чтобы вызывать Invalidate контрола CellPoolControl по таймеру.
            /// </summary>
            public readonly Rectangle TransitionArea;

            /// <summary>
            /// Положение шашки на момент предыдущей отрисовки.
            /// </summary>
            public Rectangle LastPaintPosition;

            /// <summary>
            /// Создание "судьбы".
            /// </summary>
            /// <param name="cell">Шашка, которая поедет на своё место.</param>
            /// <param name="startPosition">Старое состояние шашки.</param>
            public TransitionInfo(
                CellPlate cell,
                Rectangle startPosition)
            {
                Cell = cell;
                StartPosition = startPosition;
                StartTime = DateTime.Now;

                TransitionArea = PaintUtils.Union(startPosition, cell.CellRectangle);
            }


            /// <summary>
            /// Пришла ли шашка на свой путь назначения? Закончилось ли отпущенное время?
            /// </summary>
            public bool IsAlive
            {
                get { return DateTime.Now < StartTime + TransitionTime; }
            }

            /// <summary>
            /// Свой срок земной пройдя наполовину...
            /// </summary>
            public decimal CompletionRatio
            {
                get
                {
                    TimeSpan lostTime = DateTime.Now - StartTime;

                    return (decimal) (lostTime.TotalMilliseconds/TransitionTime.TotalMilliseconds);
                }
            }

            /// <summary>
            /// Обозначает область контрола PoolCellControl как инвалидную.
            /// Windows сам вызывает перерисовку, когда это будет удобно.
            /// </summary>
            /// <remark>
            /// Такая анимация в придачу к DoubleBuffer не вызывает мерцания и гарантировано освобождает от артефактов.
            /// Кроме того, несколько воследовательных вызовов Invalidate приводят к единой отрисовке объединённого региона.
            /// Hard redraw must die.
            /// </remark>
            public void InvalidateTransitionArea()
            {
                Cell.Pool.Invalidate(TransitionArea);
            }


            /// <summary>
            /// Метод для вызова из таймера. Фактически, просто заставляет контрол перерисовать область.
            /// </summary>
            public void ProcessTick()
            {
                InvalidateTransitionArea();
            }

            /// <summary>
            /// Вызывается при смерти.
            /// </summary>
            public void Finish()
            {
                InvalidateTransitionArea();
            }
        }

        #endregion
    }
}