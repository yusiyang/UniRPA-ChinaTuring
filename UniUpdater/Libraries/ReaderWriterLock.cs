using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UniUpdater.Libraries
{
	public sealed class ReaderWriterLock
	{
		private ReaderWriterLockSlim m_lock;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="T:log4net.Util.ReaderWriterLock" /> class.
		/// </para>
		/// </remarks>
		public ReaderWriterLock()
		{
			m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		}

		/// <summary>
		/// Acquires a reader lock
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="M:log4net.Util.ReaderWriterLock.AcquireReaderLock" /> blocks if a different thread has the writer 
		/// lock, or if at least one thread is waiting for the writer lock.
		/// </para>
		/// </remarks>
		public void AcquireReaderLock()
		{
			try
			{
			}
			finally
			{
				m_lock.EnterReadLock();
			}
		}

		/// <summary>
		/// Decrements the lock count
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="M:log4net.Util.ReaderWriterLock.ReleaseReaderLock" /> decrements the lock count. When the count 
		/// reaches zero, the lock is released.
		/// </para>
		/// </remarks>
		public void ReleaseReaderLock()
		{
			m_lock.ExitReadLock();
		}

		/// <summary>
		/// Acquires the writer lock
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method blocks if another thread has a reader lock or writer lock.
		/// </para>
		/// </remarks>
		public void AcquireWriterLock()
		{
			try
			{
			}
			finally
			{
				m_lock.EnterWriteLock();
			}
		}

		/// <summary>
		/// Decrements the lock count on the writer lock
		/// </summary>
		/// <remarks>
		/// <para>
		/// ReleaseWriterLock decrements the writer lock count. 
		/// When the count reaches zero, the writer lock is released.
		/// </para>
		/// </remarks>
		public void ReleaseWriterLock()
		{
			m_lock.ExitWriteLock();
		}
	}
}
