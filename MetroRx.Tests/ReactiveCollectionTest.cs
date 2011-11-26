using System.Reactive.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using MetroRx;
using System.IO;
using System.Text;
using MetroRx.Testing;
using MetroRx.Tests;
using System.Runtime.Serialization.Json;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetroRx.Tests
{
    [TestClass]
    public class ReactiveCollectionTest : IEnableLogger
    {
        [TestMethod]
        public void CollectionCountChangedTest()
        {
            var fixture = new ReactiveCollection<int>();
            var before_output = new List<int>();
            var output = new List<int>();

            fixture.CollectionCountChanging.Subscribe(before_output.Add);
            fixture.CollectionCountChanged.Subscribe(output.Add);

            fixture.Add(10);
            fixture.Add(20);
            fixture.Add(30);
            fixture.RemoveAt(1);
            fixture.Clear();

            var before_results = new[] {0,1,2,3,2};
            Assert.AreEqual(before_results.Length, before_output.Count);
            before_results.AssertSequenceAreEqual(before_output);

            var results = new[]{1,2,3,2,0};
            results.AssertSequenceAreEqual(output);
        }

        [TestMethod]
        public void ItemsAddedAndRemovedTest()
        {
            var fixture = new ReactiveCollection<int>();
            var before_added = new List<int>();
            var before_removed = new List<int>();
            var added = new List<int>();
            var removed = new List<int>();

            fixture.BeforeItemsAdded.Subscribe(before_added.Add);
            fixture.BeforeItemsRemoved.Subscribe(before_removed.Add);
            fixture.ItemsAdded.Subscribe(added.Add);
            fixture.ItemsRemoved.Subscribe(removed.Add);

            fixture.Add(10);
            fixture.Add(20);
            fixture.Add(30);
            fixture.RemoveAt(1);
            fixture.Clear();

            var added_results = new[]{10,20,30};
            Assert.AreEqual(added_results.Length, added.Count);
            added_results.AssertSequenceAreEqual(added);

            var removed_results = new[]{20};
            Assert.AreEqual(removed_results.Length, removed.Count);
            removed_results.AssertSequenceAreEqual(removed);

            Assert.AreEqual(before_added.Count, added.Count);
            added.AssertSequenceAreEqual(before_added);

            Assert.AreEqual(before_removed.Count, removed.Count);
            removed.AssertSequenceAreEqual(before_removed);
        }

#if FALSE
        [TestMethod]
        public void CollectionsShouldntShareSubscriptions()
        {
            var fixture1 = new ReactiveCollection<TestFixture>() { ChangeTrackingEnabled = true };
            var fixture2 = new ReactiveCollection<TestFixture>() { ChangeTrackingEnabled = true };
            var item1 = new TestFixture() { IsOnlyOneWord = "Foo" };
            var output1 = new List<Tuple<TestFixture, string>>();
            var output2 = new List<Tuple<TestFixture, string>>();

            fixture1.ItemChanged.Subscribe(x => {
                output1.Add(new Tuple<TestFixture,string>((TestFixture)x.Sender, x.PropertyName));
            });

            fixture2.ItemChanged.Subscribe(x => {
                output2.Add(new Tuple<TestFixture,string>((TestFixture)x.Sender, x.PropertyName));
            });

            fixture1.Add(item1);
            fixture1.Add(item1);
            fixture2.Add(item1);
            fixture2.Add(item1);

            item1.IsOnlyOneWord = "Bar";
            Assert.Equal(1, output1.Count);
            Assert.Equal(1, output2.Count);

            fixture2.RemoveAt(0);

            item1.IsOnlyOneWord = "Baz";
            Assert.Equal(2, output1.Count);
            Assert.Equal(2, output2.Count);
        }

        [TestMethod]
        public void CreateCollectionWithoutTimer()
        {
            var input = new[] {"Foo", "Bar", "Baz", "Bamf"};
            var fixture = (new TestScheduler()).With(sched => {
                var f = input.ToObservable(sched).CreateCollection();

                sched.Start();
                return f;
            });
            
            input.AssertAreEqual(fixture);
        }

        [TestMethod]
        public void CreateCollectionWithTimer()
        {
            var input = new[] {"Foo", "Bar", "Baz", "Bamf"};
            var sched = new TestScheduler();

            using (TestUtils.WithScheduler(sched)) {
                ReactiveCollection<string> fixture;

                fixture = input.ToObservable(sched).CreateCollection(TimeSpan.FromSeconds(0.5));
                sched.RunToMilliseconds(1005);
                fixture.AssertAreEqual(input.Take(2));
                
                sched.RunToMilliseconds(1505);
                fixture.AssertAreEqual(input.Take(3));
    
                sched.RunToMilliseconds(10000);
                fixture.AssertAreEqual(input);
            }
        }

        [TestMethod]
        public void DerivedCollectionsShouldFollowBaseCollection()
        {
            var input = new[] {"Foo", "Bar", "Baz", "Bamf"};
            var fixture = new ReactiveCollection<TestFixture>(
                input.Select(x => new TestFixture() { IsOnlyOneWord = x }));

            var output = fixture.CreateDerivedCollection(new Func<TestFixture, string>(x => x.IsOnlyOneWord));

            input.AssertAreEqual(output);

            fixture.Add(new TestFixture() { IsOnlyOneWord = "Hello" });
            Assert.Equal(5, output.Count);
            Assert.Equal(output[4], "Hello");

            fixture.RemoveAt(4);
            Assert.Equal(4, output.Count);

            fixture[1] = new TestFixture() { IsOnlyOneWord = "Goodbye" };
            Assert.Equal(4, output.Count);
            Assert.Equal(output[1], "Goodbye");

            fixture.Clear();
            Assert.Equal(0, output.Count);
        }
#endif
    }
}
