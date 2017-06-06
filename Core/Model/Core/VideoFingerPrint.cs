// automatically generated by the FlatBuffers compiler, do not modify

namespace Core
{

    using System;
    using FlatBuffers;

    internal struct VideoFingerPrint : IFlatbufferObject
    {
        private Table __p;
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public static VideoFingerPrint GetRootAsVideoFingerPrint(ByteBuffer _bb) { return GetRootAsVideoFingerPrint(_bb, new VideoFingerPrint()); }
        public static VideoFingerPrint GetRootAsVideoFingerPrint(ByteBuffer _bb, VideoFingerPrint obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
        public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
        public VideoFingerPrint __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

        public string FilePath { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
        public ArraySegment<byte>? GetFilePathBytes() { return __p.__vector_as_arraysegment(4); }
        public FrameFingerPrint? FrameFingerPrints(int j) { int o = __p.__offset(6); return o != 0 ? (FrameFingerPrint?)(new FrameFingerPrint()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
        public int FrameFingerPrintsLength { get { int o = __p.__offset(6); return o != 0 ? __p.__vector_len(o) : 0; } }

        public static Offset<VideoFingerPrint> CreateVideoFingerPrint(FlatBufferBuilder builder,
            StringOffset filePathOffset = default(StringOffset),
            VectorOffset frameFingerPrintsOffset = default(VectorOffset))
        {
            builder.StartObject(2);
            VideoFingerPrint.AddFrameFingerPrints(builder, frameFingerPrintsOffset);
            VideoFingerPrint.AddFilePath(builder, filePathOffset);
            return VideoFingerPrint.EndVideoFingerPrint(builder);
        }

        public static void StartVideoFingerPrint(FlatBufferBuilder builder) { builder.StartObject(2); }
        public static void AddFilePath(FlatBufferBuilder builder, StringOffset filePathOffset) { builder.AddOffset(0, filePathOffset.Value, 0); }
        public static void AddFrameFingerPrints(FlatBufferBuilder builder, VectorOffset frameFingerPrintsOffset) { builder.AddOffset(1, frameFingerPrintsOffset.Value, 0); }
        public static VectorOffset CreateFrameFingerPrintsVector(FlatBufferBuilder builder, Offset<FrameFingerPrint>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
        public static void StartFrameFingerPrintsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
        public static Offset<VideoFingerPrint> EndVideoFingerPrint(FlatBufferBuilder builder)
        {
            int o = builder.EndObject();
            return new Offset<VideoFingerPrint>(o);
        }
    };


}
