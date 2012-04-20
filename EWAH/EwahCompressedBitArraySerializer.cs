using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ewah
{
    /*
     * Copyright 2012, Kemal Erdogan, Daniel Lemire and Ciaran Jessup
     * Licensed under APL 2.0.
     */
    /// <summary>
    /// A very simple Serialization schema for serialising and de-serialising instance of <see cref="Ewah.EwahCompressedBitArray"/>. 
    /// 
    /// The current implementation lacks serialization version information, and type information, it is rigid, brittle, simplistic but
    /// results in less byte bloat than a traditional BinaryFormatter.
    /// 
    /// Consists of a very simple, fixed width header, and abritrary length buffer.
    ///
    /// Bytes 1-4    : 'SizeInBits'
    /// Bytes 5-8    : 'ActualSizeInWords'
    /// Bytes 9-12   : 'RunningLengthWordPosition'
    /// Bytes 13-End : Contents of the internal long[] buffer
    /// 
    /// The encoding scheme is that of Microsoft's Built in System.BitConverter methods. Unfortunately the endian-ness of these calls
    /// is architecture specific, so beware that to be certain of the endian order on the machine serializing this class one must use the 
    /// BitConverter.IsLittleEndian property.
    /// </summary>
    public class EwahCompressedBitArraySerializer
    {
        /// <summary>
        /// Deserializes the specified stream into an instance of <see cref="Ewah.EwahCompressedBitArray"/>
        /// </summary>
        /// <param name="serializationStream">The stream containing the data that constructs a valid instance of EwahCompressedBitArray.</param>
        /// <returns></returns>
        public EwahCompressedBitArray Deserialize(Stream serializationStream) {
            byte[] buff= new byte[8];
            serializationStream.Read(buff, 0, 4);
            int sizeInBits = BitConverter.ToInt32(buff, 0);
            serializationStream.Read(buff, 0, 4);
            int actualSizeInWords = BitConverter.ToInt32(buff, 0);
            serializationStream.Read(buff, 0, 4);
            int runningLengthWordPosition = BitConverter.ToInt32(buff, 0);
            long[] buffer = new long[actualSizeInWords];
            for (int i = 0; i < actualSizeInWords; i++) {
                serializationStream.Read(buff, 0, 8);
                buffer[i] = BitConverter.ToInt64(buff, 0);
            }
            return new EwahCompressedBitArray(sizeInBits, actualSizeInWords, buffer, runningLengthWordPosition);
        }

        /// <summary>
        /// Serializes an instance of <see cref="Ewah.EwahCompressedBitArray"/> into the given stream
        /// </summary>
        /// <param name="serializationStream">The serialization stream.</param>
        /// <param name="bitArray">The bit array.</param>
        public void Serialize(Stream serializationStream, EwahCompressedBitArray bitArray) {
            // No actual need to call Shrink with this serialisation strategy, so we can avoid
            // mutating the source type (side-effects are bad ;) )
            serializationStream.Write( BitConverter.GetBytes(bitArray.SizeInBits), 0, 4 );
            serializationStream.Write( BitConverter.GetBytes(bitArray._ActualSizeInWords),0, 4 );
            serializationStream.Write(BitConverter.GetBytes(bitArray._Rlw.Position), 0, 4);
            for(int i=0; i< bitArray._ActualSizeInWords;i++) {
                serializationStream.Write(BitConverter.GetBytes(bitArray._Buffer[i]), 0, 8);
            }
            return;
        }
    }
}
