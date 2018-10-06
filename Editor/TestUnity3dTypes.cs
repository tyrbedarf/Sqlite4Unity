#if UNITY_STANDALONE

using System;
using System.Collections.Generic;
using System.Linq;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SetUp = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#else
using NUnit.Framework;
#endif

using UnityEngine;

namespace SQLite.Tests
{
    [TestFixture]
    public class TestUnity3dMethods
    {
        private TestDb _db;

        public class ItemWithVectors
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public Vector2 Vec2 { get; set; }
            public Vector3 Vec3 { get; set; }
            public Color Color  { get; set; }
        }

        public class TestDb : SQLiteConnection
        {
            public TestDb(String path)
                : base(path)
            {
				CreateTable<ItemWithVectors>();
            }
        }

        [SetUp]
        public void Setup()
        {
            _db = new TestDb(TestPath.GetTempFileName());
			_db.SetForeignKeysPermissions(true);
        }
        [TearDown]
        public void TearDown()
        {
            if (_db != null) _db.Close();
        }

        [Test]
        public void TestVector2()
        {
            for(int x = 0; x < 5; x++)
            {
                for(int y = 0; y < 5; y++)
                {
                    var vec2 = new Vector2(x, y);

                    var test = new ItemWithVectors();
                    test.Vec2 = vec2;

                    _db.Insert(test);

                    var max = _db.Table<ItemWithVectors>().Max(m => m.Id);
                    var result = _db.Table<ItemWithVectors>().Where(item => item.Id == max).FirstOrDefault();
                    Assert.NotNull(result);
                    Assert.AreEqual(result.Vec2.x, x);
                    Assert.AreEqual(result.Vec2.y, y);
                }
            }
        }

        [Test]
        public void TestVector3()
        {
            for(int x = 0; x < 5; x++)
            {
                for(int y = 0; y < 5; y++)
                {
                    for(int z = 0; z < 5; z++)
                    {
                        var vec3 = new Vector3(x, y, z);

                        var test = new ItemWithVectors();
                        test.Vec3 = vec3;

                        _db.Insert(test);

                        var max = _db.Table<ItemWithVectors>().Max(m => m.Id);
                        var result = _db.Table<ItemWithVectors>().Where(item => item.Id == max).FirstOrDefault();
                        Assert.NotNull(result);
                        Assert.AreEqual(result.Vec3.x, x);
                        Assert.AreEqual(result.Vec3.y, y);
                        Assert.AreEqual(result.Vec3.z, z);
                    }
                }
            }
        }

        [Test]
        public void TestColor()
        {
            for(int x = 0; x < 5; x++)
            {
                for(int y = 0; y < 5; y++)
                {
                    for(int z = 0; z < 5; z++)
                    {
                        var vec3 = new Color(x, y, z, x + y);

                        var test = new ItemWithVectors();
                        test.Color = vec3;

                        _db.Insert(test);

                        var max = _db.Table<ItemWithVectors>().Max(m => m.Id);
                        var result = _db.Table<ItemWithVectors>().Where(item => item.Id == max).FirstOrDefault();
                        Assert.NotNull(result);
                        Assert.AreEqual(result.Color.r, x);
                        Assert.AreEqual(result.Color.g, y);
                        Assert.AreEqual(result.Color.b, z);
                        Assert.AreEqual(result.Color.a, x + y);
                    }
                }
            }
        }
    }
}
#endif
