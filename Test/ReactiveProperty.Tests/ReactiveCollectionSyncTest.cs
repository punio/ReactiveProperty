using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reactive.Bindings;

namespace ReactiveProperty.Tests
{
    [TestClass]
    public class ReactiveCollectionSyncTest
    {
        private ReactiveCollectionSync<int> target;

        [TestInitialize]
        public void Initialize()
        {
            target = new ReactiveCollectionSync<int>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            target = null;
        }

        [TestMethod]
        public void Create()
        {
            target.Count.Is(0);
        }

        [TestMethod]
        public void Add()
        {
            target.Clear();
            target.Add(0);
            target.Add(1);
            target.Is(0, 1);
        }

        [TestMethod]
        public void AddRange()
        {
            target.Clear();
            target.AddRange(0, 1, 2);
            target.Is(0, 1, 2);
        }

        [TestMethod]
        public void AddRangeAsync()
        {
            Task.Run(() =>
            {
                var sync = new ReactiveCollectionSync<int>();
                sync.Clear();
                sync.AddRange(0, 1, 2);
                Assert.AreEqual(sync.Count, 3);

                var async = new ReactiveCollection<int>(Scheduler.ThreadPool);
                async.AddRangeOnScheduler(0, 1, 2);
                Assert.AreNotEqual(async.Count, 3);
            });
        }

        [TestMethod]
        public void Insert()
        {
            target.Clear();
            target.AddRange(0, 1);
            target.Insert(1, 2);
            target.Is(0, 2, 1);
        }

        [TestMethod]
        public void RemoveAt()
        {
            target.Clear();
            target.AddRange(0, 1, 2);
            target.RemoveAt(0);
            target.Is(1, 2);
        }
    }
}
