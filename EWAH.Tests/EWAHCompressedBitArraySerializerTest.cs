using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Ewah
{
	
/*
 * Copyright 2012, Kemal Erdogan, Daniel Lemire and Ciaran Jessup
 * Licensed under APL 2.0.
 */
    [TestFixture]
    public class EWAHCompressedBitArraySerializerTest
    {

        /// <summary>
        /// Tests the custom serialization strategy.
        /// </summary>
        [Test]
        public void TestCustomSerializationStrategy() {
            Console.WriteLine("testing Custom serialization strategy");

            // Create a compressed bit array, and randomly assign up to 20,000 bits to it.
            var bmp = new EwahCompressedBitArray();
            var r= new Random();
            for (int i = 0; i < 23000; i++) {
                if (r.NextDouble() < 0.5) {
                    bmp.Set(i);
                }
            }
            
            byte[] originalDeserialized= null;
            byte[] newFormDeserialized= null;
            EwahCompressedBitArray newFormReserialized= null;
            EwahCompressedBitArray originalReserialized= null;

            // First de-serialize+ re-serialize 'normally' 
            using (var ms = new MemoryStream()) {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, bmp);
                originalDeserialized = ms.ToArray();
                ms.Seek(0, SeekOrigin.Begin);
                originalReserialized = (EwahCompressedBitArray)bf.Deserialize(ms);
            }

            // Now de-serialize + re-serialize with the new form.
            using (var ms = new MemoryStream()) {
                EwahCompressedBitArraySerializer bf = new EwahCompressedBitArraySerializer();
                bf.Serialize(ms, bmp);
                newFormDeserialized = ms.ToArray();
                ms.Seek(0, SeekOrigin.Begin);
                newFormReserialized = (EwahCompressedBitArray)bf.Deserialize(ms);
            }

            // Assert that the new form is more compact than the original form.
            Assert.Less(newFormDeserialized.Length, originalDeserialized.Length);

            // Compare the 'normal' de-serialized + re-serialized form, against the original.
            Assert.AreEqual(bmp, originalReserialized);

            // Compare the 'new form' de-serialized + re-serialized form, against the original.
            Assert.AreEqual(bmp, newFormReserialized);

            // Compare the 'normal' de-serialized + re-serialized form, against the newly de-serialized + re-serialized form.
            Assert.AreEqual(newFormReserialized, originalReserialized);

            Console.WriteLine("testing Custom serialization strategy:ok");
        }
    }
}
