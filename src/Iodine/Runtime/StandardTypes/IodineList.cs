/**
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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Iodine.Compiler;

namespace Iodine.Runtime
{
    // TODO: Rewrite this
    public class IodineList : IodineObject
    {
        public static readonly IodineTypeDefinition TypeDefinition = new ListTypeDef ();

        class ListTypeDef : IodineTypeDefinition
        {
            public ListTypeDef ()
                : base ("List")
            {
                BindAttributes (this);

                SetDocumentation (
                    "A mutable sequence of objects"
                );
            }

            public override IodineObject BindAttributes (IodineObject obj)
            {
                obj.SetAttribute ("append", new BuiltinMethodCallback (Add, obj));
                obj.SetAttribute ("prepend", new BuiltinMethodCallback (Prepend, obj));
                obj.SetAttribute ("appendrange", new BuiltinMethodCallback (AddRange, obj));
                obj.SetAttribute ("discard", new BuiltinMethodCallback (Discard, obj));
                obj.SetAttribute ("remove", new BuiltinMethodCallback (Remove, obj));
                obj.SetAttribute ("removeat", new BuiltinMethodCallback (RemoveAt, obj));
                obj.SetAttribute ("contains", new BuiltinMethodCallback (Contains, obj));
                obj.SetAttribute ("clear", new BuiltinMethodCallback (Clear, obj));
                obj.SetAttribute ("index", new BuiltinMethodCallback (Index, obj));
                obj.SetAttribute ("rindex", new BuiltinMethodCallback (RightIndex, obj));
                obj.SetAttribute ("find", new BuiltinMethodCallback (Find, obj));
                obj.SetAttribute ("rfind", new BuiltinMethodCallback (RightFind, obj));
                return obj;
            }

            public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
            {
                IodineList list = new IodineList (new IodineObject[0]);
                if (args.Length > 0) {
                    foreach (IodineObject arg in args) {
                        IodineObject collection = arg.GetIterator (vm);
                        collection.IterReset (vm);
                        while (collection.IterMoveNext (vm)) {
                            IodineObject o = collection.IterGetCurrent (vm);
                            list.Add (o);
                        }
                    }
                }
                return list;
            }

            [BuiltinDocString (
                "Appends each argument to the end of the list",
                "@param *args The objects to be appended to the list"
            )]
            private IodineObject Add (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineList thisObj = self as IodineList;
                if (arguments.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }
                foreach (IodineObject obj in arguments) {
                    thisObj.Add (obj);
                }
                return thisObj;
            }

            [BuiltinDocString (
                "Iterates through the supplied arguments, adding each item to the end of the list.",
                "@param iterable The iterable object to be used."
            )]
            private IodineObject AddRange (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineList thisObj = self as IodineList;
                if (arguments.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }
                IodineObject collection = arguments [0].GetIterator (vm);
                collection.IterReset (vm);
                while (collection.IterMoveNext (vm)) {
                    IodineObject o = collection.IterGetCurrent (vm);
                    thisObj.Add (o);
                }
                return thisObj;
            }

            [BuiltinDocString (
                "Prepends an item to the beginning of the list.",
                "@param item The item to be inserted into the beginning of the list."
            )]
            private IodineObject Prepend (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineList thisObj = self as IodineList;
                if (arguments.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }
                thisObj.Objects.Insert (0, arguments [0]);
                return thisObj;
            }

            [BuiltinDocString (
                "Removes an item from the list, returning true if success, otherwise, false.",
                "@param item The item to be discarded."
            )]
            private IodineObject Discard (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineList thisObj = self as IodineList;
                if (arguments.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }

                IodineObject key = arguments [0];
                if (thisObj.Objects.Any (o => o.Equals (key))) {
                    thisObj.Objects.RemoveAt (thisObj.Objects.FindIndex (o => o.Equals (key)));
                    return IodineBool.True;
                }

                return IodineBool.False;
            }

            [BuiltinDocString (
                "Removes an item from the list, raising a KeyNotFound exception if the list does not contain [item].",
                "@param item The item to be discarded."
            )]
            private IodineObject Remove (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineList thisObj = self as IodineList;
                if (arguments.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }
                IodineObject key = arguments [0];
                if (thisObj.Objects.Any (o => o.Equals (key))) {
                    thisObj.Objects.Remove (thisObj.Objects.First (o => o.Equals (key)));
                    return this;
                }
                vm.RaiseException (new IodineKeyNotFound ());
                return null;
            }

            [BuiltinDocString (
                "Removes an item at a specified index.",
                "@param index The 0 based index of the item which is to be removed."
            )]
            private IodineObject RemoveAt (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineList thisObj = self as IodineList;
                if (arguments.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }
                IodineInteger index = arguments [0] as IodineInteger;
                if (index != null) {
                    if (index.Value < thisObj.Objects.Count) {
                        thisObj.Objects.RemoveAt ((int)index.Value);
                    } else {
                        vm.RaiseException (new IodineKeyNotFound ());
                        return null;
                    }
                    return this;
                }
                vm.RaiseException (new IodineTypeException ("Int"));
                return null;
            }

            [BuiltinDocString (
                "Returns true if the supplied argument can be fund within the list.",
                "@param item The item to test whether or not this list contains."
            )]
            private IodineObject Contains (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineList thisObj = self as IodineList;
                if (arguments.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }
                IodineObject key = arguments [0];
                bool found = false;
                foreach (IodineObject obj in thisObj.Objects) {
                    found |= obj.Equals (key);
                }

                return IodineBool.Create (found);
            }

            private IodineObject Splice (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineList thisObj = self as IodineList;
                if (arguments.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }

                int start = 0;
                int end = thisObj.Objects.Count;

                IodineInteger startInt = arguments [0] as IodineInteger;
                if (startInt == null) {
                    vm.RaiseException (new IodineTypeException ("Int"));
                    return null;
                }
                start = (int)startInt.Value;

                if (arguments.Length >= 2) {
                    IodineInteger endInt = arguments [1] as IodineInteger;
                    if (endInt == null) {
                        vm.RaiseException (new IodineTypeException ("Int"));
                        return null;
                    }
                    end = (int)endInt.Value;
                }

                if (start < 0)
                    start = thisObj.Objects.Count - start;
                if (end < 0)
                    end = thisObj.Objects.Count - end;

                IodineList retList = new IodineList (new IodineObject[]{ });

                for (int i = start; i < end; i++) {
                    if (i < 0 || i > thisObj.Objects.Count) {
                        vm.RaiseException (new IodineIndexException ());
                        return null;
                    }
                    retList.Add (thisObj.Objects [i]);
                }

                return retList;
            }

            [BuiltinDocString (
                "Clears the list, removing all items from it."
            )]
            private IodineObject Clear (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineList thisObj = self as IodineList;
                thisObj.Objects.Clear ();
                return this;
            }

            [BuiltinDocString (
                "Returns the index of the first occurance of the supplied argument, raising a KeyNotFound exception " +
                " if the supplied argument cannot be found.",
                "@param item The whose index will be returned."
            )]
            private IodineObject Index (VirtualMachine vm, IodineObject self, IodineObject[] args)
            {
                IodineList thisObj = self as IodineList;
                if (args.Length == 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }

                IodineObject item = args [0];

                if (!thisObj.Objects.Any (o => o.Equals (item))) {
                    vm.RaiseException (new IodineKeyNotFound ());
                    return null;
                }
                return new IodineInteger (thisObj.Objects.FindIndex (o => o.Equals (item)));
            }

            [BuiltinDocString (
                "Returns the index of the last occurance of the supplied argument, raising a KeyNotFound exception " +
                " if the supplied argument cannot be found.",
                "@param item The whose index will be returned."
            )]
            private IodineObject RightIndex (VirtualMachine vm, IodineObject self, IodineObject[] args)
            {
                IodineList thisObj = self as IodineList;
                if (args.Length == 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }

                IodineObject item = args [0];

                if (!thisObj.Objects.Any (o => o.Equals (item))) {
                    vm.RaiseException (new IodineKeyNotFound ());
                    return null;
                }
                return new IodineInteger (thisObj.Objects.FindLastIndex (o => o.Equals (item)));
            } 


            [BuiltinDocString (
                "Returns the index of the first occurance of the supplied argument, returning -1 " +
                " if the supplied argument cannot be found.",
                "@param item The whose index will be returned."
            )]
            private IodineObject Find (VirtualMachine vm, IodineObject self, IodineObject[] args)
            {
                IodineList thisObj = self as IodineList;
                if (args.Length == 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }

                IodineObject item = args [0];

                if (!thisObj.Objects.Any (o => o.Equals (item))) {
                    return new IodineInteger (-1);
                }
                return new IodineInteger (thisObj.Objects.FindIndex (o => o.Equals (item)));
            }

            [BuiltinDocString (
                "Returns the index of the last occurance of the supplied argument, returning -1 " +
                " if the supplied argument cannot be found.",
                "@param item The whose index will be returned."
            )]
            private IodineObject RightFind (VirtualMachine vm, IodineObject self, IodineObject[] args)
            {
                IodineList thisObj = self as IodineList;

                if (args.Length == 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }

                IodineObject item = args [0];

                if (!thisObj.Objects.Any (o => o.Equals (item))) {
                    return new IodineInteger (-1);
                }
                return new IodineInteger (thisObj.Objects.FindLastIndex (o => o.Equals (item)));
            }

        }

        class ListIterator : IodineObject
        {
            private static IodineTypeDefinition TypeDefinition = new IodineTypeDefinition ("ListIterator");

            private int iterIndex = 0;
            private List<IodineObject> objects;

            public ListIterator (List<IodineObject> objects)
                : base (TypeDefinition)
            {
                this.objects = objects;
            }

            public override IodineObject IterGetCurrent (VirtualMachine vm)
            {
                return objects [iterIndex - 1];
            }

            public override bool IterMoveNext (VirtualMachine vm)
            {
                if (iterIndex >= objects.Count) {
                    return false;
                }
                iterIndex++;
                return true;
            }

            public override void IterReset (VirtualMachine vm)
            {
                iterIndex = 0;
            }
        }

        public List<IodineObject> Objects { private set; get; }

        public IodineList (List<IodineObject> list)
            : base (TypeDefinition)
        {

            SetAttribute ("__iter__", new BuiltinMethodCallback ((VirtualMachine vm, IodineObject self, IodineObject[] args) => {
                return GetIterator (vm);
            }, this));

            Objects = list;
        }

        public IodineList (IodineObject[] items)
            : this (new List<IodineObject> (items))
        {
        }

        public override bool Equals (IodineObject obj)
        {
            IodineList listVal = obj as IodineList;

            if (listVal != null && listVal.Objects.Count == Objects.Count) {
                bool result = true;
                for (int i = 0; i < Objects.Count; i++) {
                    result &= Objects [i].Equals (listVal.Objects [i]);
                }
                return result;
            }
            return false;
        }

        public override IodineObject Len (VirtualMachine vm)
        {
            return new IodineInteger (Objects.Count);
        }

        public override IodineObject Slice (VirtualMachine vm, IodineSlice slice)
        {
            return Sublist (
                slice.Start,
                slice.Stop,
                slice.Stride,
                slice.DefaultStart,
                slice.DefaultStop
            );
        }

        private IodineList Sublist (int start, int end, int stride, bool defaultStart, bool defaultEnd)
        {
            int actualStart = start >= 0 ? start : Objects.Count - (start + 2);
            int actualEnd = end >= 0 ? end : Objects.Count - (end + 2);

            List<IodineObject> accum = new List<IodineObject> ();

            if (stride >= 0) {

                if (defaultStart) {
                    actualStart = 0;
                }

                if (defaultEnd) {
                    actualEnd = Objects.Count;
                }

                for (int i = actualStart; i < actualEnd; i += stride) {
                    accum.Add (Objects [i]);
                }
            } else {

                if (defaultStart) {
                    actualStart = Objects.Count - 1;
                }

                if (defaultEnd) {
                    actualEnd = 0;
                }

                for (int i = actualStart; i >= actualEnd; i += stride) {
                    accum.Add (Objects [i]);
                }
            }

            return new IodineList (accum);
        }

        public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
        {
            IodineInteger index = key as IodineInteger;
            if (index == null) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return null;
            }

            if (index.Value < Objects.Count)
                return Objects [(int)index.Value];
            vm.RaiseException (new IodineIndexException ());
            return null;
        }

        public override void SetIndex (VirtualMachine vm, IodineObject key, IodineObject value)
        {
            IodineInteger index = key as IodineInteger;
            if (index == null) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return;
            }

            if (index.Value < Objects.Count)
                this.Objects [(int)index.Value] = value;
            else
                vm.RaiseException (new IodineIndexException ());
        }

        public override IodineObject Add (VirtualMachine vm, IodineObject right)
        {
            IodineList list = new IodineList (Objects.ToArray ());
            right.IterReset (vm);
            while (right.IterMoveNext (vm)) {
                IodineObject o = right.IterGetCurrent (vm);
                list.Add (o);
            }
            return list;
        }

        public override IodineObject Equals (VirtualMachine vm, IodineObject right)
        {
            return IodineBool.Create (Equals (right));
        }

        public override IodineObject GetIterator (VirtualMachine vm)
        {
            return new ListIterator (Objects);
        }

        public override IodineObject Represent (VirtualMachine vm)
        {
            string repr = String.Join (", ", Objects.Select (p => p.Represent (vm).ToString ()));
            return new IodineString (String.Format ("[{0}]", repr));
        }

        public void Add (IodineObject obj)
        {
            Objects.Add (obj);
        }

        private bool Compare (IodineList list1, IodineList list2)
        {
            if (list1.Objects.Count != list2.Objects.Count) {
                return false;
            }

            for (int i = 0; i < list1.Objects.Count; i++) {
                if (list1.Objects [i].GetHashCode () != list2.Objects [i].GetHashCode ()) {
                    return false;
                }
            }
            return true;
        }

        public override bool IsTrue ()
        {
            return Objects.Count > 0;
        }

        public override int GetHashCode ()
        {
            int accum = 17;
            unchecked {
                foreach (IodineObject obj in Objects) {
                    if (obj != null) {
                        accum += 529 * obj.GetHashCode ();
                    }
                }
            }
            return accum;
        }
    }
}
