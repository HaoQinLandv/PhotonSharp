﻿using System;
using Photon.AST;
using Photon.OpCode;
using Photon.Scanner;


namespace Photon.Compiler
{
    public partial class Compiler
    {

        bool CompileExpr(CommandSet cm, Node n, bool lhs)
        {

            if (n is BinaryExpr)
            {
                var v = n as BinaryExpr;
                CompileNode(cm, v.X, lhs);
                CompileNode(cm, v.Y, lhs);

                switch (v.Op)
                {
                    case TokenType.Add:
                        cm.Add(new Command(Opcode.Add));
                        break;
                }

                return true;
            }

            if (n is BasicLit)
            {
                var v = n as BasicLit;

                var c = Lit2Const(v);
                var ci = _exe.Constants.Add(c);

                cm.Add(new Command(Opcode.LoadC, ci)).Comment = c.GetDesc();

                return true;
            }

            if (n is Ident)
            {
                var v = n as Ident;

                if (lhs)
                {
                    cm.Add(new Command(Opcode.SetR, v.ScopeInfo.Slot)).Comment = v.Name;
                }
                else
                {
                    cm.Add(new Command(Opcode.LoadR, v.ScopeInfo.Slot)).Comment = v.Name;
                }

                return true;
            }

            if (n is CallExpr)
            {
                var v = n as CallExpr;
                CompileNode(cm, v.Func, false);

                foreach (var arg in v.Args)
                {
                    CompileNode(cm, arg, false);
                }


                cm.Add(new Command(Opcode.Call, v.Args.Count, v.RegBase));
                return true;
            }

            return false;
        }
    }
}
