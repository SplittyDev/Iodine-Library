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
using System.Security.Cryptography;

namespace Iodine.Runtime
{
    [BuiltinDocString (
        "Provides functions for generating random values."
    )]
    [IodineBuiltinModule ("random")]
    public class RandomModule : IodineModule
    {
        private static Random rgn = new Random ();
        private static RNGCryptoServiceProvider secureRand = new RNGCryptoServiceProvider ();

        public RandomModule ()
            : base ("random")
        {
            SetAttribute ("rand", new BuiltinMethodCallback (Rand, this));
            SetAttribute ("randint", new BuiltinMethodCallback (RandInt, this));
            SetAttribute ("choose", new BuiltinMethodCallback (Choice, this));
            SetAttribute ("cryptostr", new BuiltinMethodCallback (CryptoString, this));
            //SetAttribute ("urandom", new BuiltinMethodCallback (CryptoString, this));
        }

        [BuiltinDocString (
            "Returns a random floating point number between 0 and 1."
        )]
        private IodineObject Rand (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            return new IodineFloat (rgn.NextDouble ());
        }

        [BuiltinDocString (
            "Returns a random integer between 0 and [a], or between [a] and [b] (if [b] is supplied).",
            "@param a The starting value (Or max value if b is not supplied)",
            "@param b The upper limit"
        )]
        private IodineObject RandInt (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                return new IodineInteger (rgn.Next (Int32.MinValue, Int32.MaxValue));
            } else {
                int start = 0;
                int end = 0;
                if (args.Length <= 1) {
                    IodineInteger integer = args [0] as IodineInteger;
                    if (integer == null) {
                        vm.RaiseException (new IodineTypeException ("Int"));
                        return null;
                    }
                    end = (int)integer.Value;
                } else {
                    IodineInteger startInteger = args [0] as IodineInteger;
                    IodineInteger endInteger = args [1] as IodineInteger;
                    if (startInteger == null || endInteger == null) {
                        vm.RaiseException (new IodineTypeException ("Int"));
                        return null;
                    }
                    start = (int)startInteger.Value;
                    end = (int)endInteger.Value;
                }
                return new IodineInteger (rgn.Next (start, end));
            }
        }

        /**
         * Iodine Function: cryptoString (size)
         */
        private IodineObject CryptoString (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
                
            IodineInteger count = args [0] as IodineInteger;

            if (count == null) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return null;
            }

            byte[] buf = new byte[(int)count.Value];
            secureRand.GetBytes (buf);
            return new IodineString (Convert.ToBase64String (buf).Substring (0, (int)count.Value));
        }


        [BuiltinDocString (
            "Chooses a random item in an iterable sequence.",
            "@param iterable The iterable to choose from"
        )]
        private IodineObject Choice (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineObject collection = args [0].GetIterator (vm);
            int count = 0;
            collection.IterReset (vm);

            while (collection.IterMoveNext (vm)) {
                collection.IterGetCurrent (vm);
                count++;
            }

            int choice = rgn.Next (0, count);
            count = 0;

            collection.IterReset (vm);
            while (collection.IterMoveNext (vm)) {
                IodineObject o = collection.IterGetCurrent (vm);
                if (count == choice)
                    return o;
                count++;
            }

            return null;
        }
    }
}

