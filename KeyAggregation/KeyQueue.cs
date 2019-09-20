using System;
using System.Collections.Generic;
using System.Linq;

namespace wanshitong.KeyAggregation
{
    public class KeyQueue
    {
        internal Deque<string> m_queue;
        
        private Action m_flushCallback;

        private object queue_lock = new object();

        private bool flushing = false;

        public KeyQueue(Action flushCallback)
        {
            m_queue = new Deque<string>();
            m_flushCallback = flushCallback;
        }

        public void Add(string s)
        {

            if(s == "[return]")
            {
                m_flushCallback();   
                return;
            }

            if (s == "[del]" && m_queue.Count > 0)
            {
                m_queue.RemoveFront();
                return;
            }

            if(s.First() == '[' && s.Last() == ']')
            {
                // ignore other characters for now. 
                return;   
            }

            lock (this.queue_lock)
            {
                m_queue.AddFront(s);            
            }
        }

        public string Flush()
        {
            var result = "";
            lock (this.queue_lock)
            {
                while (this.m_queue.Count > 0)
                {
                    result += m_queue.RemoveBack();
                }
            }

            return result;
        }
    }
}