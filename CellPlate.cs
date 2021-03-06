using System;
using System.Drawing;

namespace Games.x15
{
    /// <summary>
    /// Шашка для взрослой игры в 15.
    /// Используется в детских осьмушках, так как промышленность не выпускает шашек-осьмушек.
    /// Вся игра вращается вокруг простой шашки, пока шашка скользит по контролу.
    /// </summary>
    public class CellPlate
    {
        /// <summary>
        /// Шашка помнит своё поле и навсегда к нему привязана.
        /// Это проявляется в модификаторе 'readonly'.
        /// </summary>
        private readonly CellTable m_Table;


        /// <summary>
        /// Надпись на шашке очень удобна цифр, букв и кратких ёмких слов по делу.
        /// </summary>
        private string m_Caption;

        /// <summary>
        /// Местонахождение шашки. В процессе игры обычно многократно изменяется.
        /// </summary>
        private Rectangle m_CellRectangle;


        /// <summary>
        /// Позиция (колонка) в игровом поле, где находится шашка в данный момент времени.
        /// </summary>
        private int m_X;

        /// <summary>
        /// Позиция, строка, в игровом поле.
        /// </summary>
        private int m_Y;


        /// <summary>
        /// Конструктор для шашки помечен internal, ведь никому не дано создать шашку,
        /// кроме соответствующих уполномоченых органов.
        /// </summary>
        /// <param name="table">Игровое поле для игры в осьмушки. Шашка при рождении запоминает
        /// это поле и хранит эту память всю жизнь.</param>
        /// <param name="x">Начальная позиция в игровом поле (колонка).</param>
        /// <param name="y">Начальная позиция (колонка).</param>
        internal CellPlate(CellTable table, int x, int y)
        {
            if (table == null) throw new ArgumentNullException("table");

            m_Table = table;
            m_X = x;
            m_Y = y;
        }

        /// <summary>
        /// Ссылка на материнское игровое поле. Эта информация считается сокровенной
        /// и может быть раскрыта только близким.
        /// </summary>
        internal CellTable Table
        {
            get { return m_Table; }
        }

        /// <summary>
        /// Контрол, который хранит в себе игровое поле и отрисовывает его.
        /// В сущности, для получения информации об отцовском контроле, шашка переспрашивает материнское игровое поле.
        /// </summary>
        internal CellPoolControl Pool
        {
            get { return Table.Pool; }
        }


        /// <summary>
        /// Краткая надпись на шашке. Обычно пишут цифру.
        /// </summary>
        public string Caption
        {
            get { return m_Caption; }
            set
            {
                if (value == m_Caption)
                    return;

                m_Caption = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Текущая позиция в игровом поле (колонка).
        /// </summary>
        public int X
        {
            get { return m_X; }
        }

        /// <summary>
        /// Текущая позиция в игровом поле (строка).
        /// </summary>
        public int Y
        {
            get { return m_Y; }
        }

        /// <summary>
        /// Текущее положение шашки. Только для чтения.
        /// </summary>
        public Rectangle CellRectangle
        {
            get
            {
                Table.NeedCellRectangle();
                return m_CellRectangle;
            }
        }

        /// <summary>
        /// Задвигает шашку в другое место.
        /// </summary>
        /// <param name="relativeX">Колонка другого места.</param>
        /// <param name="relativeY">Строка другого места.</param>
        public void ShiftRelative(int relativeX, int relativeY)
        {
            Rectangle oldRect = CellRectangle;

            Table.InternalShiftCellRelative(this, relativeX, relativeY, true);
            Pool.AddTransition(this, oldRect);

//            Invalidate();
//            Pool.Invalidate(oldRect);
        }

        /// <summary>
        /// Задвигает шашку в другое место.
        /// </summary>
        /// <param name="relativeCoord">Координаты другого места.</param>
        public void ShiftRelative(Point relativeCoord)
        {
            ShiftRelative(relativeCoord.X, relativeCoord.Y);
        }

        /// <summary>
        /// Специальный скрытый метод, которым свои могут моментально изменить положение шашки.
        /// Но кому попало это сделать нельзя. Посторонние использовать разве что ShiftRelative.
        /// </summary>
        /// <param name="cellRectangle">Новое положение шашки.</param>
        internal void SetCellRectangle(Rectangle cellRectangle)
        {
            m_CellRectangle = cellRectangle;
        }

        /// <summary>
        /// Специальный скрытый метод, которым свои могут моментально изменить положение шашки.
        /// Но кому попало это сделать нельзя. Посторонние использовать разве что ShiftRelative.
        /// </summary>
        /// <param name="x">Новая координата шашки в игровом поле (колонка).</param>
        /// <param name="y">Новая координата (строка).</param>
        internal void SetXY(int x, int y)
        {
            m_X = x;
            m_Y = y;
        }

        /// <summary>
        /// Подать документы на перерисовку шашки.
        /// </summary>
        internal void Invalidate()
        {
            Pool.Invalidate(CellRectangle);
        }
    }
}