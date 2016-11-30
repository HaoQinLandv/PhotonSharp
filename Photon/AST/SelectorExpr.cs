﻿using SharpLexer;
using System.Collections.Generic;

namespace Photon
{
    // a.b   x=a  selector=b
    internal class SelectorExpr : Expr
    {
        public Expr X;
        public Ident Selector;
        public TokenPos DotPos;
        
        public SelectorExpr(Expr x, Ident sel, TokenPos pos)
        {
            X = x;
            Selector = sel;
            DotPos = pos;
            BuildRelation();

        }

        public override IEnumerable<Node> Child()
        {
            yield return X;

            yield return Selector;
        }

        public override string ToString()
        {
            return "SelectorExpr";
        }

        internal int CompileSelfParameter(CompileParameter param)
        {
            var xident = X as Ident;
            if (xident != null)
            {
                if (xident.Symbol == null)
                {
                    throw new CompileException("undefined symbol: " + xident.Name, DotPos);
                }


                switch( xident.Symbol.Usage )
                {
                    case SymbolUsage.Parameter:
                    case SymbolUsage.Variable:
                        {
                            X.Compile(param.SetLHS(false));
                            return 1;
                        }                     
                }
            }

            return 0;
        }

        internal override void Compile(CompileParameter param)
        {
            var xident = X as Ident;
            if ( xident != null )
            {
                if ( xident.Symbol == null )
                {
                    throw new CompileException("undefined symbol: " + xident.Name, DotPos);
                }
                
                switch( xident.Symbol.Usage )
                {
                        // 包.函数名
                    case SymbolUsage.Package:
                        {
                            var pkg = param.Pkg.Exe.GetPackageByName(xident.Name);

                            if (pkg == null)
                            {
                                throw new CompileException("package not found: " + xident.Name, DotPos);
                            }

                            // Ident直接出代码
                            Selector.Compile(param.SetLHS(false).SetPackage(pkg));
                        }
                        break;
                    // 实例.函数名  转换为  函数名( 实例, p2...)
                    case SymbolUsage.Parameter:
                    case SymbolUsage.Variable:
                        {                            
                            // 使用字符串的Const值作为成员函数的key索引                            
                            Selector.Compile(param.SetLHS(false));
                        }
                        break;
                }

            }
            else
            {
                // 动态表达式, 需要用指令解析
                X.Compile(param);

                var c = new ValueString(Selector.Name);

                var ci = param.Pkg.Constants.Add(c);

                param.CS.Add(new Command(Opcode.SEL, ci));
            }

           
        }
    }
}
