// automatically generated by the FlatBuffers compiler, do not modify

namespace VideoIndexer
{

    using System;
    using FlatBuffers;

    public sealed class DatabaseMetaTableEntry : Table
    {
        public static DatabaseMetaTableEntry GetRootAsDatabaseMetaTableEntry(ByteBuffer _bb) { return GetRootAsDatabaseMetaTableEntry(_bb, new DatabaseMetaTableEntry()); }
        public static DatabaseMetaTableEntry GetRootAsDatabaseMetaTableEntry(ByteBuffer _bb, DatabaseMetaTableEntry obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
        public DatabaseMetaTableEntry __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

        public string FileName { get { int o = __offset(4); return o != 0 ? __string(o + bb_pos) : null; } }
        public ArraySegment<byte>? GetFileNameBytes() { return __vector_as_arraysegment(4); }
        public ulong FileSize { get { int o = __offset(6); return o != 0 ? bb.GetUlong(o + bb_pos) : (ulong)0; } }

        public static Offset<DatabaseMetaTableEntry> CreateDatabaseMetaTableEntry(FlatBufferBuilder builder,
            StringOffset fileNameOffset = default(StringOffset),
            ulong fileSize = 0)
        {
            builder.StartObject(2);
            DatabaseMetaTableEntry.AddFileSize(builder, fileSize);
            DatabaseMetaTableEntry.AddFileName(builder, fileNameOffset);
            return DatabaseMetaTableEntry.EndDatabaseMetaTableEntry(builder);
        }

        public static void StartDatabaseMetaTableEntry(FlatBufferBuilder builder) { builder.StartObject(2); }
        public static void AddFileName(FlatBufferBuilder builder, StringOffset fileNameOffset) { builder.AddOffset(0, fileNameOffset.Value, 0); }
        public static void AddFileSize(FlatBufferBuilder builder, ulong fileSize) { builder.AddUlong(1, fileSize, 0); }
        public static Offset<DatabaseMetaTableEntry> EndDatabaseMetaTableEntry(FlatBufferBuilder builder)
        {
            int o = builder.EndObject();
            return new Offset<DatabaseMetaTableEntry>(o);
        }
    };


}
