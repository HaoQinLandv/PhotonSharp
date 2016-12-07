﻿
namespace Photon
{
    class ValueFloat32 : Value
    {
        float _data = 0;

        public ValueFloat32( float data )
        {
            _data = data;
        }

        public float RawValue
        {
            get { return _data; }
        }

        internal override object Raw
        {
            get { return _data; }
        }

        public override bool Equals(object other)
        {
            var otherT = other as ValueFloat32;
            if (otherT == null)
                return false;

            return otherT._data == _data;
        }
        public override string DebugString()
        {
            return _data.ToString() + " (float32)";
        }

        public override ValueKind Kind
        {
            get { return ValueKind.Float32; }
        }

        internal override Value BinaryOperate(Opcode code, Value other)
        {
            var a = RawValue;
            var b = Convertor.ValueToFloat32(other);

            switch (code)
            {
                case Opcode.ADD:
                    return new ValueFloat32(a + b);
                case Opcode.SUB:
                    return new ValueFloat32(a - b);
                case Opcode.MUL:
                    return new ValueFloat32(a * b);
                case Opcode.DIV:
                    return new ValueFloat32(a / b);
                case Opcode.GT:
                    return new ValueBool(a > b);
                case Opcode.GE:
                    return new ValueBool(a >= b);
                case Opcode.LT:
                    return new ValueBool(a < b);
                case Opcode.LE:
                    return new ValueBool(a <= b);
                case Opcode.EQ:
                    return new ValueBool(a == b);
                case Opcode.NE:
                    return new ValueBool(a != b);
                default:
                    throw new RuntimeException("Unknown binary operator:" + code.ToString());
            }
        }

        internal override Value UnaryOperate(Opcode code)
        {
            var a = RawValue;

            switch (code)
            {
                case Opcode.MINUS:
                    return new ValueFloat32(-a);
                default:
                    throw new RuntimeException("Unknown unary operator:" + code.ToString());
            }
        }
    }



 
}
