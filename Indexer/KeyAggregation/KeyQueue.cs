using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wanshitong.KeyAggregation
{
    public class KeyQueue
    {
        private class KeyGroup
        {
            public KeyGroup(string key, string group)
            {
                this.Key = key;
                this.Group = group;

            }
            public string Key { get; set; }
            public string Group { get; set; }
        }

        private Deque<KeyGroup> m_queue;

        private Action m_flushCallback;

        private object queue_lock = new object();

        private bool flushing = false;

        public KeyQueue(Action flushCallback)
        {
            m_queue = new Deque<KeyGroup>();
            m_flushCallback = flushCallback;
        }

        public void Add(string s, string group)
        {
            if (s == "[return]")
            {
                m_flushCallback();
                return;
            }

            if (s == "[del]" && m_queue.Count > 0)
            {
                m_queue.RemoveFront();
                return;
            }

            if (s.First() == '[' && s.Last() == ']')
            {
                // ignore other characters for now. 
                return;
            }

            lock (this.queue_lock)
            {
                m_queue.AddFront(new KeyGroup(s, group));
            }
        }

        public List<(string, string)> Flush()
        {
            var result = new List<(string, string)>();

            var localBuilder = new StringBuilder();
            lock (this.queue_lock)
            {
                var lastGroup = "";
                while (this.m_queue.Count > 0)
                {
                    var current = m_queue.RemoveBack();
                    if (lastGroup == "") 
                    {
                        lastGroup = current.Group;
                    }

                    if(current.Group == lastGroup)
                    {
                        localBuilder.Append(current.Key);
                    }
                    else
                    {
                        result.Add((lastGroup, localBuilder.ToString()));

                        lastGroup = "";
                        m_queue.AddBack(current);
                    }
                }

                if(lastGroup != "")
                {
                    result.Add((lastGroup, localBuilder.ToString()));
                }
            }

            return result;
        }
    }
}