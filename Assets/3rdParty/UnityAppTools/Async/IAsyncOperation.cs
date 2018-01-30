﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityAppTools
{
	/// <summary>
	/// A yieldable asynchronous operation with status info.
	/// </summary>
	/// <seealso cref="IAsyncOperation{T}"/>
	public interface IAsyncOperation
	{
		/// <summary>
		/// Returns an <see cref="System.Exception"/> that caused the operation to end prematurely. If the operation completed successfully
		/// or has not yet thrown any exceptions, this will return <see langword="null"/>. Read only.
		/// </summary>
		/// <value>An exception that caused the operation to end prematurely.</value>
		/// <seealso cref="IsFaulted"/>
		Exception Exception { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the operation has completed successfully, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <value>A value indicating whether the operation has finished successfully.</value>
		/// <seealso cref="IsCompleted"/>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="IsCanceled"/>
		bool IsCompletedSuccessfully { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the operation has completed (either successfully or not), <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <value>A value indicating whether the operation has completed.</value>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="IsCanceled"/>
		bool IsCompleted { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the operation has failed for any reason, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <value>A value indicating whether the operation has failed.</value>
		/// <seealso cref="Exception"/>
		/// <seealso cref="IsCompleted"/>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsCanceled"/>
		bool IsFaulted { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the operation has been canceled by user, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <value>A value indicating whether the operation has been canceled by user.</value>
		/// <seealso cref="IsCompleted"/>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsFaulted"/>
		bool IsCanceled { get; }
	}
}
