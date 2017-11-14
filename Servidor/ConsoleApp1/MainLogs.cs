using Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Messaging;

namespace Log
{
    public static class MainLogs
    {
        private static void Main(string[] args)
        {
            StartLogs();
        }
        public static void StartLogs() {
            CreateQueue();
            Thread thdListener = new Thread(new ThreadStart(QThread));
            thdListener.Start();
        }
        public static void QThread()
        {
            string queuePath = ".\\private$\\test";
            MessageQueue queue = new MessageQueue(queuePath);
            System.Messaging.Message msg;
            ((XmlMessageFormatter)queue.Formatter).TargetTypes =
            new Type[1];
            ((XmlMessageFormatter)queue.Formatter).TargetTypes[0] =
            "".GetType();
            while (true)
            {
                msg = queue.Receive();
                Console.WriteLine((string)msg.Body);
            }
        }
        public static void CreateQueue()
        {
            string queueName = ".\\private$\\test";
            MessageQueue mq;
            if (MessageQueue.Exists(queueName))
            {
                mq = new MessageQueue(queueName);
            }
            else
            {
                mq = MessageQueue.Create(queueName);
            }
        }
    }
}
