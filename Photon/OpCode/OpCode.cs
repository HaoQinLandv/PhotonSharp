﻿
using System.Collections.Generic;
namespace Photon.OpCode
{
    // S: 栈  R: 寄存器 G: 全局表  C:常量表　I:索引 Top: 栈顶
    public enum Opcode
    {
        Nop,
        Add,    // S[Top] = S[I] + S[I+1]
        Sub,
        Mul,
        Div,
        SetG,  // G[I] = S[I+1]
        LoadG, // S[Top] = G[I]
        LoadC,  // S[Top] = C[I]
        LoadR,    // S[Top] = R[I]
        SetR,     // R[I] = S[I+1]
        Call,       // S[I](S[I+1], ... )
        Ret,     // S[I], S[I+1] = RetValue
    }





}