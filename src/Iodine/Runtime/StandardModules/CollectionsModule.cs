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
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Iodine.Runtime
{
	[IodineBuiltinModule ("collections")]
	public class CollectionsModule : IodineModule
	{
		class IodineArray : IodineObject
		{
			public static readonly IodineTypeDefinition TypeDefinition = new ArrayTypeDefinition ();

			class ArrayTypeDefinition : IodineTypeDefinition
			{
				public ArrayTypeDefinition ()
					: base ("Array")
				{
				}

				public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
				{
					if (arguments.Length == 0) {
						vm.RaiseException (new IodineArgumentException (1));
						return null;
					}

					IodineInteger intobj = arguments [0] as IodineInteger;

					if (intobj == null) {
						vm.RaiseException (new IodineTypeException ("Int"));
						return null;
					}

					return new IodineArray ((int)intobj.Value);
				}
			}

			private IodineObject[] data;
			private int iterIndex = 0;

			private IodineArray ()
				: base (TypeDefinition)
			{
			}

			public IodineArray (int size)
				: this ()
			{
				data = new IodineObject[size];
			}

			public override IodineObject Len (VirtualMachine vm)
			{
				return new IodineInteger (data.Length);
			}

			public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
			{
				IodineInteger index = key as IodineInteger;

				int position = (int)index.Value;

				if (index == null) {
					vm.RaiseException (new IodineTypeException ("Int"));
					return null;
				}

				if (position > data.Length) {
					vm.RaiseException (new IodineIndexException ());
					return null;
				}

				return data [position];
			}

			public override void SetIndex (VirtualMachine vm, IodineObject key, IodineObject value)
			{
				IodineInteger index = key as IodineInteger;

				int position = (int)index.Value;

				if (index == null) {
					vm.RaiseException (new IodineTypeException ("Int"));
					return;
				}

				if (position > data.Length) {
					vm.RaiseException (new IodineIndexException ());
					return;
				}

				data [position] = value;
			}
		}

		class IodineTypedList : IodineObject
		{
			static IodineTypeDefinition TypeDefinition  = new TypedListTypeDefinition ();

			class TypedListTypeDefinition : IodineTypeDefinition
			{
				public TypedListTypeDefinition ()
					: base ("TypedList")
				{
				}

				public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
				{
					if (arguments.Length == 0) {
						vm.RaiseException (new IodineArgumentException (1));
						return null;
					}

					IodineTypeDefinition typedef = arguments [0] as IodineTypeDefinition;

					if (typedef == null) {
						vm.RaiseException (new IodineTypeException ("TypeDef"));
						return null;
					}

					return new IodineTypedList (typedef);
				}
			}

			private List<IodineObject> list = new List<IodineObject> ();

			private IodineTypeDefinition listType;

			public IodineTypedList (IodineTypeDefinition type)
				: base (TypeDefinition)
			{
				listType = type;
				SetAttribute ("add", new InternalMethodCallback (add, this));
			}

			private IodineObject add (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				foreach (IodineObject obj in args) {
					if (!obj.InstanceOf (listType)) {
						vm.RaiseException (new IodineTypeException (listType.Name));
						return null;
					}
					list.Add (obj);
				}
				return null;
			}
		}

		class IodineStack : IodineObject
		{
			public static readonly IodineTypeDefinition TypeDefinition = new StackTypeDefinition ();

			class StackTypeDefinition : IodineTypeDefinition
			{
				public StackTypeDefinition ()
					: base ("Stack")
				{
					
				}

				public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
				{
					return base.Invoke (vm, arguments);
				}
			}

			public readonly LinkedStack<IodineObject> Stack = new LinkedStack<IodineObject> ();

			public IodineStack ()
				: base (TypeDefinition)
			{
				SetAttribute ("push", new InternalMethodCallback (push, this));
				SetAttribute ("pop", new InternalMethodCallback (pop, this));
				SetAttribute ("peek", new InternalMethodCallback (peek, this));
				SetAttribute ("isEmpty", new InternalMethodCallback (isEmpty, this));
			}

			private IodineObject push (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				foreach (IodineObject obj in args) {
					Stack.Push (obj);
				}
				return null;
			}

			private IodineObject pop (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				return Stack.Pop ();
			}

			private IodineObject peek (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				return Stack.Peek ();
			}

			private IodineObject isEmpty (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				return IodineBool.Create (Stack.Count == 0);
			}
		}

		public CollectionsModule ()
			: base ("collections")
		{
			SetAttribute ("List", IodineList.TypeDefinition);
			SetAttribute ("TypedList", IodineList.TypeDefinition);
			SetAttribute ("HashMap", IodineHashMap.TypeDefinition);
			SetAttribute ("Stack", IodineStack.TypeDefinition);
			SetAttribute ("Array", IodineArray.TypeDefinition);
		}

	}
}

