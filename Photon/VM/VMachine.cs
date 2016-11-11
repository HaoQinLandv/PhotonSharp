﻿using System.Collections.Generic;
using Photon.Model;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Photon.VM
{
    public enum State
    {
        Standby = 0,
        Running,
        Breaking,
    }

    public enum DebugHook
    {        
        ExecInstruction,        // 每个指令运行前
        SourceLine,             // 源码行执行前
        Call,                   // 函数执行前
        Return,                 // 返回函数后
        MAX,
    }

    public partial class VMachine
    {
        // 数据交换栈
        DataStack _dataStack = new DataStack(10);

        // 数据寄存器
        Register _localReg = new Register("R", 10);

        // 运行帧栈
        Stack<RuntimeFrame> _frameStack = new Stack<RuntimeFrame>();

        // 当前运行帧
        RuntimeFrame _currFrame;

        Action<VMachine>[] _hook = new Action<VMachine>[(int)DebugHook.MAX];

        
        // 运行状态
        State _state = State.Standby;

        struct RegRange
        {
            public int Min;
            public int Max;
        }

        // 寄存器使用范围栈
        Stack<RegRange> _regBaseStack = new Stack<RegRange>();
        int _regBase = 0;

        Executable _exe;        

        // 指令集
        Instruction[] _instruction = new Instruction[(int)Opcode.MAX];

        // 当前寄存器最小使用位置
        public int RegBase
        {
            get { return _regBase; }
        }

        public State State 
        {
            get { return _state; }
        }


        public bool ShowDebugInfo
        {
            get;
            set;
        }

        public DataStack Stack
        {
            get { return _dataStack; }
        }

        public Register Reg
        {
            get { return _localReg; }
        }

        public Executable Exec
        {
            get { return _exe; }
        }

        public RuntimeFrame CurrFrame
        {
            get { return _currFrame; }
        }

        Command CurrCommand
        {
            get
            {

                int pc = _currFrame.PC;
                if (pc >= _currFrame.CmdSet.Commands.Count || pc < 0)
                {
                    return null;
                }

                return _currFrame.CmdSet.Commands[pc];
            }
        }

        public VMachine()
        {
            StaticRegisterAssemblyInstructions();
        }

        void StaticRegisterAssemblyInstructions()
        {
            var ass = Assembly.GetExecutingAssembly();

            foreach (var t in ass.GetTypes())
            {
                var att = t.GetCustomAttribute<InstructionAttribute>();
                if (att == null)
                    continue;

                var cmd = Activator.CreateInstance(t) as Instruction;
                cmd.vm = this;
                _instruction[(int)att.Cmd] = cmd;                
            }
        }

        string InstructToString( Command cmd )
        {
            var inc = _instruction[(int)cmd.Op];

            if (inc == null)
            {            
                return string.Empty;
            }

            return inc.Print( cmd );
        }

        void ExecCode(Command cmd)
        {
            var inc = _instruction[(int)cmd.Op];

            if (inc == null)
            {
                throw new RuntimeExcetion("invalid instruction");                
            }


            if( inc.Execute( cmd) )
            {
                _currFrame.PC++;
            }
        }

        public void SetHook(DebugHook hook, Action<VMachine> callback )
        {
            _hook[(int)hook] = callback;
        }

        public void EnterFrame( int funcIndex )
        {
            CallHook(DebugHook.Call);

            var newFrame = new RuntimeFrame(_exe.CmdSet[funcIndex] );

            if ( _currFrame != null )
            {
                _frameStack.Push(_currFrame);
            }

            _currFrame = newFrame;

            // globa不用local寄存器

            // 第一层的reg是0, 不记录
            if (_regBaseStack.Count > 0)
            {
                _regBase = _regBaseStack.Peek().Max;
            }

            RegRange rr;
            rr.Min = _regBase;
            rr.Max = _regBase + newFrame.CmdSet.RegCount;

            _localReg.SetUsedCount(rr.Max);

            // 留作下次调用叠加时使用
            _regBaseStack.Push(rr);
        }

        public void LeaveFrame( )
        {
            if ( _currFrame.RestoreDataStack )
            {
                _dataStack.Count = _currFrame.DataStackBase;
            }

            _currFrame = _frameStack.Pop();

            _regBaseStack.Pop();

            var rr = _regBaseStack.Peek();
            _regBase = rr.Min;

            _localReg.SetUsedCount(rr.Max);

            CallHook(DebugHook.Return);
        }

        void CallHook( DebugHook hook )
        {
            var func = _hook[(int)hook];
            if (func != null)
            {
                _state = VM.State.Breaking;

                func(this);

                _state = VM.State.Running;
            }
        }

        public void Stop( )
        {
            _currFrame.PC = -1;
        }

        public void Run( Executable exe, SourceFile file )
        {
            _exe = exe;

            _frameStack.Clear();
            _dataStack.Clear();

            EnterFrame(0);

            int currSrcLine = 0;

            _state = VM.State.Running;
            
            while (true)
            {
                var cmd = CurrCommand;
                if (cmd == null)
                    break;

                if (ShowDebugInfo)
                {
                    Debug.WriteLine("{0}|{1}", cmd.CodePos.Line, file.GetLine(cmd.CodePos.Line));
                    Debug.WriteLine("{0,5} {1,2}| {2} {3}", _currFrame.CmdSet.Name, _currFrame.PC, cmd.Op.ToString(), InstructToString(cmd) );
                }

                // 源码行有变化时
                if (currSrcLine == 0 || currSrcLine != cmd.CodePos.Line)
                {
                    if ( currSrcLine != 0 )
                    {
                        CallHook(DebugHook.SourceLine);
                    }
                    
                    currSrcLine = cmd.CodePos.Line;
                }


                // 每条指令执行前
                CallHook(DebugHook.ExecInstruction);


                ExecCode(cmd);

                // 打印执行完后的信息
                if (ShowDebugInfo)
                {
                    // 寄存器信息
                    Reg.DebugPrint();

                    // 数据栈信息
                    Stack.DebugPrint();
                    

                    Debug.WriteLine("");
                }

            }
        }
    }
}
