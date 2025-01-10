using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RunAtTime.Module
{
    public static class Interval
    {
        public static System.Timers.Timer Set(Action action, int interval)
        {
            try
            {
                action();
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
                Form1.formMain.updateNotiTextBox(err.Message);
            }

            var timer = new System.Timers.Timer(interval);
            timer.Elapsed += (s, e) => {
                timer.Enabled = false;
                try
                {
                    action();
                }
                catch (Exception err) {
                    Console.WriteLine(err); 
                    Form1.formMain.updateNotiTextBox(err.Message);
                }

                timer.Enabled = true;
            };
            timer.Enabled = true;
            return timer;
        }
        public static void Stop(System.Timers.Timer timer)
        {
            timer.Stop();
            timer.Dispose();
        }
    }

}
