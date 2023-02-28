using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace VM.Data.Queue
{/// <summary>
 /// Summary description for Queue.
 /// </summary>
    public class Queue
    {
        private int maxQueueSize = 0;
        private ArrayList queueData = new ArrayList();
        private object mutex;

        /// <summary>
        /// CTOR. Default queue size is 100000
        /// </summary>
        public Queue()
        {
            maxQueueSize = 100000;
            mutex = this;
        }

        /// <summary>
        /// CTOR. With the max queue size
        /// </summary>
        /// <param name="maxSize"></param>
        public Queue(int maxSize)
        {
            maxQueueSize = maxSize;
            mutex = this;
        }

        /// <summary>
        /// Current count of the elements in the queue.     
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            lock (mutex)
            {
                return queueData.Count;
            }
        }


        /// <summary>
        /// If there is no element in the queue.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            lock (mutex)
            {
                return (queueData.Count == 0);
            }
        }


        /// <summary>
        /// Removes first element form the queue and returns it.
        /// If the queue is empty, returns null.
        /// </summary>
        /// <returns></returns>
        public object Dequeue()
        {
            lock (mutex)
            {
                object first = null;
                if (queueData.Count > 0)
                {
                    first = queueData[0];
                    queueData.RemoveAt(0);
                }
                return first;
            }
        }



        /// <summary>
        /// Tries to find the provided element in the queue and if found,
        /// removes it from the queue and returns it.
        /// If the element is not found returns null.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object Dequeue(object obj)
        {
            object found = null;
            lock (mutex)
            {
                found = queueData.Contains(obj);
                if (found != null)
                {
                    queueData.Remove(obj);
                }
            }
            return obj;
        }



        /// <summary>
        /// Appends an element to the end of the queue. If the queue
        /// has set limit on maximum elements and there is already specified
        /// max count of elements in the queue throws IndexOutOfRangeException.
        /// </summary>
        /// <param name="obj"></param>
        public void Enqueue(Object obj)
        {
            lock (mutex)
            {
                if (queueData.Count >= maxQueueSize)
                {
                    throw new IndexOutOfRangeException("Queue is full. Element not added.");
                }
                queueData.Add(obj);
            }
        }

        public void Clear()
        {
            lock (mutex)
            {
                if (queueData.Count >= 0)
                {
                    queueData.Clear();
                }
            }
        }


        public void Remove(object obj)
        {
            lock (mutex)
            {
                if (queueData.Count >= 0)
                {
                    queueData.Remove(obj);
                }
            }
        }

        /// <summary>
        /// Searches the queue to find the provided element.
        /// Uses <code>equals</code> method to compare elements.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object Find(object obj)
        {
            lock (mutex)
            {
                object current;
                IEnumerator iter = queueData.GetEnumerator();
                while (iter.MoveNext())
                {
                    current = iter.Current;
                    if (current.Equals(obj))
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public ArrayList ToArrayList()
        {
            return queueData;
        }

        public ArrayList ToArrayListAndClear()
        {
            lock (mutex)
            {
                ArrayList result = new ArrayList();
                if (queueData.Count >= 0)
                {
                    result.AddRange(queueData);
                    queueData.Clear();
                }
                return result;
            }
        }

        /// <summary>
        /// Save data on queue to file
        /// </summary>
        /// <param name="file_name"></param>
        public void SaveQueue(string file_name)
        {
            if (File.Exists(file_name))
            {
                File.Delete(file_name);
            }
            using (Stream st = File.Create(file_name))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(st, queueData);
            }
        }

        /// <summary>
        /// Load data on file to queue
        /// </summary>
        /// <param name="file_name"></param>
        /// <returns></returns>
        public int LoadQueue(string file_name)
        {
            if (File.Exists(file_name))
            {
                using (Stream st = File.OpenRead(file_name))
                {
                    if (st.Length > 0)
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        queueData = (ArrayList)bf.Deserialize(st);
                    }
                }
            }
            return queueData.Count;
        }
    }
}
