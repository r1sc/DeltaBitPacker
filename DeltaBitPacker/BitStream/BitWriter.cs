using System;
using System.IO;
using System.Text;

namespace DeltaBitPacker
{
    public class BitWriter
    {
        private readonly Stream _stream;
        private int _bitPosition;
        private int _data;
        private bool _flushed;

        public BitWriter(Stream stream)
        {
            _stream = stream;
        }
        
        public void WriteBool(bool value, int bits = 1)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bits);
        }
        
        public void WriteFloat(float value, int bits = 32)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bits);
        }

        public void WriteUInt16(UInt16 value, int bits = 16)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bits);
        }

        public void WriteInt16(Int16 value, int bits = 16)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bits);
        }
        
        public void WriteUInt32(UInt32 value, int bits = 32)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bits);
        }

        public void WriteInt32(Int32 value, int bits = 32)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bits);
        }

        public void WriteString(string value)
        {
            WriteByte((byte) value.Length);
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteBytes(bytes, bytes.Length*8);
        }

        public void WriteByte(byte value, int bits=8)
        {
            WriteBytes(new []{value}, bits);
        }
        
        private void WriteBytes(byte[] bytes, int bits)
        {
            if (_flushed)
                throw new Exception("Bitwriter has been flushed");

            for (int i = 0; i < bits; i++)
            {
                var byteIndex = (byte) (i/8);
                var bitIndex = (byte) (i%8);
                byte value = (byte) ((bytes[byteIndex] >> bitIndex) & 1);
                if(value != 0)
                    _data |= (value << _bitPosition);
                
                _bitPosition++;
                if (_bitPosition == 8)
                {
                    _stream.WriteByte((byte) _data);
                    _bitPosition = 0;
                    _data = 0;
                }
            }
        }

        public void Flush()
        {
            if (_bitPosition > 0)
                _stream.WriteByte((byte) _data);
            _flushed = true;
        }
    }
}