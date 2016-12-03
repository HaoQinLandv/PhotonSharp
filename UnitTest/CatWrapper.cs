﻿using Photon;
using System;

namespace PhotonCompiler
{
	[NativeWrapperClass(typeof(Cat))]
	public class CatWrapper
	{
		[NativeEntry(NativeEntryType.ClassMethod)]
		public static int xx( VMachine vm )
		{
			var phoClassIns = vm.DataStack.GetNativeInstance<Cat>(0);
			
			var a = vm.DataStack.GetInteger32(1);
			var c = vm.DataStack.GetString(2);
			
			Int32 b;
			var phoRetArg = phoClassIns.xx( a, c, out b );

			vm.DataStack.PushInteger32( b );
			vm.DataStack.PushString( phoRetArg );
			
			return 2;
		}
		
		[NativeEntry(NativeEntryType.ClassMethod)]
		public static int foo( VMachine vm )
		{
			var phoClassIns = vm.DataStack.GetNativeInstance<Cat>(0);
			
			var a = vm.DataStack.GetInteger32(1);
			
			var phoRetArg = phoClassIns.foo( a );

			vm.DataStack.PushString( phoRetArg );
			
			return 1;
		}
		
	}
}
