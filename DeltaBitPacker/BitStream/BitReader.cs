using System;
using System.IO;

namespace DeltaBitPacker
{
    public class BitReader
    {
        private int _bitPosition;
        private int _data;
        private readonly Stream _stream;

        public BitReader(Stream stream)
        {
            _stream = stream;
            _data = stream.ReadByte();
        }

        public bool ReadBool()
        {
            return Read(1)[0] == 1;
        }

        public byte[] Read(int bits)
        {
            var numBytes = Math.Max(1, bits / 8);
            var retData = new byte[numBytes];
            for (int i = 0; i < bits; i++)
            {
                var byteIndex = (byte)(i / 8);
                var bitIndex = (byte)(i % 8);

                if (_data == -1)
                    throw new Exception("Attempt to read beyond end of stream");
                bool bit = ((_data >> _bitPosition) & 1) == 1;
                if(bit)
                    retData[byteIndex] |= (byte)(1 << bitIndex);

                _bitPosition++;
                if (_bitPosition == 8)
                {
                    _data = _stream.ReadByte();
                    _bitPosition = 0;
                }
            }
            return retData;
        }
    }
}