﻿using System.Collections.Generic;

namespace Photon.OpCode
{

    public class Executable
    {
        List<CommandSet> _cmdset = new List<CommandSet>();
        ConstantSet _constSet = new ConstantSet();

        public List<CommandSet> CmdSet
        {
            get { return _cmdset; }
        }

        public ConstantSet Constants
        {
            get { return _constSet; }
        }

        
        public void DebugPrint( )
        {
            _constSet.DebugPrint();

            foreach( var cs in _cmdset )
            {
                cs.DebugPrint();
            }
        }

        public CommandSet Add(CommandSet f)
        {
            _cmdset.Add(f);
            return f;
        }
    }
}