// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using GraphQL.AspNet.Common.Generics;
    using NUnit.Framework;

    [TestFixture]
    public class OrderedDictionaryTests
    {
        private OrderedDictionary<string, string> GetAlphabetDictionary(IEqualityComparer<string> comparer = null)
        {
            OrderedDictionary<string, string> alphabet = new OrderedDictionary<string, string>(comparer);
            for (var a = Convert.ToInt32('a'); a <= Convert.ToInt32('z'); a++)
            {
                var c = Convert.ToChar(a);
                alphabet.Add(c.ToString(), c.ToString().ToUpper());
            }

            Assert.AreEqual(26, alphabet.Count);
            return alphabet;
        }

        private List<KeyValuePair<string, string>> GetAlphabetList()
        {
            var alphabet = new List<KeyValuePair<string, string>>();
            for (var a = Convert.ToInt32('a'); a <= Convert.ToInt32('z'); a++)
            {
                var c = Convert.ToChar(a);
                alphabet.Add(new KeyValuePair<string, string>(c.ToString(), c.ToString().ToUpper()));
            }

            Assert.AreEqual(26, alphabet.Count);
            return alphabet;
        }

        private class CustomIntComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return y - x;
            }
        }

        [Test]
        public void Constructor_FromOtherDictionary()
        {
            var od = new OrderedDictionary<string, string>();
            od.Add("key1", "value1");
            od.Add("key2", "value2");

            var od2 = new OrderedDictionary<string, string>(od);
            Assert.AreEqual(2, od2.Count);
            Assert.AreEqual("value1", od2["key1"]);
            Assert.AreEqual("value2", od2["key2"]);
        }

        [Test]
        public void Constructor_FromOtherDictionary_CaseInsensitive()
        {
            var od = new OrderedDictionary<string, string>();
            od.Add("key1", "value1");
            od.Add("key2", "value2");
            Assert.IsFalse(od.ContainsKey("KEY1"));

            var od2 = new OrderedDictionary<string, string>(od, StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(2, od2.Count);
            Assert.AreEqual("value1", od2["KEY1"]);
            Assert.AreEqual("value2", od2["KEY2"]);
        }

        [Test]
        public void Browser_PropertyCheck()
        {
            var dic = new OrderedDictionary<string, int>();
            dic.Add("key1", 1);
            dic.Add("key2", 2);

            var browser = new OrderedDictionaryDebugBrowser(dic);
            Assert.AreEqual(2, browser.Count);
            Assert.AreEqual(2, browser.Keys.Count);
            Assert.AreEqual(2, browser.Values.Count);
            Assert.AreEqual(2, browser.Items.Count);

            Assert.IsTrue(browser.Keys.Contains("key1"));
            Assert.IsTrue(browser.Keys.Contains("key2"));

            Assert.IsTrue(browser.Values.Contains(1));
            Assert.IsTrue(browser.Values.Contains(2));

            Assert.IsTrue(browser.Items.Contains(new KeyValuePair<object, object>("key1", 1)));
            Assert.IsTrue(browser.Items.Contains(new KeyValuePair<object, object>("key2", 2)));

            dic["key1"] = 3;
            Assert.AreEqual(3, dic["key1"]);
        }

        [Test]
        public void DuplicateAdd_ThrowsError()
        {
            var dic = new OrderedDictionary<string, int>();
            dic.Add("key1", 1);

            Assert.Throws<ArgumentException>(() => { dic.Add("key1", 1); });
        }

        [Test]
        public void ICollectionKVP_CastingCheck()
        {
            var dic = new OrderedDictionary<string, int>();

            var kvp = new KeyValuePair<string, int>("key1", 3);
            var col = dic as ICollection<KeyValuePair<string, int>>;
            col.Add(kvp);
            Assert.AreEqual("key1", col.First().Key);
            Assert.AreEqual(3, col.First().Value);

            Assert.IsTrue(col.Contains(kvp));
            Assert.IsFalse(col.IsReadOnly);

            col.Remove(kvp);
            Assert.AreEqual(0, col.Count);

            col.Add(kvp);
            Assert.AreEqual(1, col.Count);
            col.Clear();
            Assert.AreEqual(0, col.Count);

            col.Add(kvp);

            var i = 0;
            foreach (var _ in col)
                i++;
            Assert.AreEqual(1, i);
        }

        [Test]
        public void IDicionaryKVP_CastingCheck()
        {
            var od = new OrderedDictionary<string, string>() as IDictionary<string, string>;
            Assert.AreEqual(0, od.Count);

            od.Add("foo", "bar");
            Assert.AreEqual(1, od.Count);
            Assert.AreEqual(od["foo"], "bar");
            Assert.AreEqual(od.Keys.FirstOrDefault(), "foo");
            Assert.AreEqual(od.Values.FirstOrDefault(), "bar");
            Assert.IsTrue(od.ContainsKey("foo"));

            var i = 0;
            foreach (var _ in od)
                i++;
            Assert.AreEqual(1, i);

            var success = od.TryGetValue("foo", out var k);
            Assert.IsTrue(success);
            Assert.AreEqual("bar", k);

            od["foo"] = "bab";
            Assert.AreEqual("bab", od["foo"]);

            od.Remove("foo");
            Assert.AreEqual(0, od.Count);

            var arr = new KeyValuePair<string, string>[1];
            od.CopyTo(arr, 0);
            Assert.IsNotNull(arr[0]);
        }

        [Test]
        public void IOrderedDictionary_CastingCheck()
        {
            var od = new OrderedDictionary<string, string>() as IOrderedDictionary;
            Assert.AreEqual(0, od.Count);

            // IOrderedDictionary.Insert
            od.Insert(0, "foo", "bar");
            Assert.AreEqual(1, od.Count);
            Assert.AreEqual(od["foo"], "bar");
            Assert.AreEqual("bar", od[0]);
            Assert.AreEqual("bar", od["foo"]);

            // IOrderedDictionary.GetEnumerator
            var i = 0;
            foreach (var _ in od)
                i++;
            Assert.AreEqual(1, i);

            // IOrderedDictionary.RemoveAt
            od.RemoveAt(0);
            Assert.AreEqual(0, od.Count);

            // IOrderedDictionary[index]
            od["foo"] = "bab";
            Assert.AreEqual("bab", od["foo"]);

            od[0] = "bob";
            Assert.AreEqual("bob", od["foo"]);

            var arr = new KeyValuePair<string, string>[1];

            // IOrderedDictionary.CopyTo
            od.CopyTo(arr, 0);
            Assert.IsNotNull(arr[0]);
        }

        [Test]
        public void ICollection_CastingCheck()
        {
            var dictionary = GetAlphabetDictionary();
            var collection = dictionary as ICollection;

            Assert.IsNotNull(collection.SyncRoot);
            Assert.IsFalse(collection.IsSynchronized);
            Assert.AreEqual(26, collection.Count);

            var array = new KeyValuePair<string, string>[26];
            collection.CopyTo(array, 0);
            CollectionAssert.AreEqual(dictionary, array);
        }

        [Test]
        public void IDictionary_CastingCheck()
        {
            var dictionary = GetAlphabetDictionary();
            var idic = dictionary as IDictionary;

            Assert.IsNotNull(idic);
            Assert.AreEqual(26, idic.Count);
            Assert.AreEqual(dictionary.IsReadOnly, idic.IsReadOnly);
            Assert.IsFalse(idic.IsFixedSize);
            CollectionAssert.AreEqual(dictionary.Values, idic.Values);

            Assert.IsTrue(idic.Contains("q"));

            idic.Clear();
            Assert.AreEqual(0, dictionary.Count);
        }

        [Test]
        public void Add()
        {
            var od = new OrderedDictionary<string, string>();
            Assert.AreEqual(0, od.Count);
            Assert.AreEqual(-1, od.IndexOf("foo"));

            od.Add("foo", "bar");
            Assert.AreEqual(1, od.Count);
            Assert.AreEqual(0, od.IndexOf("foo"));
            Assert.AreEqual(od[0], "bar");
            Assert.AreEqual(od["foo"], "bar");
            Assert.AreEqual(od.GetItem(0).Key, "foo");
            Assert.AreEqual(od.GetItem(0).Value, "bar");
        }

        [Test]
        public void Remove()
        {
            var od = new OrderedDictionary<string, string>();

            od.Add("foo", "bar");
            Assert.AreEqual(1, od.Count);

            od.Remove("foo");
            Assert.AreEqual(0, od.Count);
        }

        [Test]
        public void RemoveAt()
        {
            var od = new OrderedDictionary<string, string>();

            od.Add("foo", "bar");
            Assert.AreEqual(1, od.Count);

            od.RemoveAt(0);
            Assert.AreEqual(0, od.Count);

            od.Add("foo", "bar");
            Assert.Throws<ArgumentException>(() => od.RemoveAt(2));
        }

        [Test]
        public void Clear()
        {
            var od = this.GetAlphabetDictionary();
            Assert.AreEqual(26, od.Count);
            od.Clear();
            Assert.AreEqual(0, od.Count);
        }

        [Test]
        public void OrderIsPreserved()
        {
            var alphabetDict = this.GetAlphabetDictionary();
            var alphabetList = this.GetAlphabetList();
            Assert.AreEqual(26, alphabetDict.Count);
            Assert.AreEqual(26, alphabetList.Count);

            var keys = alphabetDict.Keys.ToList();
            var values = alphabetDict.Values.ToList();

            for (var i = 0; i < 26; i++)
            {
                var dictItem = alphabetDict.GetItem(i);
                var listItem = alphabetList[i];
                var key = keys[i];
                var value = values[i];

                Assert.AreEqual(dictItem, listItem);
                Assert.AreEqual(key, listItem.Key);
                Assert.AreEqual(value, listItem.Value);
            }
        }

        [Test]
        public void TryGetValue()
        {
            var alphabetDict = this.GetAlphabetDictionary();
            string result = null;
            Assert.IsFalse(alphabetDict.TryGetValue("abc", out result));
            Assert.IsNull(result);
            Assert.IsTrue(alphabetDict.TryGetValue("z", out result));
            Assert.AreEqual("Z", result);
        }

        [Test]
        public void Enumerator()
        {
            var alphabetDict = this.GetAlphabetDictionary();

            var keys = alphabetDict.Keys.ToList();
            Assert.AreEqual(26, keys.Count);

            foreach (var kvp in alphabetDict)
            {
                var value = alphabetDict[kvp.Key];
                Assert.AreEqual(kvp.Value, value);
            }
        }

        [Test]
        public void InvalidIndex()
        {
            var alphabetDict = this.GetAlphabetDictionary();
            try
            {
                var _ = alphabetDict[100];
                Assert.IsTrue(false, "Exception should have thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("The index was outside the bounds of the dictionary"));
            }
        }

        [Test]
        public void MissingKey()
        {
            var alphabetDict = this.GetAlphabetDictionary();
            try
            {
                var _ = alphabetDict["abc"];
                Assert.IsTrue(false, "Exception should have thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("key is not present"));
            }
        }

        [Test]
        public void UpdateExistingValue()
        {
            var alphabetDict = this.GetAlphabetDictionary();
            Assert.IsTrue(alphabetDict.ContainsKey("c"));
            Assert.AreEqual(2, alphabetDict.IndexOf("c"));
            Assert.AreEqual(alphabetDict[2], "C");
            alphabetDict[2] = "CCC";
            Assert.IsTrue(alphabetDict.ContainsKey("c"));
            Assert.AreEqual(2, alphabetDict.IndexOf("c"));
            Assert.AreEqual(alphabetDict[2], "CCC");
        }

        [Test]
        public void InsertValue()
        {
            var alphabetDict = this.GetAlphabetDictionary();
            Assert.IsTrue(alphabetDict.ContainsKey("c"));
            Assert.AreEqual(2, alphabetDict.IndexOf("c"));
            Assert.AreEqual(alphabetDict[2], "C");
            Assert.AreEqual(26, alphabetDict.Count);
            Assert.IsFalse(alphabetDict.ContainsValue("ABC"));

            alphabetDict.Insert(2, "abc", "ABC");
            Assert.IsTrue(alphabetDict.ContainsKey("c"));
            Assert.AreEqual(2, alphabetDict.IndexOf("abc"));
            Assert.AreEqual(alphabetDict[2], "ABC");
            Assert.AreEqual(27, alphabetDict.Count);
            Assert.IsTrue(alphabetDict.ContainsValue("ABC"));
        }

        [Test]
        public void ValueComparer()
        {
            var alphabetDict = this.GetAlphabetDictionary();
            Assert.IsFalse(alphabetDict.ContainsValue("a"));
            Assert.IsTrue(alphabetDict.ContainsValue("a", StringComparer.OrdinalIgnoreCase));
        }

        [Test]
        public void SetValue()
        {
            var alphabet = GetAlphabetDictionary();

            Assert.AreEqual("A", alphabet["a"]);

            alphabet.SetValue("a", "qq");
            Assert.AreEqual("qq", alphabet["a"]);

            alphabet.SetValue("qq", "a");
            Assert.AreEqual("a", alphabet["qq"]);
            Assert.AreEqual(27, alphabet.Count);
        }

        [Test]
        public void SetItem()
        {
            var alphabet = GetAlphabetDictionary();

            Assert.AreEqual("A", alphabet["a"]);

            // first index is "a" ensure value is set to it
            alphabet.SetItem(0, "qq");
            Assert.AreEqual("qq", alphabet["a"]);

            // less than 0
            Assert.Throws<ArgumentOutOfRangeException>(() => alphabet.SetItem(-1, "bb"));

            // greater than list length
            Assert.Throws<ArgumentOutOfRangeException>(() => alphabet.SetItem(alphabet.Count + 1, "bb"));
        }

        [Test]
        public void SortByKeys()
        {
            var alphabetDict = this.GetAlphabetDictionary();
            var reverseAlphabetDict = this.GetAlphabetDictionary();
            int StringReverse(string x, string y) => string.Equals(x, y) ? 0 : string.CompareOrdinal(x, y) >= 1 ? -1 : 1;
            reverseAlphabetDict.SortKeys(StringReverse);
            for (int j = 0, k = 25; j < alphabetDict.Count; j++, k--)
            {
                var ascValue = alphabetDict.GetItem(j);
                var dscValue = reverseAlphabetDict.GetItem(k);
                Assert.AreEqual(ascValue.Key, dscValue.Key);
                Assert.AreEqual(ascValue.Value, dscValue.Value);
            }
        }

        [Test]
        public void DelegateKeyCollection_Sort()
        {
            var col = new DelegateKeyedCollection<int, string>((s) => s == null ? 0 : s.Length);
            col.Add("string1");
            col.Add("string333");
            col.Add("string22");

            Assert.AreEqual("string1", col.ElementAt(0));
            Assert.AreEqual("string333", col.ElementAt(1));
            Assert.AreEqual("string22", col.ElementAt(2));

            col.Sort();
            Assert.AreEqual("string1", col.ElementAt(0));
            Assert.AreEqual("string22", col.ElementAt(1));
            Assert.AreEqual("string333", col.ElementAt(2));

            Assert.AreEqual("string22", col[8]);
        }

        [Test]
        public void DelegateKeyCollection_Sort_ItemComparer()
        {
            var col = new DelegateKeyedCollection<int, string>((s) => s == null ? 0 : s.Length);
            col.Add("string1");
            col.Add("string333");
            col.Add("string22");

            Assert.AreEqual("string1", col.ElementAt(0));
            Assert.AreEqual("string333", col.ElementAt(1));
            Assert.AreEqual("string22", col.ElementAt(2));

            // reverse list by item value (not key)
            col.Sort((x, y) => string.Compare(y, x, StringComparison.Ordinal));
            Assert.AreEqual("string333", col.ElementAt(0));
            Assert.AreEqual("string22", col.ElementAt(1));
            Assert.AreEqual("string1", col.ElementAt(2));

            Assert.AreEqual("string22", col[8]);
        }

        [Test]
        public void DelegateKeyCollection_SortByKeys()
        {
            var col = new DelegateKeyedCollection<int, string>((s) => s == null ? 0 : s.Length);
            col.Add("string1");
            col.Add("string333");
            col.Add("string22");

            Assert.AreEqual("string1", col.ElementAt(0));
            Assert.AreEqual("string333", col.ElementAt(1));
            Assert.AreEqual("string22", col.ElementAt(2));

            col.SortByKeys();
            Assert.AreEqual("string1", col.ElementAt(0));
            Assert.AreEqual("string22", col.ElementAt(1));
            Assert.AreEqual("string333", col.ElementAt(2));

            Assert.AreEqual("string22", col[8]);
        }

        [Test]
        public void DelegateKeyCollection_SortByKeys_CustomComparer()
        {
            var col = new DelegateKeyedCollection<int, string>((s) => s == null ? 0 : s.Length);
            col.Add("string1");
            col.Add("string333");
            col.Add("string22");

            Assert.AreEqual("string1", col.ElementAt(0));
            Assert.AreEqual("string333", col.ElementAt(1));
            Assert.AreEqual("string22", col.ElementAt(2));

            col.SortByKeys(new CustomIntComparer());
            Assert.AreEqual("string333", col.ElementAt(0));
            Assert.AreEqual("string22", col.ElementAt(1));
            Assert.AreEqual("string1", col.ElementAt(2));

            Assert.AreEqual("string22", col[8]);
        }

        [Test]
        public void DelegateKeyCollection_SortByKeys_ComparrisonDelegate()
        {
            var col = new DelegateKeyedCollection<int, string>((s) => s == null ? 0 : s.Length);
            col.Add("string1");
            col.Add("string333");
            col.Add("string22");

            Assert.AreEqual("string1", col.ElementAt(0));
            Assert.AreEqual("string333", col.ElementAt(1));
            Assert.AreEqual("string22", col.ElementAt(2));

            col.SortByKeys((x, y) => y - x);
            Assert.AreEqual("string333", col.ElementAt(0));
            Assert.AreEqual("string22", col.ElementAt(1));
            Assert.AreEqual("string1", col.ElementAt(2));

            Assert.AreEqual("string22", col[8]);
        }

        [Test]
        public void DelegateKeyCollection_PropertyCheck()
        {
            var col = new DelegateKeyedCollection<int, string>((s) => s == null ? 0 : s.Length);
            col.Add("string1");
            col.Add("string333");
            col.Add("string22");

            Assert.AreEqual(3, col.Keys.Count);
            Assert.AreEqual(3, col.Values.Count);
        }

        [Test]
        public void DictionaryEnumerator_Reset_YieldsFirstItemAgain()
        {
            var dic = new OrderedDictionary<string, string>();
            dic.Add("a", "string1");
            dic.Add("b", "string2");

            var enumerator = new DictionaryEnumerator<string, string>(dic);
            enumerator.MoveNext();

            var firstValue = enumerator.Value;

            enumerator.Reset();
            enumerator.MoveNext();
            var firstValueAfterReset = enumerator.Value;

            Assert.AreEqual(firstValue, firstValueAfterReset);
        }

        [Test]
        public void DictionaryEnumerator_PropertyCheck()
        {
            var dic = new OrderedDictionary<string, string>();
            dic.Add("a", "string1");
            dic.Add("b", "string2");

            var enumerator = new DictionaryEnumerator<string, string>(dic);
            enumerator.MoveNext();

            Assert.AreEqual("a", enumerator.Key);
            Assert.AreEqual("string1", enumerator.Value);
        }
    }
}