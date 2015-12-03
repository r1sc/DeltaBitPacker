using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DeltaBitPacker
{
    public class BitSerializer
    {
        private readonly Dictionary<Type, FieldInfo[]> _typeMappings = new Dictionary<Type, FieldInfo[]>();

        public void RegisterType(Type type)
        {
            _typeMappings.Add(type, type.GetFields());
        }
        
        public void Deserialize(BitReader reader, bool delta, Type type, ref object o1)
        {
            if (delta)
            {
                var hasValue = reader.ReadBool();
                if (!hasValue)
                {
                    o1 = null;
                    return;
                }
            }

            if (type.IsArray)
            {
                var ary2 = (Array)o1;
                var elementType = type.GetElementType();
                for (int i = 0; i < ary2.Length; i++)
                {
                    object obj = ary2.GetValue(i);
                    ReadField(reader, delta, elementType, ref obj);
                    ary2.SetValue(obj, i);
                }
            }
            else if (type == typeof(string))
            {
                var len = reader.Read(8)[0];
                o1 = Encoding.UTF8.GetString(reader.Read(len * 8));
            }
            else if (type.IsValueType && !type.IsEnum)
            {
                if (!_typeMappings.ContainsKey(type))
                    throw new Exception("Type mapping for " + type.FullName + " not registered");
                for (int i = 0; i < _typeMappings[type].Length; i++)
                {
                    var field = _typeMappings[type][i];
                    object val = field.GetValue(o1);
                    ReadField(reader, delta, field.FieldType, ref val);
                    field.SetValue(o1, val);
                }
            }
            else
            {
                throw new Exception("This shouldn't happen");
            }
        }

        private void ReadField(BitReader reader, bool delta, Type type, ref object val)
        {
            if (type.IsArray || _typeMappings.ContainsKey(type) || type == typeof(string))
            {
                Deserialize(reader, delta, type, ref val);
                return;
            }

            if (delta)
            {
                var hasValue = reader.ReadBool();
                if (!hasValue)
                    return;
            }

            if (type == typeof(float))
            {
                val = BitConverter.ToSingle(reader.Read(32), 0);
            }
            else if (type == typeof(UInt16))
            {
                val = BitConverter.ToUInt16(reader.Read(16), 0);
            }
            else if (type == typeof(Int16))
            {
                val = BitConverter.ToInt16(reader.Read(16), 0);
            }
            else if (type == typeof(UInt32))
            {
                val = BitConverter.ToUInt32(reader.Read(32), 0);
            }
            else if (type == typeof(Int32))
            {
                val = BitConverter.ToInt32(reader.Read(32), 0);
            }
            else if (type == typeof(byte))
            {
                val = reader.Read(8)[0];
            }
            else if (type == typeof(bool))
            {
                val = reader.ReadBool();
            }
            else
            {
                throw new Exception("No serializer for " + type.FullName);
            }
        }

        public void Serialize(BitWriter writer, object o1)
        {
            Type type = o1.GetType();
            if (type.IsArray)
            {
                var ary1 = (Array)o1;
                var elementType = type.GetElementType();
                for (int i = 0; i < ary1.Length; i++)
                {
                    var aryVal1 = ary1.GetValue(i);
                    WriteField(writer, false, elementType, null, aryVal1);
                }
            }
            else if (type == typeof(string))
            {
                writer.WriteString((string)o1);
            }
            else if (type.IsValueType && !type.IsEnum)
            {
                if (!_typeMappings.ContainsKey(type))
                    throw new Exception("Type mapping for " + type.FullName + " not registered");
                foreach (var field in _typeMappings[type])
                {
                    var val1 = o1 == null ? null : field.GetValue(o1);
                    WriteField(writer, false, field.FieldType, null, val1);
                }
            }
            else
            {
                throw new Exception("This shouldn't happen");
            }
        }

        public void SerializeDelta(BitWriter writer, object o1, object o2)
        {
            if (o1 == null && o2 == null)
            {
                writer.WriteBool(false);
                return;
            }
            if (o2 == null)
            {
                writer.WriteBool(false);    // doesn't have a value
                return;
            }
            writer.WriteBool(true);         // has a value

            Type type = o1 == null ? o2.GetType() : o1.GetType();
            if (type.IsArray)
            {
                var ary1 = (Array)o1;
                var ary2 = (Array)o2;
                var elementType = type.GetElementType();
                for (int i = 0; i < ary1.Length; i++)
                {
                    var aryVal1 = ary1.GetValue(i);
                    var aryVal2 = ary2.GetValue(i);
                    WriteField(writer, true, elementType, aryVal1, aryVal2);
                }
            }
            else if (type == typeof(string))
            {
                writer.WriteString((string)o2);
            }
            else if (type.IsValueType && !type.IsEnum)
            {
                if (!_typeMappings.ContainsKey(type))
                    throw new Exception("Type mapping for " + type.FullName + " not registered");
                foreach (var field in _typeMappings[type])
                {
                    var val1 = o1 == null ? null : field.GetValue(o1);
                    var val2 = field.GetValue(o2);
                    WriteField(writer, true, field.FieldType, val1, val2);
                }
            }
            else
            {
                throw new Exception("This shouldn't happen");
            }
        }

        private void WriteField(BitWriter writer, bool delta, Type type, object o1, object o2)
        {
            if (type.IsArray || _typeMappings.ContainsKey(type) || type == typeof(string))
            {
                if (delta)
                    SerializeDelta(writer, o1, o2);
                else
                    Serialize(writer, o2);
                return;
            }

            if (delta)
            {
                bool changed = !o1.Equals(o2);
                writer.WriteBool(changed);

                if (!changed)
                    return;
            }

            if (type == typeof (float))
            {
                writer.WriteFloat((float) o2);
            }
            else if (type == typeof (UInt16))
            {
                writer.WriteUInt16((UInt16) o2);
            }
            else if (type == typeof (Int16))
            {
                writer.WriteInt16((Int16) o2);
            }
            else if (type == typeof (UInt32))
            {
                writer.WriteUInt32((UInt32) o2);
            }
            else if (type == typeof (Int32))
            {
                writer.WriteInt32((Int32) o2);
            }
            else if (type == typeof (byte))
            {
                writer.WriteByte((byte) o2);
            }
            else if (type == typeof (bool))
            {
                writer.WriteBool((bool) o2);
            }
            else
            {
                throw new Exception("No serializer for " + type.FullName);
            }
        }
    }
}