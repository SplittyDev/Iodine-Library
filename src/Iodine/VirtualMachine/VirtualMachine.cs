﻿using System;
using System.Collections.Generic;

namespace Iodine
{
	public class VirtualMachine
	{
		private Dictionary<string, IodineObject> globalDict = new Dictionary<string, IodineObject> ();
		private Stack<IodineExceptionHandler> exceptionHandlers = new Stack<IodineExceptionHandler> ();
		private IodineException lastException = null;

		public IodineStack Stack
		{
			private set;
			get;
		}

		public VirtualMachine ()
		{
			this.Stack = new IodineStack ();
			LoadExtension (new BuiltinFunctions ());
		}
			

		public IodineObject InvokeMethod (IodineMethod method, IodineObject self, IodineObject[] arguments)
		{
			Stack.NewFrame (method, self, method.LocalCount);
			int insCount = method.Body.Count;
	
			int i = 0;
			foreach (string param in method.Parameters.Keys) {
				Stack.StoreLocal (method.Parameters[param], arguments[i++]);
			}

			StackFrame top = Stack.Top;
			while (top.InstructionPointer < insCount && !top.AbortExecution) {
				Instruction currInstruction = method.Body[Stack.InstructionPointer++];
				ExecuteInstruction (currInstruction);
			}

			if (top.AbortExecution) {
				return null;
			}

			IodineObject retVal = Stack.Pop ();
			Stack.EndFrame ();
			return retVal;
		}

		public IodineObject InvokeMethod (IodineMethod method, StackFrame frame, IodineObject self,
			IodineObject[] arguments)
		{
			Stack.NewFrame (frame);
			int insCount = method.Body.Count;

			int i = 0;
			foreach (string param in method.Parameters.Keys) {
				Stack.StoreLocal (method.Parameters[param], arguments[i++]);
			}

			StackFrame top = Stack.Top;
			while (top.InstructionPointer < insCount && !top.AbortExecution) {
				Instruction currInstruction = method.Body[Stack.InstructionPointer++];
				ExecuteInstruction (currInstruction);
			}

			if (top.AbortExecution) {
				return null;
			}

			IodineObject retVal = Stack.Pop ();
			Stack.EndFrame ();
			return retVal;
		}

		public void LoadExtension (IIodineExtension extension)
		{
			extension.Initialize (globalDict);
		}

		public void RaiseException (string message, params object[] args)
		{
			IodineException ex = new IodineException (message, args);
			if (exceptionHandlers.Count == 0) {
				throw new UnhandledIodineExceptionException (ex);
			} else {
				IodineExceptionHandler handler = exceptionHandlers.Pop ();
				Stack.Unwind (Stack.Frames - handler.Frame);
				lastException = ex;
				Stack.InstructionPointer = handler.InstructionPointer;
			}
		}

		private void ExecuteInstruction (Instruction ins)
		{
			switch (ins.OperationCode) {
			case Opcode.Pop: {
					Stack.Pop ();
					break;
				}
			case Opcode.Dup: {
					IodineObject val = Stack.Pop ();
					Stack.Push (val);
					Stack.Push (val);
					break;
				}
			case Opcode.Dup3: {
					IodineObject val = Stack.Pop ();
					Stack.Push (val);
					Stack.Push (val);
					Stack.Push (val);
					break;
				}
			case Opcode.LoadConst: {
					Stack.Push (Stack.CurrentModule.ConstantPool[ins.Argument]);
					break;
				}
			case Opcode.LoadNull: {
					Stack.Push (null);
					break;
				}
			case Opcode.LoadSelf: {
					Stack.Push (Stack.Self);
					break;
				}
			case Opcode.LoadTrue: {
					Stack.Push (IodineBool.True);
					break;
				}
			case Opcode.LoadException: {
					Stack.Push (lastException);
					break;
				}
			case Opcode.LoadFalse: {
					Stack.Push (IodineBool.False);
					break;
				}
			case Opcode.StoreLocal: {
					Stack.StoreLocal (ins.Argument, Stack.Pop ());
					break;
				}
			case Opcode.LoadLocal: {
					Stack.Push (Stack.LoadLocal (ins.Argument));
					break;
				}
			case Opcode.StoreGlobal: {
					string name = ((IodineName)Stack.CurrentModule.ConstantPool[ins.Argument]).Value;
					if (Stack.CurrentModule.HasAttribute (name)) {
						Stack.CurrentModule.SetAttribute (name, Stack.Pop ());
					} else {
						globalDict[name] = Stack.Pop ();
					}
					break;
				}
			case Opcode.LoadGlobal: {
					string name = ((IodineName)Stack.CurrentModule.ConstantPool[ins.Argument]).Value;
					if (globalDict.ContainsKey (name)) {
						Stack.Push (globalDict[name]);
					} else {
						Stack.Push (Stack.CurrentModule.GetAttribute (name));
					}
					break;
				}
			case Opcode.StoreAttribute: {
					IodineObject target = Stack.Pop ();
					IodineObject value = Stack.Pop ();
					string attribute = ((IodineName)Stack.CurrentModule.ConstantPool[ins.Argument]).Value;
					target.SetAttribute (attribute, value);
					break;
				}
			case Opcode.LoadAttribute: {
					IodineObject target = Stack.Pop ();
					string attribute = ((IodineName)Stack.CurrentModule.ConstantPool[ins.Argument]).Value;
					if (target.HasAttribute (attribute))
						Stack.Push (target.GetAttribute (attribute));
					else
						RaiseException ("Could not find attribute '{0}'", attribute);
					break;
				}
			case Opcode.StoreIndex: {
					IodineObject index = Stack.Pop ();
					IodineObject target = Stack.Pop ();
					IodineObject value = Stack.Pop ();
					target.SetIndex (index, value);
					break;
				}
			case Opcode.LoadIndex: {
					IodineObject index = Stack.Pop ();
					IodineObject target = Stack.Pop ();
					Stack.Push (target.GetIndex (index));
					break;
				}
			case Opcode.BinOp: {
					Stack.Push (Stack.Pop ().PerformBinaryOperation (this, (BinaryOperation)ins.Argument,
						Stack.Pop ()));
					break;
				}
			case Opcode.UnaryOp: {
					Stack.Pop ().PerformUnaryOperation (this, (UnaryOperation)ins.Argument);
					break;
				}
			case Opcode.Invoke: {
					IodineObject target = Stack.Pop ();
					IodineObject[] arguments = new IodineObject[ins.Argument];
					for (int i = 1; i <= ins.Argument; i++ ){
						arguments[ins.Argument - i] = Stack.Pop ();
					}
					Stack.Push (target.Invoke (this, arguments));
					break;
			}
			case Opcode.Return: {
					this.Stack.InstructionPointer = int.MaxValue;
					break;
				}
			case Opcode.JumpIfTrue: {
					if (Stack.Pop ().IsTrue ()) {
						Stack.InstructionPointer = ins.Argument;
					}
					break;
				}
			case Opcode.JumpIfFalse: {
					if (!Stack.Pop ().IsTrue ()) {
						Stack.InstructionPointer = ins.Argument;
					}
					break;
				}
			case Opcode.Jump: {
					Stack.InstructionPointer = ins.Argument;
					break;
				}
			case Opcode.Print: {
					Console.WriteLine (Stack.Pop ().ToString ());
					Stack.Push (null);
					break;
				}
			case Opcode.BuildList: {
					IodineObject[] items = new IodineObject[ins.Argument];
					for (int i = 1; i <= ins.Argument; i++ ){
						items[ins.Argument - i] = Stack.Pop ();
					}
					Stack.Push (new IodineList (items));
					break;
				}
			case Opcode.BuildClosure: {
					IodineMethod method = Stack.Pop () as IodineMethod;
					Stack.Push (new IodineClosure (Stack.Top, method));
					break;
				}
			case Opcode.IterGetNext: {
					Stack.Push (Stack.Pop ().IterGetNext (this));
					break;
				}
			case Opcode.IterMoveNext: {
					Stack.Push (new IodineBool (Stack.Pop ().IterMoveNext (this)));
					break;
				}
			case Opcode.IterReset: {
					Stack.Pop ().IterReset (this);
					break;
				}
			case Opcode.PushExceptionHandler: {
					exceptionHandlers.Push (new IodineExceptionHandler (Stack.Frames, ins.Argument));
					break;
				}
			case Opcode.PopExceptionHandler: {
					exceptionHandlers.Pop ();
					break;
				}
			}
		}
	}
}

