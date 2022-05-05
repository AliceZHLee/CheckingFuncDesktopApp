using QuickFix;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ICEFixAdapter.MsgProcessor {
    internal class QueueReader {
        private ConcurrentQueue<Message> queue;
        private OTCFixApp messageProcessor;
        //private MessageProcessor messageProcessor;
        private volatile bool isActive = true;
        private Task mainTask;

        internal QueueReader(OTCFixApp processor) {
            messageProcessor = processor;
            queue = new ConcurrentQueue<Message>();

            mainTask = Task.Factory.StartNew(() => {
                while (isActive) {
                    try {
                        if (queue.TryDequeue(out Message message))
                            messageProcessor.Process(message);
                        else {
                            Thread.Sleep(20);
                        }
                    }
                    catch (ThreadInterruptedException) {
                        break;
                    }
                }
            });
        }

        internal bool Offer(Message message) {
            try {
                queue.Enqueue(message);
            }
            catch (Exception) {
                return false;
            }
            return true;
        }

        internal void Dispose() {
            try {
                isActive = false;
                queue.Clear();
                mainTask.Wait();
            }
            catch (Exception ex) {
            }
        }
    }
}

