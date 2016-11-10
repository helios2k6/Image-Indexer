// automatically generated by the FlatBuffers compiler, do not modify

namespace ImageIndexer
{

    using FlatBuffers;

    internal sealed class Macroblock : Table
    {
        public static Macroblock GetRootAsMacroblock(ByteBuffer _bb) { return GetRootAsMacroblock(_bb, new Macroblock()); }
        public static Macroblock GetRootAsMacroblock(ByteBuffer _bb, Macroblock obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
        public Macroblock __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

        public int Width { get { int o = __offset(4); return o != 0 ? bb.GetInt(o + bb_pos) : (int)0; } }
        public int Height { get { int o = __offset(6); return o != 0 ? bb.GetInt(o + bb_pos) : (int)0; } }
        public Pixel GetPixels(int j) { return GetPixels(new Pixel(), j); }
        public Pixel GetPixels(Pixel obj, int j) { int o = __offset(8); return o != 0 ? obj.__init(__vector(o) + j * 12, bb) : null; }
        public int PixelsLength { get { int o = __offset(8); return o != 0 ? __vector_len(o) : 0; } }

        public static Offset<Macroblock> CreateMacroblock(FlatBufferBuilder builder,
            int width = 0,
            int height = 0,
            VectorOffset pixelsOffset = default(VectorOffset))
        {
            builder.StartObject(3);
            Macroblock.AddPixels(builder, pixelsOffset);
            Macroblock.AddHeight(builder, height);
            Macroblock.AddWidth(builder, width);
            return Macroblock.EndMacroblock(builder);
        }

        public static void StartMacroblock(FlatBufferBuilder builder) { builder.StartObject(3); }
        public static void AddWidth(FlatBufferBuilder builder, int width) { builder.AddInt(0, width, 0); }
        public static void AddHeight(FlatBufferBuilder builder, int height) { builder.AddInt(1, height, 0); }
        public static void AddPixels(FlatBufferBuilder builder, VectorOffset pixelsOffset) { builder.AddOffset(2, pixelsOffset.Value, 0); }
        public static void StartPixelsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(12, numElems, 4); }
        public static Offset<Macroblock> EndMacroblock(FlatBufferBuilder builder)
        {
            int o = builder.EndObject();
            return new Offset<Macroblock>(o);
        }
    };


}