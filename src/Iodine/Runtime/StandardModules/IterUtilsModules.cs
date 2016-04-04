﻿/**
  * Copyright (c) 2015, GruntTheDivine All rights reserved.

  * Redistribution and use in source and binary forms, with or without modification,
  * are permitted provided that the following conditions are met:
  * 
  *  * Redistributions of source code must retain the above copyright notice, this list
  *    of conditions and the following disclaimer.
  * 
  *  * Redistributions in binary form must reproduce the above copyright notice, this
  *    list of conditions and the following disclaimer in the documentation and/or
  *    other materials provided with the distribution.

  * Neither the name of the copyright holder nor the names of its contributors may be
  * used to endorse or promote products derived from this software without specific
  * prior written permission.
  * 
  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
  * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
  * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
  * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
  * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
  * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
  * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
  * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
  * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
  * DAMAGE.
**/

using System;
using System.Collections;

namespace Iodine.Runtime
{
	[IodineBuiltinModule ("iterutils")]
	public class IterUtilsModule : IodineModule
	{
		/*
		 * For simplicitly its easiest to implement most of functions found in this module 
		 * using C# generator functions. However, C# generator functions do not implement
		 * IEnumerator.Reset ().
		 * 
		 * Solution: Pass a delegate that will reset the enumerator
		 */
		delegate IEnumerator GeneraterResetDelegate ();

		class InternalGenerator : IodineObject
		{
			private IEnumerator generator;
			private GeneraterResetDelegate resetFunc = null;

			private static IodineTypeDefinition TypeDefinition = new IodineTypeDefinition ("InternalGenerator");

			public InternalGenerator (GeneraterResetDelegate resetFunc)
				: base (TypeDefinition)
			{
				this.resetFunc = resetFunc;
			}

			public override IodineObject IterGetCurrent (VirtualMachine vm)
			{
				return (IodineObject)generator.Current;
			}

			public override bool IterMoveNext (VirtualMachine vm)
			{
				return generator.MoveNext ();
			}

			public override void IterReset (VirtualMachine vm)
			{
				generator = resetFunc ();
			}
		}
			
		public IterUtilsModule ()
			: base ("iterutils")
		{
			SetAttribute ("chain", new BuiltinMethodCallback (Chain, this));
			SetAttribute ("take", new BuiltinMethodCallback (Take, this));
			SetAttribute ("skip", new BuiltinMethodCallback (Skip, this));
			SetAttribute ("each", new BuiltinMethodCallback (Each, this));
			SetAttribute ("takeWhile", new BuiltinMethodCallback (TakeWhile, this));
			SetAttribute ("skipWhile", new BuiltinMethodCallback (SkipWhile, this));
		}

		private IodineObject Chain (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new InternalGenerator (() => InternalChain (vm, args));
		}

		private IodineObject Take (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			IodineInteger count = args [1] as IodineInteger;

			return new InternalGenerator (() => InternalTake (vm, args [0].GetIterator (vm), count.Value));
		}

		private IodineObject Skip (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			IodineInteger count = args [1] as IodineInteger;

			return new InternalGenerator (() => InternalSkip (vm, args [0].GetIterator (vm), count.Value));
		}

		private IodineObject TakeWhile (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			return new InternalGenerator (() => InternalTakeWhile (vm, args [0].GetIterator (vm), args [1]));
		}

		private IodineObject SkipWhile (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			return new InternalGenerator (() => InternalSkipWhile (vm, args [0], args [1]));
		}

		private IodineObject Each (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			IodineObject iter = args [0].GetIterator (vm);
			IodineObject func = args [1];
			iter.IterReset (vm);
			while (iter.IterMoveNext (vm)) {
				func.Invoke (vm, new IodineObject[] {iter.IterGetCurrent (vm)});
			}
			return null;
		}

		private static IEnumerator InternalChain (VirtualMachine vm, IodineObject[] args)
		{
			foreach (IodineObject obj in args) {
				IodineObject iterator = obj.GetIterator (vm);
				iterator.IterReset (vm);
				while (iterator.IterMoveNext (vm)) {
					yield return iterator.IterGetCurrent (vm);
				}
			}
		}

		private static IEnumerator InternalTake (VirtualMachine vm, IodineObject iterator, long count)
		{
			iterator.IterReset (vm);
			long i = 0;
			while (iterator.IterMoveNext (vm)) {
				IodineObject obj = iterator.IterGetCurrent (vm);
				if (i >= count)
					yield return obj;
				i++;
			}
		}

		private static IEnumerator InternalSkip (VirtualMachine vm, IodineObject iterator, long count)
		{
			iterator.IterReset (vm);
			long i = 0;
			while (iterator.IterMoveNext (vm)) {
				IodineObject obj = iterator.IterGetCurrent (vm);
				if (i < count)
					yield return obj;
				i++;
			}
		}

		private static IEnumerator InternalTakeWhile (VirtualMachine vm, IodineObject iterator,
			IodineObject func)
		{
			iterator.IterReset (vm);
			while (iterator.IterMoveNext (vm)) {
				IodineObject obj = iterator.IterGetCurrent (vm);
				if (func.Invoke (vm, new IodineObject[] {obj}).IsTrue ())
					yield return obj;
				else
					break;
			}
		}

		private static IEnumerator InternalSkipWhile (VirtualMachine vm, IodineObject iterator,
			IodineObject func)
		{
			iterator.IterReset (vm);
			while (iterator.IterMoveNext (vm)) {
				IodineObject obj = iterator.IterGetCurrent (vm);
				if (!func.Invoke (vm, new IodineObject[] {obj}).IsTrue ())
					yield return obj;
				else
					break;
			}
		}
	}
}

