﻿using Symbioz.Core.Pool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSync.Sockets
{
    public class BufferManager //: IDisposable
    {

        public static readonly List<BufferManager> Managers = new List<BufferManager>();

        /// <summary>
        /// Default BufferManager for small buffers with up to 128 bytes length
        /// </summary>
        public static readonly BufferManager Tiny = new BufferManager(1024, 128);

        /// <summary>
        /// Default BufferManager for small buffers with up to 1kb length
        /// </summary>
        public static readonly BufferManager Small = new BufferManager(1024, 1024);

        /// <summary>
        /// Default BufferManager for default-sized buffers (usually up to 8kb)
        /// </summary>
        public static readonly BufferManager Default = new BufferManager(512, 8192); //8192

        /// <summary>
        /// Large BufferManager for buffers up to 64kb size
        /// </summary>
        public static readonly BufferManager Large = new BufferManager(128, 64 * 1024);

        /// <summary>
        /// Extra Large BufferManager holding 512kb buffers
        /// </summary>
        public static readonly BufferManager ExtraLarge = new BufferManager(32, 512 * 1024);

        /// <summary>
        /// Super Large BufferManager holding 1MB buffers
        /// </summary>
        public static readonly BufferManager SuperSized = new BufferManager(16, 1024 * 1024);

        /// <summary>
        /// Holds the total amount of memory allocated by all buffer managers.
        /// </summary>
        public static long GlobalAllocatedMemory = 0;

        /// <summary>
        /// Count of segments per Buffer
        /// </summary>
        private readonly int _segmentCount;
        /// <summary>
        /// Segment size
        /// </summary>_buffers
        private readonly int _segmentSize;
        /// <summary>
        /// Total count of segments in all buffers
        /// </summary>
        private int _totalSegmentCount;
        private volatile static int _segmentId;

        private readonly List<ArrayBuffer> _buffers;
        private readonly LockfreeQueue<BufferSegment> _availableSegments;

        /// <summary>
        /// The number of currently available segments
        /// </summary>
        public int AvailableSegmentsCount
        {
            get { return _availableSegments.Count; } //do we really care about volatility here?
        }

        public bool InUse
        {
            get { return _availableSegments.Count < _totalSegmentCount; }
        }

        public int UsedSegmentCount
        {
            get { return _totalSegmentCount - _availableSegments.Count; }
        }

        /// <summary>
        /// The total number of currently allocated buffers.
        /// </summary>
        public int TotalBufferCount
        {
            get { return _buffers.Count; } //do we really care about volatility here?
        }

        /// <summary>
        /// The total number of currently allocated segments.
        /// </summary>
        public int TotalSegmentCount
        {
            get { return _totalSegmentCount; } //do we really care about volatility here?
        }

        /// <summary>
        /// The total amount of all currently allocated buffers.
        /// </summary>
        public int TotalAllocatedMemory
        {
            get { return _buffers.Count * (_segmentCount * _segmentSize); } // do we really care about volatility here?
        }

        /// <summary>
        /// The size of a single segment
        /// </summary>
        public int SegmentSize
        {
            get { return _segmentSize; }
        }

        #region Constructors

        /// <summary>
        /// Constructs a new <see cref="Default"></see> object
        /// </summary>
        /// <param name="segmentCount">The number of chunks tocreate per segment</param>
        /// <param name="segmentSize">The size of a chunk in bytes</param>
        public BufferManager(int segmentCount, int segmentSize)
        {
            _segmentCount = segmentCount;
            _segmentSize = segmentSize;
            _buffers = new List<ArrayBuffer>();
            _availableSegments = new LockfreeQueue<BufferSegment>();
            Managers.Add(this);
        }

        #endregion

        /// <summary>
        /// Checks out a segment, creating more if the pool is empty.
        /// </summary>
        /// <returns>a BufferSegment object from the pool</returns>
        public BufferSegment CheckOut()
        {
            BufferSegment segment;

            if (!_availableSegments.TryDequeue(out segment))
            {
                lock (_buffers)
                {
                    while (!_availableSegments.TryDequeue(out segment))
                    {
                        CreateBuffer();
                    }
                }
            }

            // this doubles up with what CheckIn() looks for, but no harm in that, really.
            if (segment.m_uses > 1)
            {

            }

            // set initial usage to 1
            segment.m_uses = 1;

            return segment;
        }

        /// <summary>
        /// Checks out a segment, and wraps it with a SegmentStream, creating more if the pool is empty.
        /// </summary>
        /// <returns>a SegmentStream object wrapping the BufferSegment taken from the pool</returns>
        public SegmentStream CheckOutStream()
        {
            return new SegmentStream(CheckOut());
        }

        /// <summary>
        /// Requeues a segment into the buffer pool.
        /// </summary>
        /// <param name="segment">the segment to requeue</param>
        public void CheckIn(BufferSegment segment)
        {
            if (segment.m_uses > 1)
            {

            }

            _availableSegments.Enqueue(segment);
        }

        /// <summary>
        /// Creates a new buffer and adds the segments to the buffer pool.
        /// </summary>
        private void CreateBuffer()
        {
            // create a new buffer 
            var newBuf = new ArrayBuffer(this, _segmentCount * _segmentSize);

            // create segments from the buffer
            for (int i = 0; i < _segmentCount; i++)
            {
                _availableSegments.Enqueue(new BufferSegment(newBuf, i * _segmentSize, _segmentSize, _segmentId++));
            }

            // increment our total count
            _totalSegmentCount += _segmentCount;

            // hold a ref to our new buffer
            _buffers.Add(newBuf);

            // update global alloc'd memory
            Interlocked.Add(ref GlobalAllocatedMemory, _segmentCount * _segmentSize);
        }

        /// <summary>
        /// Returns a BufferSegment that is at least of the given size.
        /// </summary>
        /// <param name="payloadSize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">In case that the payload exceeds the SegmentSize of the largest buffer available.</exception>
        public static BufferSegment GetSegment(int payloadSize)
        {
            if (payloadSize <= Tiny.SegmentSize)
            {
                return Tiny.CheckOut();
            }
            if (payloadSize <= Small.SegmentSize)
            {
                return Small.CheckOut();
            }
            if (payloadSize <= Default.SegmentSize)
            {
                return Default.CheckOut();
            }
            if (payloadSize <= Large.SegmentSize)
            {
                return Large.CheckOut();
            }
            if (payloadSize <= ExtraLarge.SegmentSize)
            {
                return ExtraLarge.CheckOut();
            }

            throw new ArgumentOutOfRangeException("Required buffer is way too big: " + payloadSize);
        }

        /// <summary>
        /// Returns a SegmentStream that is at least of the given size.
        /// </summary>
        /// <param name="payloadSize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">In case that the payload exceeds the SegmentSize of the largest buffer available.</exception>
        public static SegmentStream GetSegmentStream(int payloadSize)
        {
            return new SegmentStream(GetSegment(payloadSize));
        }

        #region IDisposable Members

        ~BufferManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // clear the segment queue
            BufferSegment segment;
            while (_availableSegments.TryDequeue(out segment)) ;

            // clean up buffers

            _buffers.Clear();
        }

        #endregion
    }

}
