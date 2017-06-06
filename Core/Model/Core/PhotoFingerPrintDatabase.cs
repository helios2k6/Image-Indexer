// automatically generated by the FlatBuffers compiler, do not modify

namespace Core
{

    using System;
    using FlatBuffers;

    internal struct PhotoFingerPrintDatabase : IFlatbufferObject
    {
        private Table __p;
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public static PhotoFingerPrintDatabase GetRootAsPhotoFingerPrintDatabase(ByteBuffer _bb) { return GetRootAsPhotoFingerPrintDatabase(_bb, new PhotoFingerPrintDatabase()); }
        public static PhotoFingerPrintDatabase GetRootAsPhotoFingerPrintDatabase(ByteBuffer _bb, PhotoFingerPrintDatabase obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
        public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
        public PhotoFingerPrintDatabase __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

        public PhotoFingerPrint? FingerPrints(int j) { int o = __p.__offset(4); return o != 0 ? (PhotoFingerPrint?)(new PhotoFingerPrint()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
        public int FingerPrintsLength { get { int o = __p.__offset(4); return o != 0 ? __p.__vector_len(o) : 0; } }

        public static Offset<PhotoFingerPrintDatabase> CreatePhotoFingerPrintDatabase(FlatBufferBuilder builder,
            VectorOffset FingerPrintsOffset = default(VectorOffset))
        {
            builder.StartObject(1);
            PhotoFingerPrintDatabase.AddFingerPrints(builder, FingerPrintsOffset);
            return PhotoFingerPrintDatabase.EndPhotoFingerPrintDatabase(builder);
        }

        public static void StartPhotoFingerPrintDatabase(FlatBufferBuilder builder) { builder.StartObject(1); }
        public static void AddFingerPrints(FlatBufferBuilder builder, VectorOffset FingerPrintsOffset) { builder.AddOffset(0, FingerPrintsOffset.Value, 0); }
        public static VectorOffset CreateFingerPrintsVector(FlatBufferBuilder builder, Offset<PhotoFingerPrint>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
        public static void StartFingerPrintsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
        public static Offset<PhotoFingerPrintDatabase> EndPhotoFingerPrintDatabase(FlatBufferBuilder builder)
        {
            int o = builder.EndObject();
            return new Offset<PhotoFingerPrintDatabase>(o);
        }
        public static void FinishPhotoFingerPrintDatabaseBuffer(FlatBufferBuilder builder, Offset<PhotoFingerPrintDatabase> offset) { builder.Finish(offset.Value); }
    };


}
