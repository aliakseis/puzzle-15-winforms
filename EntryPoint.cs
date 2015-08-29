using System;
using System.Windows.Forms;

namespace Games.x15
{
    /// <summary>
    /// Стандартные проект, создаваемый Visual Studio, добавляет входную точку
    /// в первую попавшуюся форму. Я всегда выделяю для этого отдельный класс.
    /// Гигиена — залог хорошего здоровья.
    /// </summary>
    internal class EntryPoint
    {
        /// <summary>
        /// Точка входа приложения.
        /// Заодно к стандартному запуску формы отображает исключения.
        /// Тоже мой стандартный подход — так мы всегда получаем точную информацию,
        /// даже если клиент Shift от Reset'а не отличает.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.DoEvents();
                Application.Run(new PlayForm());
            }
            catch (Exception err)
            {
                using (ThreadExceptionDialog errDlg = new ThreadExceptionDialog(err))
                {
                    errDlg.ShowDialog();
                }
            }
        }
    }
}