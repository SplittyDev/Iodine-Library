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

namespace Iodine
{
	/// <summary>
	/// Represents a location inside an Iodine source file
	/// </summary>
	public class Location
	{
		/// <summary>
		/// Gets or sets the line.
		/// </summary>
		/// <value>The line.</value>
		public int Line {
			set;
			get;
		}

		/// <summary>
		/// Gets or sets the column.
		/// </summary>
		/// <value>The column.</value>
		public int Column {
			set;
			get;
		}

		/// <summary>
		/// Gets or sets the file.
		/// </summary>
		/// <value>The file.</value>
		public string File {
			set;
			get;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Iodine.Location"/> struct.
		/// </summary>
		/// <param name="line">Line.</param>
		/// <param name="column">Column.</param>
		/// <param name="file">File.</param>
		public Location (int line, int column, string file)
		{
			Line = line;
			Column = column;
			File = file;
		}

		/// <summary>
		/// Increments the line.
		/// </summary>
		/// <returns>The line.</returns>
		public Location IncrementLine ()
		{
			return new Location (Line + 1, Column, File);
		}

		/// <summary>
		/// Increments the column.
		/// </summary>
		/// <returns>The column.</returns>
		public Location IncrementColumn ()
		{
			return new Location (Line, Column + 1, File);
		}
	}
}

