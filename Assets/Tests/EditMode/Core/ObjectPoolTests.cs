using NUnit.Framework;
using UnityEngine;
using ValenthiaChronicles.Core.Patterns;

namespace ValenthiaChronicles.Tests.EditMode.Core
{
    public class ObjectPoolTests
    {
        private GameObject _prefabGO;
        private BoxCollider _prefab;

        [SetUp]
        public void SetUp()
        {
            _prefabGO = new GameObject("PoolTestPrefab");
            _prefab = _prefabGO.AddComponent<BoxCollider>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_prefabGO);
        }

        [Test]
        public void Constructor_WithInitialSize_PreWarmsPool()
        {
            var pool = new ObjectPool<BoxCollider>(_prefab, null, 3);

            Assert.AreEqual(3, pool.CountInactive);
            Assert.AreEqual(0, pool.CountActive);
        }

        [Test]
        public void Get_ReturnsActiveInstance()
        {
            var pool = new ObjectPool<BoxCollider>(_prefab, null, 1);

            var instance = pool.Get();

            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.gameObject.activeSelf);
            Assert.AreEqual(1, pool.CountActive);
            Assert.AreEqual(0, pool.CountInactive);

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void Get_WhenExhausted_AutoExpands()
        {
            var pool = new ObjectPool<BoxCollider>(_prefab, null, 0);

            var instance = pool.Get();

            Assert.IsNotNull(instance);
            Assert.AreEqual(1, pool.CountActive);

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void Release_DeactivatesAndReturnsToPool()
        {
            var pool = new ObjectPool<BoxCollider>(_prefab, null, 1);
            var instance = pool.Get();

            pool.Release(instance);

            Assert.IsFalse(instance.gameObject.activeSelf);
            Assert.AreEqual(0, pool.CountActive);
            Assert.AreEqual(1, pool.CountInactive);

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void GetThenReleaseThenGet_ReusesInstance()
        {
            var pool = new ObjectPool<BoxCollider>(_prefab, null, 1);
            var first = pool.Get();

            pool.Release(first);
            var second = pool.Get();

            Assert.AreEqual(first, second);

            Object.DestroyImmediate(first.gameObject);
        }

        [Test]
        public void PreWarm_AddsInactiveInstances()
        {
            var pool = new ObjectPool<BoxCollider>(_prefab);

            pool.PreWarm(5);

            Assert.AreEqual(5, pool.CountInactive);
            Assert.AreEqual(0, pool.CountActive);
        }
    }
}
