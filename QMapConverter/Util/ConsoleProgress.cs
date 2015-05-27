using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMapConverter.Util
{
    public class ConsoleProgress : IDisposable
    {
        String workItem_;
        int taskCount_;
        int completedCount_ = 0;
        bool finished_ = false;

        public ConsoleProgress(String progressItem, int taskCount)
        {
            workItem_ = progressItem;
            taskCount_ = taskCount;
            Console.WriteLine("");
            Console.Write(String.Format("{0}... 0%", workItem_));
        }

        public void Increment()
        {
            completedCount_ += 1;
        }

        public void Increment(int ct)
        {
            completedCount_ += ct;
        }

        public void Write()
        {
            Console.Write(String.Format("\r{0}... {1}%", workItem_, Progress()));
        }

        public void Finish()
        {
            if (!finished_)
                Console.WriteLine(String.Format("\rCompleted {0}      \r", workItem_));
            finished_ = true;
        }

        public void Terminate()
        {
            if (!finished_)
                Console.Write(String.Format("\r{0} Terminated at {1}%\r", workItem_, Progress()));
            finished_ = true;
        }

        int Progress()
        {
            float percentage = ((float)completedCount_) / ((float)taskCount_) * 100.0f;
            return (int)percentage;
        }

        public void Dispose()
        {
            Finish();
        }
    }
}
