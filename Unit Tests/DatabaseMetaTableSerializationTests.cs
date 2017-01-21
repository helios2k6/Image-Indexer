/* 
 * Copyright (c) 2015 Andrew Johnson
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
 * Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using VideoIndexer.Serialization;
using VideoIndexer.Wrappers;

namespace UnitTests
{
    [TestClass]
    public class DatabaseMetaTableSerializationTests
    {
        [TestMethod]
        public void TestDatabaseMetaTableSerialization()
        {
            var entry = new DatabaseMetaTableEntryWrapper
            {
                FileName = "test",
                FileSize = 1,
            };

            var entry2 = new DatabaseMetaTableEntryWrapper
            {
                FileName = "test 2",
                FileSize = 2,
            };

            var database = new DatabaseMetaTableWrapper
            {
                DatabaseMetaTableEntries = new[] { entry, entry2 },
            };

            using (var memoryStream = new MemoryStream())
            {
                DatabaseMetaTableSaver.Save(database, memoryStream);
                byte[] savedDatabase = memoryStream.ToArray();
                DatabaseMetaTableWrapper reloadedDatabase = DatabaseMetaTableLoader.Load(savedDatabase);

                Assert.AreEqual(database, reloadedDatabase);
            }
        }
    }
}
