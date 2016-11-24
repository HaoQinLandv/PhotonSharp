﻿using System.Collections.Generic;
using System.Diagnostics;

namespace Photon
{
    public class ConstantSet : DataAccessor
    {
        List<Value> _cset = new List<Value>();

        internal int Add(Value inc)
        {
            int index = 0;
            foreach( var c in _cset )
            {
                if (c.Equal(inc))
                    return index;

                index++;
            }

            _cset.Add( inc );

            return _cset.Count - 1;
        }

        internal int Count
        {
            get { return _cset.Count; }
        }

        internal override Value Get(int index)
        {
            return _cset[index];
        }

        public void DebugPrint( )
        {
            Debug.WriteLine("constant:");

            int index = 0;
            foreach (var c in _cset)
            {
                Debug.WriteLine("C{0}: {1}", index, c.ToString());
                index++;
            }

            Debug.WriteLine("");
        }
    }
}
