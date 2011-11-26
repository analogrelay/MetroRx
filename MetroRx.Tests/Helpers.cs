using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetroRx.Tests
{
    public static class EnumerableTestMixin
    {
        public static void AssertSequenceAreEqual<T>(this IEnumerable<T> expected, IEnumerable<T> results)
        {
            Assert.AreEqual(expected.Count(), results.Count());
            foreach (var v in expected.Zip(results, (e, a) => new { e, a }))
            {
                Assert.AreEqual(v.e, v.a);
            }
        }
    }
}
