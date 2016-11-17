﻿using Photon;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PhotonToy
{
    enum DebuggerMode
    {
        Continue,
        StepIn,
        StepOut,
        StepOver,
    }



    class DebugBox
    {
        public Control _invoker;

        VMachine _vm;
        Executable _exe;

        Thread _thread;
        AutoResetEvent _debugSignal = new AutoResetEvent(false);
        AutoResetEvent _exitSignal = new AutoResetEvent(false);

        public Action<VMState> OnBreak;
        public Action<Executable> OnLoad;
        public Action<string> OnError;
       
        VarGuard<int> _expectCallDepth = new VarGuard<int>(-1);
        VarGuard<int> _callDepth = new VarGuard<int>(0);
        VarGuard<DebuggerMode> _mode = new VarGuard<DebuggerMode>(DebuggerMode.StepIn);
        
        object _stateGuard = new object();

        public State State
        {
            get {

                lock (_stateGuard)
                {
                    if (_vm == null)
                        return State.None;

                    return _vm.State;
                }

            }
        }

        public DebugBox( Control invoker )
        {
            _invoker = invoker;
        }

        public SourceFile Source
        {
            get;
            set;
        }


        public void Start(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return;

            var file = new SourceFile(File.ReadAllText(filename));            

            try
            {
                _exe = Compiler.Compile(file);
            }
            catch( Exception e )
            {
                if (OnError != null)
                    OnError(e.ToString());

                return;
            }            

            if (OnLoad != null)
            {
                OnLoad(_exe);
            }

            _vm = new VMachine();

            _mode.Value = DebuggerMode.StepIn;
            _thread = new Thread(VMThread);
            _thread.Name = "DebugBox";
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Stop( )
        {
            switch( State )
            {
                case State.Breaking:
                    {
                        _vm.Stop();

                        Operate(DebuggerMode.Continue);

                        _exitSignal.WaitOne();    
                    }
                    break;
                case State.Running:
                    {
                        // 在跑, 直接停

                        _thread.Abort();
                    }
                    break;
            }


            
        }

        delegate void InvokeHandler(Action callback);
        void SafeCall(Action callback)
        {

            if (_invoker != null && _invoker.InvokeRequired)
            {
                _invoker.BeginInvoke(new InvokeHandler(SafeCall), callback);
            }
            else
            {
                callback();
            }
        }

        public void Operate( DebuggerMode hookmode )
        {
            if (_vm == null)
                return;

            _mode.Value = hookmode;
            switch ( hookmode )
            {
                case DebuggerMode.Continue:
                case DebuggerMode.StepIn:
                    {
                        _expectCallDepth.Value = -1;
                    }
                    break;
                case DebuggerMode.StepOver:
                    {
                        _expectCallDepth.Value = _callDepth.Value;
                    }
                    break;
                case DebuggerMode.StepOut:
                    {
                        _expectCallDepth.Value = _callDepth.Value - 1;
                    }
                    break;

            }

            _debugSignal.Set();
        }

        void VMThread( )
        {
            _vm.SetHook(DebugHook.AssemblyLine, (vm) =>
            {
                switch( _mode.Value )
                {
                    case DebuggerMode.Continue:
                        return;

                    case DebuggerMode.StepOver:
                        {
                            // 没有恢复到期望深度, 继续执行
                            if (_expectCallDepth.Value != _callDepth.Value)
                            {
                                return;
                            }
                        }
                        break;
                    case DebuggerMode.StepOut:
                        {
                            // 没有恢复到期望深度, 继续执行
                            if (_callDepth.Value > _expectCallDepth.Value)
                            {
                                return;
                            }

                        }
                        break;
                }

                _expectCallDepth.Value = -1;
                _mode.Value = DebuggerMode.StepIn;

                var vms = new VMState(vm);
                SafeCall(delegate
                {
                    if (OnBreak != null)
                    {
                        OnBreak(vms);
                    }

                });

                _debugSignal.WaitOne();                
            });

            _vm.SetHook(DebugHook.Call, (vm) =>
            {
                _callDepth.Value++;
            });

            _vm.SetHook(DebugHook.Return, (vm) =>
            {
                _callDepth.Value--;
            });


            _vm.Run(_exe);

            var vms2 = new VMState(_vm);

            SafeCall(delegate
            {
                if (OnBreak != null)
                {

                    OnBreak(vms2);
                }

            });

            _exitSignal.Set();
        }
    }
}