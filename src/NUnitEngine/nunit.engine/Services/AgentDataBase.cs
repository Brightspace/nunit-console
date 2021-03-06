﻿// ***********************************************************************
// Copyright (c) 2011-2016 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NUnit.Engine.Services
{
    internal class AgentRecord
    {
        public Guid Id;
        public Process Process;
        public ITestAgent Agent;
        public AgentStatus Status;

        public AgentRecord(Guid id, Process p, ITestAgent a, AgentStatus s)
        {
            this.Id = id;
            this.Process = p;
            this.Agent = a;
            this.Status = s;
        }

    }

    /// <summary>
    ///  A simple class that tracks data about this
    ///  agencies active and available agents.
    ///  This class is required to be multi-thread safe.
    /// </summary>
    internal class AgentDataBase
    {
        private readonly Dictionary<Guid, AgentRecord> _agentData = new Dictionary<Guid, AgentRecord>();
        private readonly object _lock = new object();

        public AgentRecord this[Guid id]
        {
            get
            {
                lock (_lock)
                {
                    return _agentData[id];
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _agentData.Count;
                }
            }
        }

        // Take a snapshot of the database - used primarily in testing.
        public Snapshot TakeSnapshot()
        {
            lock (_lock)
            {
                return new Snapshot(_agentData.Values);
            }
        }

        public void Add(AgentRecord r)
        {
            lock (_lock)
            {
                _agentData[r.Id] = r;
            }
        }

        #region Methods not currently used

        // These methods are not currently used, but are  being
        // maintained (and tested) for now since TestAgency is 
        // undergoing some changes and may need them again.

        public void Remove(Guid agentId)
        {
            lock (_lock)
            {
                _agentData.Remove(agentId);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _agentData.Clear();
            }
        }

        #endregion

        #region Nested Snapshot Class used in Testing

        public class Snapshot
        {
            public Snapshot(IEnumerable<AgentRecord> data)
            {
                Guids = new List<Guid>();
                Records = new List<AgentRecord>();

                foreach(var record in data)
                {
                    Guids.Add(record.Id);
                    Records.Add(record);
                }
            }

            public ICollection<Guid> Guids { get; private set; }
            public ICollection<AgentRecord> Records { get; private set; }
        }

        #endregion
    }
}
