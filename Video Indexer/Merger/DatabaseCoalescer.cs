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

using System.Collections.Generic;
using System.Linq;
using VideoIndexer.Wrappers;
using System;
using VideoIndexer.Serialization;
using System.IO;

namespace VideoIndexer.Merger
{
    public static class DatabaseCoalescer
    {
        #region private fields
        private static readonly ulong FileSizeLimit = 838860800;
        #endregion
        #region public methods
        public static DatabaseMetaTableWrapper Coalesce(
            string pathToMetatable
        )
        {
            DatabaseMetaTableWrapper oldMetatable = DatabaseMetaTableLoader.Load(pathToMetatable);
            IEnumerable<DatabaseMetaTableEntryWrapper> databasesSelectedForCoalescing = GetDatabasesThatNeedCoalescing(oldMetatable);
            IEnumerable<DatabaseMetaTableEntryWrapper> remainingDatabases = oldMetatable.DatabaseMetaTableEntries.Except(databasesSelectedForCoalescing);
            IEnumerable<IEnumerable<DatabaseMetaTableEntryWrapper>> groupedEntries = DetermineGroups(databasesSelectedForCoalescing);
            IEnumerable<DatabaseMetaTableEntryWrapper> coalescedDatabaseGroups = CoalesceDatabaseGroups(groupedEntries);

            DatabaseMetaTableWrapper newMetaTable = new DatabaseMetaTableWrapper
            {
                DatabaseMetaTableEntries = coalescedDatabaseGroups.Concat(remainingDatabases).ToArray(),
            };

            DatabaseMetaTableSaver.Save(newMetaTable, pathToMetatable);

            // Delete old databases
            DeleteOldDatabases(databasesSelectedForCoalescing);

            return newMetaTable;
        }
        #endregion
        #region private methods
        private static void DeleteOldDatabases(IEnumerable<DatabaseMetaTableEntryWrapper> entries)
        {
            foreach (DatabaseMetaTableEntryWrapper entry in entries)
            {
                try
                {
                    File.Delete(entry.FileName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not delete {0}. Reason: {1}", e.Message);
                }
            }
        }

        private static IEnumerable<DatabaseMetaTableEntryWrapper> CoalesceDatabaseGroups(IEnumerable<IEnumerable<DatabaseMetaTableEntryWrapper>> groupedEntries)
        {
            return from @group in groupedEntries
                   select CoalesceDatabases(@group);
        }

        private static DatabaseMetaTableEntryWrapper CoalesceDatabases(IEnumerable<DatabaseMetaTableEntryWrapper> coalescedEntries)
        {
            // Load each database
            IEnumerable<VideoFingerPrintDatabaseWrapper> databases = from entry in coalescedEntries
                                                                     select DatabaseLoader.Load(entry.FileName);

            // Merge fingerprints
            IEnumerable<VideoFingerPrintWrapper> allVideoFingerPrints = from database in databases
                                                                        from videoFingerPrint in database.VideoFingerPrints
                                                                        select videoFingerPrint;

            // Create a new database
            var freshDatabase = new VideoFingerPrintDatabaseWrapper
            {
                VideoFingerPrints = allVideoFingerPrints.ToArray(),
            };

            // Save the database
            string databaseFileName = Path.GetRandomFileName() + ".bin";
            DatabaseSaver.Save(freshDatabase, databaseFileName);
            FileInfo databaseFileInfo = new FileInfo(databaseFileName);
            return new DatabaseMetaTableEntryWrapper
            {
                FileName = databaseFileName,
                FileSize = (ulong)databaseFileInfo.Length,
            };
        }

        private static IEnumerable<IEnumerable<DatabaseMetaTableEntryWrapper>> DetermineGroups(IEnumerable<DatabaseMetaTableEntryWrapper> entries)
        {
            var unusedEntries = new HashSet<DatabaseMetaTableEntryWrapper>(entries);
            var groupedEntries = new HashSet<IEnumerable<DatabaseMetaTableEntryWrapper>>();
            while (unusedEntries.Any())
            {
                ulong currentGroupSize = 0;
                var currentGroup = new HashSet<DatabaseMetaTableEntryWrapper>();
                while (currentGroupSize < FileSizeLimit)
                {
                    DatabaseMetaTableEntryWrapper currentEntry = unusedEntries.FirstOrDefault();
                    if (currentEntry == null)
                    {
                        // We didn't hit the limit before exhausting all available entries
                        break;
                    }
                    unusedEntries.Remove(currentEntry);
                    currentGroup.Add(currentEntry);
                    currentGroupSize += currentEntry.FileSize;
                }
                groupedEntries.Add(currentGroup);
            }

            return groupedEntries;
        }

        private static IEnumerable<DatabaseMetaTableEntryWrapper> GetDatabasesThatNeedCoalescing(DatabaseMetaTableWrapper metatable)
        {
            return from entry in metatable.DatabaseMetaTableEntries
                   where entry.FileSize < (ulong)FileSizeLimit
                   select entry;
        }
        #endregion
    }
}