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

using Core.Model;
using Core.Model.Serialization;
using Core.Model.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VideoIndexer.Merger
{
    public static class DatabaseCoalescer
    {
        #region private fields
        private static readonly ulong FileSizeLimit = 838860800;
        #endregion
        #region public methods
        public static VideoFingerPrintDatabaseMetaTableWrapper Coalesce(
            string pathToMetatable
        )
        {
            VideoFingerPrintDatabaseMetaTableWrapper oldMetatable = VideoFingerPrintDatabaseMetaTableLoader.Load(pathToMetatable);
            IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper> databasesSelectedForCoalescing = GetDatabasesThatNeedCoalescing(oldMetatable);
            IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper> remainingDatabases = oldMetatable.DatabaseMetaTableEntries.Except(databasesSelectedForCoalescing);
            IEnumerable<IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper>> groupedEntries = DetermineGroups(databasesSelectedForCoalescing);
            IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper> coalescedDatabaseGroups = CoalesceDatabaseGroups(groupedEntries);

            VideoFingerPrintDatabaseMetaTableWrapper newMetaTable = new VideoFingerPrintDatabaseMetaTableWrapper
            {
                DatabaseMetaTableEntries = coalescedDatabaseGroups.Concat(remainingDatabases).ToArray(),
            };

            VideoFingerPrintDatabaseMetaTableSaver.Save(newMetaTable, pathToMetatable);

            // Delete old databases
            DeleteOldDatabases(databasesSelectedForCoalescing);

            return newMetaTable;
        }
        #endregion
        #region private methods
        private static void DeleteOldDatabases(IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper> entries)
        {
            foreach (VideoFingerPrintDatabaseMetaTableEntryWrapper entry in entries)
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

        private static IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper> CoalesceDatabaseGroups(IEnumerable<IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper>> groupedEntries)
        {
            return from @group in groupedEntries
                   select CoalesceDatabases(@group);
        }

        private static VideoFingerPrintDatabaseMetaTableEntryWrapper CoalesceDatabases(IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper> coalescedEntries)
        {
            // Load each database
            IEnumerable<VideoFingerPrintDatabaseWrapper> databases = from entry in coalescedEntries
                                                                     select VideoFingerPrintDatabaseLoader.Load(entry.FileName);

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
            VideoFingerPrintDatabaseSaver.Save(freshDatabase, databaseFileName);
            FileInfo databaseFileInfo = new FileInfo(databaseFileName);
            return new VideoFingerPrintDatabaseMetaTableEntryWrapper
            {
                FileName = databaseFileName,
                FileSize = (ulong)databaseFileInfo.Length,
            };
        }

        private static IEnumerable<IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper>> DetermineGroups(IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper> entries)
        {
            var unusedEntries = new HashSet<VideoFingerPrintDatabaseMetaTableEntryWrapper>(entries);
            var groupedEntries = new HashSet<IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper>>();
            while (unusedEntries.Any())
            {
                ulong currentGroupSize = 0;
                var currentGroup = new HashSet<VideoFingerPrintDatabaseMetaTableEntryWrapper>();
                while (currentGroupSize < FileSizeLimit)
                {
                    VideoFingerPrintDatabaseMetaTableEntryWrapper currentEntry = unusedEntries.FirstOrDefault();
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

        private static IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper> GetDatabasesThatNeedCoalescing(VideoFingerPrintDatabaseMetaTableWrapper metatable)
        {
            return from entry in metatable.DatabaseMetaTableEntries
                   where entry.FileSize < (ulong)FileSizeLimit
                   select entry;
        }
        #endregion
    }
}