﻿
using MarkSerializer;
namespace Photon
{
    class ValueClassType : Value
    {
        internal int ID = -1;

        protected ObjectName _name;
        public ObjectName Name
        {
            get { return _name; }
        }

        internal virtual int GetInheritLevel() { return 0; }

        internal virtual void OnSerializeDone(Executable exe) { }

        public ValueClassType()
        {

        }

        public override void Serialize(BinarySerializer ser)
        {
            ser.Serialize(ref ID);
            ser.Serialize(ref _name);
        }


        internal ValueClassType( ObjectName name)
        {            
            _name = name;
        }

        public override bool Equals(object v)
        {
            var other = v as ValueClassType;
            if (other == null)
                return false;

            return _name.Equals(other._name);
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public override string TypeName
        {
            get { return Name.ToString(); }
        }

        internal virtual ValueObject CreateInstance( )
        {
            return null;
        }
    }

}
