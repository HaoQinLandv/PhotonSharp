﻿

using System.Collections.Generic;

namespace Photon
{
    internal partial class Parser
    {
        Stmt ParseFuncDecl()
        {
            var funcPos = CurrTokenPos;

            Expect(TokenType.Func);

            var scope = OpenScope(ScopeType.Function, funcPos );

            Ident funcName;
            Ident className = null ;

            var NameA = ParseIdent();

            if (CurrTokenType == TokenType.Dot)
            {
                Next();

                funcName = ParseIdent();

                className = NameA;                
            }
            else
            {
                funcName = NameA;
            }


            var paramlist = ParseParameters(scope, className != null );
            
            if ( CurrTokenType == TokenType.LBrace )
            {

                var body = ParseBody(scope);

                var decl = new FuncDeclare(funcName, body, new FuncType(funcPos, paramlist, scope) );
                decl.ClassName = className;

                funcName.Symbol = Declare(decl, _global, funcName.Name, funcName.DefinePos, SymbolUsage.Func);

                return decl;
            }
            else
            {
                // 声明已经结束
                CloseScope();

                // 函数前置声明
                var decl = new DelegateDeclare(funcName, new FuncType(funcPos, paramlist, scope) );

                funcName.Symbol = Declare(decl, _global, funcName.Name, funcName.DefinePos, SymbolUsage.Delegate);

                return decl;
            }
            
        }

        List<Ident> ParseParameters( Scope s, bool isMethod )
        {
            Expect(TokenType.LBracket);

            List<Ident> p = new List<Ident>();


            if (CurrTokenType != TokenType.RBracket )
            {
                while (true)
                {
                    var param = ParseIdent();
                    p.Add(param);


                    SymbolUsage usage;
                    if (isMethod && p.Count == 1 )
                    {
                        usage = SymbolUsage.SelfParameter;
                    }
                    else
                    {
                        usage = SymbolUsage.Parameter;
                    }


                    Declare(param, s, param.Name, param.DefinePos, usage);

                    if (CurrTokenType != TokenType.Comma)
                    {
                        break;
                    }

                    Next();
                }
            }
            
            Expect(TokenType.RBracket);

            return p;
        }


        BlockStmt ParseBody(Scope s) 
        {
            var lpos = CurrTokenPos;
            Expect(TokenType.LBrace);

            _topScope = s;

            var list = ParseStatmentList();

            CloseScope();

            var rpos = CurrTokenPos;
            Expect(TokenType.RBrace);

            return new BlockStmt(list, lpos, rpos);
        }

        Stmt ParseVarDecl()
        {
            var defpos = CurrTokenPos;
            Expect(TokenType.Var);

            var idents = ParseIdentList();

            foreach( var i in idents )
            {
                Declare(i, _topScope, i.Name, i.DefinePos, SymbolUsage.Variable);
            }

            List<Expr> values = new List<Expr>();

            var assignPos = CurrTokenPos;

            if (CurrTokenType == TokenType.Assign)
            {
                Next();

                values = ParseRHSList();

                var lhs = new List<Expr>();
                foreach( var id in idents )
                {
                    lhs.Add( id );
                }

                return new AssignStmt(lhs, values, assignPos);
            }

            return new VarDeclareStmt(idents, defpos);
        }


        Stmt ParseClassDecl( )
        {
            List<Ident> memberVar = new List<Ident>();

            var defpos = CurrTokenPos;
            Expect(TokenType.Class);

            var scope = OpenScope(ScopeType.Class, defpos);

            var className = ParseIdent();

            var decl = new ClassDeclare(className, scope, memberVar, defpos);

            className.Symbol = Declare(decl, _global, className.Name, className.DefinePos, SymbolUsage.Class);

            Expect(TokenType.LBrace);
            
            

            while (CurrTokenType != TokenType.RBrace)
            {
                var param = ParseIdent();
                memberVar.Add(param);                

                Declare(param, scope, param.Name, param.DefinePos, SymbolUsage.Member);                
            }

            CloseScope();

            Next();

            return decl;
        }


    }
}
