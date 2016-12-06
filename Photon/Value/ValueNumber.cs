﻿
namespace Photon
{
    class ValueNumber : Value
    {
        float _data = 0;

        public ValueNumber( float data )
        {
            _data = data;
        }

        public float Raw
        {
            get { return _data; }
        }

        internal override bool Equal(Value other)
        {
            var otherT = other as ValueNumber;
            if (otherT == null)
                return false;

            return otherT._data == _data;
        }
        public override string DebugString()
        {
            return _data.ToString() + "(number)";
        }

        public override string ToString()
        {
            return _data.ToString();
        }

        public override ValueKind Kind
        {
            get { return ValueKind.Number; }
        }
    }



 
}
