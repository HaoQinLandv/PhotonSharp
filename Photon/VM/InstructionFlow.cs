﻿using Photon.Model;

namespace Photon.VM
{
    [Instruction(Cmd = Opcode.Jz)]
    class CmdJnz : Instruction
    {
        public override bool Execute( Command cmd)
        {
            var targetPC = cmd.DataA;

            var d = vm.Stack.Pop().CastNumber();

            if ( d == 0 )
            {
                vm.CurrFrame.PC = targetPC;                
                return true;
            }

            return true;
        }

        public override string Print( Command cmd)
        {
            return string.Format("Condition: {0}, PC : {1}", vm.Stack.ValueToString(),  cmd.DataA);
        }
    }

    [Instruction(Cmd = Opcode.Jmp)]
    class CmdJmp : Instruction
    {
        public override bool Execute( Command cmd)
        {
            vm.CurrFrame.PC = cmd.DataA;

            return false;
        }

        public override string Print( Command cmd)
        {
            return string.Format("PC : {0}", cmd.DataA);
        }
    }

    [Instruction(Cmd = Opcode.Call)]
    class CmdCall : Instruction
    {
        public override bool Execute( Command cmd)
        {
            var argCount = cmd.DataA;

            var obj = vm.Stack.Pop();

            var func = obj as ValueFunc;

            if ( func != null )
            {                

                // 更换当前上下文
                vm.EnterFrame(func.Index);

                vm.CurrFrame.Closure = obj as ValueClosure;

                // 调用结束时需要平衡栈
                if (cmd.DataB != 0)
                {
                    vm.CurrFrame.RestoreDataStack = true;
                }

                // 将栈转为被调用函数的寄存器
                for (int i = 0; i < argCount; i++)
                {
                    var arg = vm.Stack.Get( -i - 1 );
                    vm.LocalRegister.Set(argCount - i - 1 + vm.RegBase, arg);
                }

                // 清空栈
                vm.Stack.PopMulti(argCount);

                // 记录当前的数据栈位置
                vm.CurrFrame.DataStackBase = vm.Stack.Count;

                // 马上跳到下个执行域            
                return false;
            }

            var dg = obj as ValueDelegate;
            if ( dg != null )
            {
               
                // 外部调用不进行栈调整
                var stackBeforeCall = vm.Stack.Count;

                var retValueCount = dg.Entry(vm);

                // 调用结束时需要平衡栈( 返回值没有被用到 )
                if (cmd.DataB != 0 )
                {
                    // 调用前(包含参数+ delegate)
                    vm.Stack.Count = stackBeforeCall - argCount;
                }
                else
                {
                    vm.Stack.Cut(stackBeforeCall - argCount, retValueCount);
                }

                return true;
            }

            throw new RuntimeExcetion("expect function or delegate");
        }


        public override string Print( Command cmd)
        {
            return string.Format("ArgCount : {0}  Func: {1}  BalanceStack: {2}", cmd.DataA,  vm.Stack.ValueToString( ), cmd.DataB );
        }
    }
    [Instruction(Cmd = Opcode.Ret)]
    class CmdRet : Instruction
    {
        public override bool Execute( Command cmd)
        {
            vm.LeaveFrame();

            return true;
        }
    }
  
    [Instruction(Cmd = Opcode.Exit)]
    class CmdExit : Instruction
    {
        public override bool Execute( Command cmd)
        {            
            vm.CurrFrame.PC = -1;

            return false;
        }
    }

    [Instruction(Cmd = Opcode.Closure)]
    class CmdClosure : Instruction
    {
        public override bool Execute( Command cmd)
        {
            var f = vm.Executable.Constants.Get(cmd.DataA).CastFunc();

            vm.Stack.Push(new ValueClosure(f));

            return true;
        }
    }
}
