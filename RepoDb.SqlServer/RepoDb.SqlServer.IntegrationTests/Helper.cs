﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepoDb.Extensions;
using RepoDb.SqlServer.IntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace RepoDb.SqlServer.IntegrationTests
{
    public static class Helper
    {
        static Helper()
        {
            EpocDate = new DateTime(1970, 1, 1, 0, 0, 0);
        }

        /// <summary>
        /// Gets the value of the Epoc date.
        /// </summary>
        public static DateTime EpocDate { get; }

        /// <summary>
        /// Gets the current <see cref="Random"/> object in used.
        /// </summary>
        public static Random Randomizer => new Random(1);

        /// <summary>
        /// Asserts the properties equality of 2 types.
        /// </summary>
        /// <typeparam name="T1">The type of first object.</typeparam>
        /// <typeparam name="T2">The type of second object.</typeparam>
        /// <param name="t1">The instance of first object.</param>
        /// <param name="t2">The instance of second object.</param>
        public static void AssertPropertiesEquality<T1, T2>(T1 t1, T2 t2)
        {
            var propertiesOfType1 = typeof(T1).GetProperties();
            var propertiesOfType2 = typeof(T2).GetProperties();
            propertiesOfType1.AsList().ForEach(propertyOfType1 =>
            {
                if (propertyOfType1.Name == "Id")
                {
                    return;
                }
                var propertyOfType2 = propertiesOfType2.FirstOrDefault(p => p.Name == propertyOfType1.Name);
                if (propertyOfType2 == null)
                {
                    return;
                }
                var value1 = propertyOfType1.GetValue(t1);
                var value2 = propertyOfType2.GetValue(t2);
                if (value1 is Array && value2 is Array)
                {
                    var array1 = (Array)value1;
                    var array2 = (Array)value2;
                    for (var i = 0; i < Math.Min(array1.Length, array2.Length); i++)
                    {
                        var v1 = array1.GetValue(i);
                        var v2 = array2.GetValue(i);
                        Assert.AreEqual(v1, v2,
                            $"Assert failed for '{propertyOfType1.Name}'. The values are '{value1} ({propertyOfType1.PropertyType.FullName})' and '{value2} ({propertyOfType2.PropertyType.FullName})'.");
                    }
                }
                else
                {
                    Assert.AreEqual(value1, value2,
                        $"Assert failed for '{propertyOfType1.Name}'. The values are '{value1} ({propertyOfType1.PropertyType.FullName})' and '{value2} ({propertyOfType2.PropertyType.FullName})'.");
                }
            });
        }

        /// <summary>
        /// Asserts the members equality of 2 object and <see cref="ExpandoObject"/>.
        /// </summary>
        /// <typeparam name="T">The type of first object.</typeparam>
        /// <param name="obj">The instance of first object.</param>
        /// <param name="expandoObj">The instance of second object.</param>
        public static void AssertMembersEquality(object obj, object expandoObj)
        {
            var dictionary = new ExpandoObject() as IDictionary<string, object>;
            foreach (var property in expandoObj.GetType().GetProperties())
            {
                dictionary.Add(property.Name, property.GetValue(expandoObj));
            }
            AssertMembersEquality(obj, dictionary);
        }

        /// <summary>
        /// Asserts the members equality of 2 object and <see cref="ExpandoObject"/>.
        /// </summary>
        /// <typeparam name="T">The type of first object.</typeparam>
        /// <param name="obj">The instance of first object.</param>
        /// <param name="expandoObj">The instance of second object.</param>
        public static void AssertMembersEquality(object obj, ExpandoObject expandoObj)
        {
            var dictionary = expandoObj as IDictionary<string, object>;
            AssertMembersEquality(obj, dictionary);
        }

        /// <summary>
        /// Asserts the members equality of 2 objects.
        /// </summary>
        /// <typeparam name="T">The type of first object.</typeparam>
        /// <param name="obj">The instance of first object.</param>
        /// <param name="dictionary">The instance of second object.</param>
        public static void AssertMembersEquality(object obj, IDictionary<string, object> dictionary)
        {
            var properties = obj.GetType().GetProperties();
            properties.AsList().ForEach(property =>
            {
                if (property.Name == "Id")
                {
                    return;
                }
                if (dictionary.ContainsKey(property.Name))
                {
                    var value1 = property.GetValue(obj);
                    var value2 = dictionary[property.Name];
                    if (value1 is Array && value2 is Array)
                    {
                        var array1 = (Array)value1;
                        var array2 = (Array)value2;
                        for (var i = 0; i < Math.Min(array1.Length, array2.Length); i++)
                        {
                            var v1 = array1.GetValue(i);
                            var v2 = array2.GetValue(i);
                            Assert.AreEqual(v1, v2,
                                $"Assert failed for '{property.Name}'. The values are '{v1}' and '{v2}'.");
                        }
                    }
                    else
                    {
                        var propertyType = property.PropertyType.GetUnderlyingType();
                        if (propertyType == typeof(TimeSpan) && value2 is DateTime)
                        {
                            value2 = ((DateTime)value2).TimeOfDay;
                        }
                        Assert.AreEqual(Convert.ChangeType(value1, propertyType), Convert.ChangeType(value2, propertyType),
                            $"Assert failed for '{property.Name}'. The values are '{value1}' and '{value2}'.");
                    }
                }
            });
        }

        #region CompleteTable

        /// <summary>
        /// Creates a list of <see cref="CompleteTable"/> objects.
        /// </summary>
        /// <param name="count">The number of rows.</param>
        /// <returns>A list of <see cref="CompleteTable"/> objects.</returns>
        public static List<CompleteTable> CreateCompleteTables(int count)
        {
            var tables = new List<CompleteTable>();
            var now = DateTime.SpecifyKind(
                DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")),
                    DateTimeKind.Unspecified);
            for (var i = 0; i < count; i++)
            {
                tables.Add(new CompleteTable
                {
                    Id = (i + 1),
                    ColumnBigInt = Convert.ToInt64(i),
                    ColumnBinary = (byte[])null,
                    ColumnBit = true,
                    ColumnChar = "C",
                    ColumnDate = now.Date,
                    ColumnDateTime = now.Date,
                    ColumnDateTime2 = now,
                    ColumnDateTimeOffset = new DateTimeOffset(now.Date).ToOffset(TimeSpan.FromHours(2)),
                    ColumnDecimal = Convert.ToDecimal(i),
                    ColumnFloat = Convert.ToDouble(i),
                    //ColumnGeography = (object)null,
                    //ColumnGeometry = (object)null,
                    //ColumnHierarchyId = (object)null,
                    ColumnImage = (byte[])null,
                    ColumnInt = Convert.ToInt32(i),
                    ColumnMoney = Convert.ToDecimal(i),
                    ColumnNChar = "C",
                    ColumnNText = "NText",
                    ColumnNumeric = Convert.ToDecimal(i),
                    ColumnNVarChar = "NVarChar",
                    ColumnReal = Convert.ToSingle(i),
                    ColumnSmallDateTime = now.Date,
                    ColumnSmallInt = Convert.ToInt16(i),
                    ColumnSmallMoney = Convert.ToDecimal(i),
                    ColumnSqlVariant = (object)null,
                    ColumnText = "Text",
                    ColumnTime = now.TimeOfDay,
                    //ColumnTimestamp = (byte[])null,
                    ColumnTinyInt = (byte)0,
                    ColumnUniqueIdentifier = Guid.NewGuid(),
                    ColumnVarBinary = (byte[])null,
                    ColumnVarChar = "VarChar",
                    ColumnXml = (string)null,
                    SessionId = Guid.NewGuid()
                });
            }
            return tables;
        }

        /// <summary>
        /// Update the properties of <see cref="CompleteTable"/> instance.
        /// </summary>
        /// <param name="table">The instance to be updated.</param>
        public static void UpdateCompleteTableProperties(CompleteTable table)
        {
            var now = DateTime.SpecifyKind(
                DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")),
                    DateTimeKind.Unspecified);
            table.ColumnBigInt = Convert.ToInt64(2);
            table.ColumnBinary = (byte[])null;
            table.ColumnBit = true;
            table.ColumnChar = "C";
            table.ColumnDate = now.Date;
            table.ColumnDateTime = now.Date;
            table.ColumnDateTime2 = now;
            table.ColumnDateTimeOffset = new DateTimeOffset(now.Date).ToOffset(TimeSpan.FromHours(2));
            table.ColumnDecimal = Convert.ToDecimal(2);
            table.ColumnFloat = Convert.ToDouble(2);
            //table.ColumnGeography = (object)null;
            //table.ColumnGeometry = (object)null;
            //table.ColumnHierarchyId = (object)null;
            table.ColumnImage = (byte[])null;
            table.ColumnInt = Convert.ToInt32(2);
            table.ColumnMoney = Convert.ToDecimal(2);
            table.ColumnNChar = "C";
            table.ColumnNText = "NText - Updated";
            table.ColumnNumeric = Convert.ToDecimal(2);
            table.ColumnNVarChar = "NVarChar - Updated";
            table.ColumnReal = Convert.ToSingle(2);
            table.ColumnSmallDateTime = now.Date;
            table.ColumnSmallInt = Convert.ToInt16(2);
            table.ColumnSmallMoney = Convert.ToDecimal(2);
            table.ColumnSqlVariant = (object)null;
            table.ColumnText = "Text - Updated";
            table.ColumnTime = now.TimeOfDay;
            //table.ColumnTimestamp = (byte[])null;
            table.ColumnTinyInt = (byte)0;
            table.ColumnUniqueIdentifier = Guid.NewGuid();
            table.ColumnVarBinary = (byte[])null;
            table.ColumnVarChar = "VarChar - Updated";
            table.ColumnXml = (string)null;
            table.SessionId = Guid.NewGuid();
        }

        /// <summary>
        /// Creates a list of <see cref="CompleteTable"/> objects represented as dynamics.
        /// </summary>
        /// <param name="count">The number of rows.</param>
        /// <returns>A list of <see cref="CompleteTable"/> objects represented as dynamics.</returns>
        public static List<dynamic> CreateCompleteTablesAsDynamics(int count)
        {
            var tables = new List<dynamic>();
            var now = DateTime.SpecifyKind(
                DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")),
                    DateTimeKind.Unspecified);
            for (var i = 0; i < count; i++)
            {
                tables.Add(new
                {
                    Id = (i + 1),
                    ColumnBigInt = Convert.ToInt64(i),
                    ColumnBinary = (byte[])null,
                    ColumnBit = true,
                    ColumnChar = "C",
                    ColumnDate = now.Date,
                    ColumnDateTime = now.Date,
                    ColumnDateTime2 = now,
                    ColumnDateTimeOffset = new DateTimeOffset(now.Date).ToOffset(TimeSpan.FromHours(2)),
                    ColumnDecimal = Convert.ToDecimal(i),
                    ColumnFloat = Convert.ToDouble(i),
                    //ColumnGeography = (object)null,
                    //ColumnGeometry = (object)null,
                    //ColumnHierarchyId = (object)null,
                    ColumnImage = (byte[])null,
                    ColumnInt = Convert.ToInt32(i),
                    ColumnMoney = Convert.ToDecimal(i),
                    ColumnNChar = "C",
                    ColumnNText = "NText",
                    ColumnNumeric = Convert.ToDecimal(i),
                    ColumnNVarChar = "NVarChar",
                    ColumnReal = Convert.ToSingle(i),
                    ColumnSmallDateTime = now.Date,
                    ColumnSmallInt = Convert.ToInt16(i),
                    ColumnSmallMoney = Convert.ToDecimal(i),
                    ColumnSqlVariant = (object)null,
                    ColumnText = "Text",
                    ColumnTime = now.TimeOfDay,
                    //ColumnTimestamp = (byte[])null,
                    ColumnTinyInt = (byte)0,
                    ColumnUniqueIdentifier = Guid.NewGuid(),
                    ColumnVarBinary = (byte[])null,
                    ColumnVarChar = "VarChar",
                    ColumnXml = (string)null,
                    SessionId = Guid.NewGuid()
                });
            }
            return tables;
        }

        /// <summary>
        /// Update the properties of <see cref="CompleteTable"/> instance represented asy dynamic.
        /// </summary>
        /// <param name="table">The instance to be updated.</param>
        public static void UpdateCompleteTableAsDynamicProperties(dynamic table)
        {
            var now = DateTime.SpecifyKind(
                DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")),
                    DateTimeKind.Unspecified);
            table.ColumnBigInt = Convert.ToInt64(2);
            table.ColumnBinary = (byte[])null;
            table.ColumnBit = true;
            table.ColumnChar = "C";
            table.ColumnDate = now.Date;
            table.ColumnDateTime = now.Date;
            table.ColumnDateTime2 = now;
            table.ColumnDateTimeOffset = new DateTimeOffset(now.Date).ToOffset(TimeSpan.FromHours(2));
            table.ColumnDecimal = Convert.ToDecimal(2);
            table.ColumnFloat = Convert.ToDouble(2);
            table.ColumnGeography = (object)null;
            table.ColumnGeometry = (object)null;
            table.ColumnHierarchyId = (object)null;
            table.ColumnImage = (byte[])null;
            table.ColumnInt = Convert.ToInt32(2);
            table.ColumnMoney = Convert.ToDecimal(2);
            table.ColumnNChar = "C";
            table.ColumnNText = "NText - Updated";
            table.ColumnNumeric = Convert.ToDecimal(2);
            table.ColumnNVarChar = "NVarChar - Updated";
            table.ColumnReal = Convert.ToSingle(2);
            table.ColumnSmallDateTime = now.Date;
            table.ColumnSmallInt = Convert.ToInt16(2);
            table.ColumnSmallMoney = Convert.ToDecimal(2);
            table.ColumnSqlVariant = (object)null;
            table.ColumnText = "Text - Updated";
            table.ColumnTime = now.TimeOfDay;
            //table.ColumnTimestamp = (byte[])null;
            table.ColumnTinyInt = (byte)0;
            table.ColumnUniqueIdentifier = Guid.NewGuid();
            table.ColumnVarBinary = (byte[])null;
            table.ColumnVarChar = "VarChar - Updated";
            table.ColumnXml = (string)null;
            table.SessionId = Guid.NewGuid();
        }

        #endregion

        #region NonIdentityCompleteTable

        /// <summary>
        /// Creates a list of <see cref="NonIdentityCompleteTable"/> objects.
        /// </summary>
        /// <param name="count">The number of rows.</param>
        /// <returns>A list of <see cref="NonIdentityCompleteTable"/> objects.</returns>
        public static List<NonIdentityCompleteTable> CreateNonIdentityCompleteTables(int count)
        {
            var tables = new List<NonIdentityCompleteTable>();
            var now = DateTime.SpecifyKind(
                DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")),
                    DateTimeKind.Unspecified);
            for (var i = 0; i < count; i++)
            {
                tables.Add(new NonIdentityCompleteTable
                {
                    Id = (i + 1),
                    ColumnBigInt = Convert.ToInt64(i),
                    ColumnBinary = (byte[])null,
                    ColumnBit = true,
                    ColumnChar = "C",
                    ColumnDate = now.Date,
                    ColumnDateTime = now.Date,
                    ColumnDateTime2 = now,
                    ColumnDateTimeOffset = new DateTimeOffset(now.Date).ToOffset(TimeSpan.FromHours(2)),
                    ColumnDecimal = Convert.ToDecimal(i),
                    ColumnFloat = Convert.ToDouble(i),
                    //ColumnGeography = (object)null,
                    //ColumnGeometry = (object)null,
                    //ColumnHierarchyId = (object)null,
                    ColumnImage = (byte[])null,
                    ColumnInt = Convert.ToInt32(i),
                    ColumnMoney = Convert.ToDecimal(i),
                    ColumnNChar = "C",
                    ColumnNText = "NText",
                    ColumnNumeric = Convert.ToDecimal(i),
                    ColumnNVarChar = "NVarChar",
                    ColumnReal = Convert.ToSingle(i),
                    ColumnSmallDateTime = now.Date,
                    ColumnSmallInt = Convert.ToInt16(i),
                    ColumnSmallMoney = Convert.ToDecimal(i),
                    ColumnSqlVariant = (object)null,
                    ColumnText = "Text",
                    ColumnTime = now.TimeOfDay,
                    //ColumnTimestamp = (byte[])null,
                    ColumnTinyInt = (byte)0,
                    ColumnUniqueIdentifier = Guid.NewGuid(),
                    ColumnVarBinary = (byte[])null,
                    ColumnVarChar = "VarChar",
                    ColumnXml = (string)null,
                    SessionId = Guid.NewGuid()
                });
            }
            return tables;
        }

        /// <summary>
        /// Update the properties of <see cref="NonIdentityCompleteTable"/> instance.
        /// </summary>
        /// <param name="table">The instance to be updated.</param>
        public static void UpdateNonIdentityCompleteTableProperties(NonIdentityCompleteTable table)
        {
            var now = DateTime.SpecifyKind(
                DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")),
                    DateTimeKind.Unspecified);
            table.ColumnBigInt = Convert.ToInt64(2);
            table.ColumnBinary = (byte[])null;
            table.ColumnBit = true;
            table.ColumnChar = "C";
            table.ColumnDate = now.Date;
            table.ColumnDateTime = now.Date;
            table.ColumnDateTime2 = now;
            table.ColumnDateTimeOffset = new DateTimeOffset(now.Date).ToOffset(TimeSpan.FromHours(2));
            table.ColumnDecimal = Convert.ToDecimal(2);
            table.ColumnFloat = Convert.ToDouble(2);
            //table.ColumnGeography = (object)null;
            //table.ColumnGeometry = (object)null;
            //table.ColumnHierarchyId = (object)null;
            table.ColumnImage = (byte[])null;
            table.ColumnInt = Convert.ToInt32(2);
            table.ColumnMoney = Convert.ToDecimal(2);
            table.ColumnNChar = "C";
            table.ColumnNText = "NText - Updated";
            table.ColumnNumeric = Convert.ToDecimal(2);
            table.ColumnNVarChar = "NVarChar - Updated";
            table.ColumnReal = Convert.ToSingle(2);
            table.ColumnSmallDateTime = now.Date;
            table.ColumnSmallInt = Convert.ToInt16(2);
            table.ColumnSmallMoney = Convert.ToDecimal(2);
            table.ColumnSqlVariant = (object)null;
            table.ColumnText = "Text - Updated";
            table.ColumnTime = now.TimeOfDay;
            //table.ColumnTimestamp = (byte[])null;
            table.ColumnTinyInt = (byte)0;
            table.ColumnUniqueIdentifier = Guid.NewGuid();
            table.ColumnVarBinary = (byte[])null;
            table.ColumnVarChar = "VarChar - Updated";
            table.ColumnXml = (string)null;
            table.SessionId = Guid.NewGuid();
        }

        /// <summary>
        /// Creates a list of <see cref="NonIdentityCompleteTable"/> objects represented as dynamics.
        /// </summary>
        /// <param name="count">The number of rows.</param>
        /// <returns>A list of <see cref="NonIdentityCompleteTable"/> objects represented as dynamics.</returns>
        public static List<dynamic> CreateNonIdentityCompleteTablesAsDynamics(int count)
        {
            var tables = new List<dynamic>();
            var now = DateTime.SpecifyKind(
                DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")),
                    DateTimeKind.Unspecified);
            for (var i = 0; i < count; i++)
            {
                tables.Add(new
                {
                    Id = (i + 1),
                    ColumnBigInt = Convert.ToInt64(i),
                    ColumnBinary = (byte[])null,
                    ColumnBit = true,
                    ColumnChar = "C",
                    ColumnDate = now.Date,
                    ColumnDateTime = now.Date,
                    ColumnDateTime2 = now,
                    ColumnDateTimeOffset = new DateTimeOffset(now.Date).ToOffset(TimeSpan.FromHours(2)),
                    ColumnDecimal = Convert.ToDecimal(i),
                    ColumnFloat = Convert.ToDouble(i),
                    //ColumnGeography = (object)null,
                    //ColumnGeometry = (object)null,
                    //ColumnHierarchyId = (object)null,
                    ColumnImage = (byte[])null,
                    ColumnInt = Convert.ToInt32(i),
                    ColumnMoney = Convert.ToDecimal(i),
                    ColumnNChar = "C",
                    ColumnNText = "NText",
                    ColumnNumeric = Convert.ToDecimal(i),
                    ColumnNVarChar = "NVarChar",
                    ColumnReal = Convert.ToSingle(i),
                    ColumnSmallDateTime = now.Date,
                    ColumnSmallInt = Convert.ToInt16(i),
                    ColumnSmallMoney = Convert.ToDecimal(i),
                    ColumnSqlVariant = (object)null,
                    ColumnText = "Text",
                    ColumnTime = now.TimeOfDay,
                    //ColumnTimestamp = (byte[])null,
                    ColumnTinyInt = (byte)0,
                    ColumnUniqueIdentifier = Guid.NewGuid(),
                    ColumnVarBinary = (byte[])null,
                    ColumnVarChar = "VarChar",
                    ColumnXml = (string)null,
                    SessionId = Guid.NewGuid()
                });
            }
            return tables;
        }

        /// <summary>
        /// Update the properties of <see cref="NonIdentityCompleteTable"/> instance represented asy dynamic.
        /// </summary>
        /// <param name="table">The instance to be updated.</param>
        public static void UpdateNonIdentityCompleteTableAsDynamicProperties(dynamic table)
        {
            var now = DateTime.SpecifyKind(
                DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")),
                    DateTimeKind.Unspecified);
            table.ColumnBigInt = Convert.ToInt64(2);
            table.ColumnBinary = (byte[])null;
            table.ColumnBit = true;
            table.ColumnChar = "C";
            table.ColumnDate = now.Date;
            table.ColumnDateTime = now.Date;
            table.ColumnDateTime2 = now;
            table.ColumnDateTimeOffset = new DateTimeOffset(now.Date).ToOffset(TimeSpan.FromHours(2));
            table.ColumnDecimal = Convert.ToDecimal(2);
            table.ColumnFloat = Convert.ToDouble(2);
            table.ColumnGeography = (object)null;
            table.ColumnGeometry = (object)null;
            table.ColumnHierarchyId = (object)null;
            table.ColumnImage = (byte[])null;
            table.ColumnInt = Convert.ToInt32(2);
            table.ColumnMoney = Convert.ToDecimal(2);
            table.ColumnNChar = "C";
            table.ColumnNText = "NText - Updated";
            table.ColumnNumeric = Convert.ToDecimal(2);
            table.ColumnNVarChar = "NVarChar - Updated";
            table.ColumnReal = Convert.ToSingle(2);
            table.ColumnSmallDateTime = now.Date;
            table.ColumnSmallInt = Convert.ToInt16(2);
            table.ColumnSmallMoney = Convert.ToDecimal(2);
            table.ColumnSqlVariant = (object)null;
            table.ColumnText = "Text - Updated";
            table.ColumnTime = now.TimeOfDay;
            //table.ColumnTimestamp = (byte[])null;
            table.ColumnTinyInt = (byte)0;
            table.ColumnUniqueIdentifier = Guid.NewGuid();
            table.ColumnVarBinary = (byte[])null;
            table.ColumnVarChar = "VarChar - Updated";
            table.ColumnXml = (string)null;
            table.SessionId = Guid.NewGuid();
        }

        #endregion
    }
}
