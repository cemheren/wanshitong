using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wanshitong.KeyAggregation
{
    public class KeyQueue
    {
        private class KeyGroup
        {
            public KeyGroup(string key, int group)
            {
                this.Key = key;
                this.Group = group;

            }
            public string Key { get; set; }
            public int Group { get; set; }
        }

        private ConcurrentDictionary<int, Deque<KeyGroup>> m_queues;

        private Action m_flushCallback;

        private object queue_lock = new object();

        private bool flushing = false;

        public KeyQueue(Action flushCallback)
        {
            m_queues = new ConcurrentDictionary<int, Deque<KeyGroup>>();
            m_flushCallback = flushCallback;
        }

        public void Add(string s, int group)
        {
            if (this.flushing)
            {
                lock (this.queue_lock)
                {
                    // wait the lock.
                }
            }

            if (s == "[return]")
            {
                m_flushCallback();
                return;
            }

            var queue = m_queues.GetOrAdd(group, new Deque<KeyGroup>());

            if (s == "[del]" && queue.Count > 0)
            {
                queue.RemoveFront();
                return;
            }

            if (s.First() == '[' && s.Last() == ']')
            {
                // ignore other characters for now. 
                return;
            }

            queue.AddFront(new KeyGroup(s, group));
        }

        public List<(int, string)> Flush()
        {
            var result = new List<(int, string)>();

            lock (this.queue_lock)
            {
                this.flushing = true;

                var localBuilder = new StringBuilder();
                foreach (var kvp in this.m_queues)
                {
                    localBuilder.Clear();
                    var queue = kvp.Value;
                    while (queue.Count > 0)
                    {   
                        var currentKey = queue.RemoveBack().Key;
                        var cmdOrCtrl = currentKey[0] == '[' && (currentKey.Contains("ctrl") || currentKey.Contains("cmd"));
                        if (cmdOrCtrl && queue.Count > 1)
                        {
                            var middleKey = queue.RemoveBack().Key;
                            var rightKey = queue.RemoveBack().Key;

                            //discard everything if keys are like [ctrl]a[ctrl]
                            if(currentKey == rightKey)
                            {
                                continue;
                            }
                        }
                        localBuilder.Append(currentKey);
                    }                
                    var str = localBuilder.ToString();
                    if (string.IsNullOrEmpty(str) == false)
                    {
                        result.Add((kvp.Key, str));                            
                    }
                    // maybe delete queues from m_queue after they are empty. This might create a memory leak.
                }

                this.flushing = false;
            }

            return result;
        }
    }
}