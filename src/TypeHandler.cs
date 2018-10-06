using System;
using System.Collections;
using System.Collections.Generic;

#if USE_CSHARP_SQLITE
using Sqlite3 = Community.CsharpSqlite.Sqlite3;
using Sqlite3DatabaseHandle = Community.CsharpSqlite.Sqlite3.sqlite3;
using Sqlite3Statement = Community.CsharpSqlite.Sqlite3.Vdbe;
#elif USE_WP8_NATIVE_SQLITE
using Sqlite3 = Sqlite.Sqlite3;
using Sqlite3DatabaseHandle = Sqlite.Database;
using Sqlite3Statement = Sqlite.Statement;
#else
using Sqlite3DatabaseHandle = System.IntPtr;
using Sqlite3Statement = System.IntPtr;
#endif

#if UNITY_STANDALONE
using UnityEngine;
#endif

namespace SQLite
{
    public static class Types
    {
        public delegate object ReadColumnDelegate(Sqlite3Statement stmt, int index);
        public delegate string GetSqlTypeDelegate(TableMapping.Column c);
        public delegate void BindParameterDelegate(Sqlite3Statement stmt, int index, object value);

        private static Dictionary<Type, ReadColumnDelegate> Readers;
        private static Dictionary<Type, GetSqlTypeDelegate> SqlTypes;
        private static Dictionary<Type, BindParameterDelegate> Binders;

        public static bool TryRead(Type t, Sqlite3Statement stmt, int index, out object result)
        {
            if(Readers == null)
            {
                Initialize();
                RegisterDefaultTypes();
            }

            if(Readers.ContainsKey(t))
            {
                result = Readers[t](stmt, index);
                return true;
            }

            result = null;
            return false;
        }

        public static bool TryGetSql(Type t, TableMapping.Column c, out string result)
        {
            if(SqlTypes == null)
            {
                Initialize();
                RegisterDefaultTypes();
            }

            if(SqlTypes.ContainsKey(t))
            {
                result = SqlTypes[t](c);
                return true;
            }

            result = null;
            return false;
        }

        public static bool TryBind(Type t, Sqlite3Statement stmt, int index, object value)
        {
            if(Binders == null)
            {
                Initialize();
                RegisterDefaultTypes();
            }

            if(Binders.ContainsKey(t))
            {
                Binders[t](stmt, index, value);
                return true;
            }

            return false;
        }

        public static void Initialize()
        {
            Readers = new Dictionary<Type, ReadColumnDelegate>();
            Binders = new Dictionary<Type, BindParameterDelegate>();
            SqlTypes = new Dictionary<Type, GetSqlTypeDelegate>();
        }

        public static void Register(
            Type t, 
            ReadColumnDelegate r, 
            GetSqlTypeDelegate s, 
            BindParameterDelegate b)
        {
            if(Readers == null || Binders == null || SqlTypes == null)
            {
                Initialize();
                RegisterDefaultTypes();
            }

            Readers.Add(t, r);
            Binders.Add(t, b);
            SqlTypes.Add(t, s);
        }

        private static void RegisterDefaultTypes()
        {
#if UNITY_STANDALONE
            Register(
                typeof(Vector2),
                (stmt, index) =>
                {
                    var bytes = SQLite3.ColumnByteArray(stmt, index);
                    var x = BitConverter.ToSingle(bytes, 0);
                    var y = BitConverter.ToSingle(bytes, sizeof(float));

                    return new Vector2(x, y);
                },
                (c) =>
                {
                    return "blob";
                },
                (stmt, index, value) =>
                {
                    var vec = (Vector2) value;
                    var bytes = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(vec.x), 0, bytes, 0, sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(vec.y), 0, bytes, sizeof(float), sizeof(float));

                    SQLite3.BindBlob(stmt, index, bytes, bytes.Length, new System.IntPtr(-1));
                });

            Register(
                typeof(Vector3),
                (stmt, index) =>
                {
                    var bytes = SQLite3.ColumnByteArray(stmt, index);
                    var x = BitConverter.ToSingle(bytes, 0);
                    var y = BitConverter.ToSingle(bytes, sizeof(float));
                    var z = BitConverter.ToSingle(bytes, sizeof(float) * 2);

                    return new Vector3(x, y, z);
                },
                (c) =>
                {
                    return "blob";
                },
                (stmt, index, value) =>
                {
                    var vec = (Vector3) value;
                    var bytes = new byte[sizeof(float) * 3];
                    Buffer.BlockCopy(BitConverter.GetBytes(vec.x), 0, bytes, 0, sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(vec.y), 0, bytes, sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(vec.z), 0, bytes, sizeof(float) * 2, sizeof(float));

                    SQLite3.BindBlob(stmt, index, bytes, bytes.Length, new System.IntPtr(-1));
                });

            Register(
                typeof(Color),
                (stmt, index) =>
                {
                    var bytes = SQLite3.ColumnByteArray(stmt, index);
                    var r = BitConverter.ToSingle(bytes, 0);
                    var g = BitConverter.ToSingle(bytes, sizeof(float));
                    var b = BitConverter.ToSingle(bytes, sizeof(float) * 2);
                    var a = BitConverter.ToSingle(bytes, sizeof(float) * 3);

                    return new Color(r, g, b, a);
                },
                (c) =>
                {
                    return "blob";
                },
                (stmt, index, value) =>
                {
                    var vec = (Color) value;
                    var bytes = new byte[sizeof(float) * 4];
                    Buffer.BlockCopy(BitConverter.GetBytes(vec.r), 0, bytes, 0, sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(vec.g), 0, bytes, sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(vec.b), 0, bytes, sizeof(float) * 2, sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(vec.a), 0, bytes, sizeof(float) * 3, sizeof(float));

                    SQLite3.BindBlob(stmt, index, bytes, bytes.Length, new System.IntPtr(-1));
                });
#endif
        }
    }
}
