﻿using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineStack
	{
		private Stack<StackFrame> frames = new Stack<StackFrame>( );
		private StackFrame top = null;

		public IodineMethod CurrentMethod {
			get {
				return top.Method;
			}
		}

		public IodineModule CurrentModule {
			get {
				return top.Module;
			}
		}

		public IodineObject Self {
			get {
				return this.top.Self;
			}
		}

		public IodineObject Last {
			private set; get;
		}

		public StackFrame Top {
			get {
				return top;
			}
		}

		public int Frames {
			private set;
			get;
		}

		public int InstructionPointer {
			get {
				return top.InstructionPointer;
			} set {
				top.InstructionPointer = value;
			}
		}

		public void NewFrame (StackFrame frame)
		{
			Frames++;
			top = frame;
			this.frames.Push (frame);
		}

		public void NewFrame (Location location, IodineMethod method, IodineObject self, int localCount)
		{
			Frames++;
			top = new StackFrame (location, method, top, self, localCount);
			this.frames.Push (top);
		}

		public void EndFrame ()
		{
			Frames--;
			this.frames.Pop ();
			if (this.frames.Count != 0) {
				top = this.frames.Peek ();
			} else {
				top = null;
			}
		}

		public void StoreLocal (int index, IodineObject obj)
		{
			top.StoreLocal (index, obj);
		}

		public IodineObject LoadLocal (int index)
		{
			return top.LoadLocal (index);
		}

		public void Push (IodineObject obj)
		{
			top.Push (obj);
			this.Last = obj;
		}

		public IodineObject Pop ()
		{
			return top.Pop ();
		}

		public void Unwind (int frames)
		{
			for (int i = 0; i < frames; i++) {
				StackFrame frame = this.frames.Pop ();
				frame.AbortExecution = true;
			}
			Frames -= frames;
			this.top = this.frames.Peek ();
		}
	}

	public class StackFrame
	{
		public int LocalCount {
			private set;
			get;
		}

		public bool AbortExecution {
			set;
			get;
		}
			
		public IodineMethod Method {
			private set;
			get;
		}

		public IodineModule Module {
			get {
				return Method.Module;
			}
		}

		public IodineObject Self {
			private set;
			get;
		}

		public Location Location {
			private set;
			get;
		}

		public int InstructionPointer {
			get;
			set;
		}

		public StackFrame Parent {
			private set;
			get;
		}

		private Stack<IodineObject> stack = new Stack<IodineObject> ();
		private IodineObject[] locals;

		public StackFrame (Location location, IodineMethod method, StackFrame parent, IodineObject self, int localCount)
		{
			this.LocalCount = localCount;
			this.locals = new IodineObject[localCount];
			this.Method = method;
			this.Self = self;
			this.Parent = parent;
			this.Location = location;
		}

		public StackFrame (Location location, IodineMethod method, StackFrame parent, IodineObject self, int localCount,
			IodineObject[] locals) : this (location, method, parent, self, localCount)
		{
			this.locals = locals;
		}

		public void StoreLocal (int index, IodineObject obj)
		{
			this.locals[index] = obj;
		}

		public IodineObject LoadLocal (int index)
		{
			return this.locals[index];
		}

		public void Push (IodineObject obj)
		{
			if (obj != null) 
				this.stack.Push (obj);
			else
				this.stack.Push (IodineNull.Instance);
		}

		public IodineObject Pop ()
		{
			return this.stack.Pop ();
		}

		public StackFrame Duplicate (StackFrame top)
		{
			return new StackFrame (this.Location, this.Method, top, this.Self, this.LocalCount,
				this.locals);;
		}
	}

	public class NativeStackFrame : StackFrame
	{
		public InternalMethodCallback NativeMethod
		{
			private set;
			get;
		}

		public NativeStackFrame (InternalMethodCallback method, StackFrame parent)
			: base (new Location (), null, parent, null, 0)
		{
			this.NativeMethod = method;
		}
	}
}