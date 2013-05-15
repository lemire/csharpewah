using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace Ewah
{
/*
 * Copyright 2012, Kemal Erdogan, Daniel Lemire and Ciaran Jessup
 * Licensed under APL 2.0.
 */
    public static class Ext
    {
        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> newElems)
        {
            foreach (T elem in newElems)
            {
                set.Add(elem);
            }
        }

        public static int NextSetBit(this BitArray bitArray, int startPos)
        {
            for (int ii = startPos; ii < bitArray.Length; ii++)
            {
                if (bitArray[ii])
                    return ii;
            }
            return -1;
        }

        public static int Cardinality(this BitArray bitArray)
        {
            int res = 0;
            for (int ii = 0; ii < bitArray.Length; ii++)
            {
                if (bitArray.Get((ii)))
                    res++;
            }
            return res;
        }

        public static int AndNot(this BitArray bitArray, BitArray other)
        {
            int res = Math.Min(bitArray.Count, other.Count);
            for (int ii = 0; ii < res; ii++)
            {
                bitArray.Set(ii, bitArray[ii] && !other[ii]);
            }
            return res;
        }

        public static void Retain<T>(this List<T> list, List<T> retain)
        {
            int ii = 0;
            while (ii < list.Count)
            {
                if (retain.Contains(list[ii]))
                {
                    ii++;
                }
                else
                {
                    list.RemoveAt(ii);
                }
            }
        }
    }

/**
 * This class is used for unit testing.
 */


    [TestFixture]
    public sealed class EwahCompressedBitArrayTest
    {
        /** The Constant MEGA: a large integer. */
        private const int Mega = 8*1024*1024;

        /** The Constant TEST_BS_SIZE: used to represent the size of a large bitmap. */
        private const int TestBsSize = 8*Mega;

  /**
   * Function used in a test inspired by Federico Fissore.
   *
   * @param size the number of set bits
   * @param seed the random seed
   * @return the pseudo-random array int[]
   */

        private static int[] CreateSortedIntArrayOfBitsToSet(int size, int seed)
        {
            var random = new Random(seed);
            // build raw int array
            var bits = new int[size];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = random.Next(TestBsSize);
            }
            // might generate duplicates
            Array.Sort(bits);
            // first Count how many distinct values
            int counter = 0;
            int oldx = -1;
            foreach (int x in bits)
            {
                if (x != oldx)
                    ++counter;
                oldx = x;
            }
            // then construct new array
            var answer = new int[counter];
            counter = 0;
            oldx = -1;
            foreach (int x in bits)
            {
                if (x != oldx)
                {
                    answer[counter] = x;
                    ++counter;
                }
                oldx = x;
            }
            return answer;
        }

   /**
   * Pseudo-non-deterministic test inspired by S.J.vanSchaik.
   * (Yes, non-deterministic tests are bad, but the test is actually deterministic.)
   */

   /**
   * Pseudo-non-deterministic test inspired by Federico Fissore.
   *
   * @param length the number of set bits in a bitmap
   */

        private static void ShouldSetBits(int length)
        {
            Console.WriteLine("testing shouldSetBits " + length);
            int[] bitsToSet = CreateSortedIntArrayOfBitsToSet(length, 434222);
            var ewah = new EwahCompressedBitArray();
            Console.WriteLine(" ... setting " + bitsToSet.Length + " values");
            foreach (int i in bitsToSet)
            {
                ewah.Set(i);
            }
            Console.WriteLine(" ... verifying " + bitsToSet.Length + " values");
            AreEqual(ewah, bitsToSet);
            Console.WriteLine(" ... checking GetCardinality");
            Assert.AreEqual(bitsToSet.Length, ewah.GetCardinality());
        }

   /**
   * Test running length word.
   */

   /**
   * Convenience function to assess equality between an array and an enumerator over
   * Integers
   *
   * @param i the enumerator
   * @param array the array
   */

        private static void AreEqual(IEnumerable<int> i, int[] array)
        {
            int cursor = 0;
            foreach (int pos in i)
            {
                int x = pos;
                int y = array[cursor++];
                Assert.AreEqual(y, x);
            }
        }

        private static void AreEqual(IList<int> i, IList<int> array)
        {
            Assert.AreEqual(i.Count, array.Count);
            for (int k = 0; k < i.Count; k++)
            {
                Assert.AreEqual(i[k], array[k]);
            }
        }

   /**
   * Convenience function to assess equality between a compressed BitArray 
   * and an uncompressed BitArray
   *
   * @param x the compressed BitArray/bitmap
   * @param y the uncompressed BitArray/bitmap
   */

        private static void AreEqual(EwahCompressedBitArray x, BitArray y)
        {
            Assert.AreEqual(x.GetCardinality(), y.Cardinality());
            var positions = new List<int>();
            for (int ii = 0; ii < y.Count; ii++)
            {
                if (y[ii])
                {
                    positions.Add(ii);
                }
            }
            AreEqual(x.GetPositions(), positions);
        }


  /**
   * a non-deterministic test proposed by Marc Polizzi.
   *
   * @param maxlength the maximum uncompressed size of the bitmap
   */

        public static void PolizziTest(int maxlength)
        {
            Console.WriteLine("Polizzi test with max length = " + maxlength);
            for (int k = 0; k < 10000; k+= 77)
            {
                var rnd = new Random();
                var ewahBitmap1 = new EwahCompressedBitArray();
                var clrBitArray1 = new BitArray(10000);
                var ewahBitmap2 = new EwahCompressedBitArray();
                var clrBitArray2 = new BitArray(10000);
                int len = rnd.Next(maxlength);
                for (int pos = 0; pos < len; pos++)
                {
                    // random *** number of bits set ***
                    if (rnd.Next(7) == 0)
                    {
                        // random *** increasing *** values
                        ewahBitmap1.Set(pos);
                        clrBitArray1.Set(pos, true);
                    }
                    if (rnd.Next(11) == 0)
                    {
                        // random *** increasing *** values
                        ewahBitmap2.Set(pos);
                        clrBitArray2.Set(pos, true);
                    }
                }
                assertEquals(clrBitArray1, ewahBitmap1);
                assertEquals(clrBitArray2, ewahBitmap2);
                // XOR
                {
                    EwahCompressedBitArray xorEwahBitmap = ewahBitmap1.Xor(ewahBitmap2);
                    var xorclrBitArray = (BitArray) clrBitArray1.Clone();
                    xorclrBitArray.Xor(clrBitArray2);
                    assertEquals(xorclrBitArray, xorEwahBitmap);
                }
                // AND
                {
                    EwahCompressedBitArray andEwahBitmap = ewahBitmap1.And(ewahBitmap2);
                    var andclrBitArray = (BitArray) clrBitArray1.Clone();
                    andclrBitArray.And(clrBitArray2);
                    assertEquals(andclrBitArray, andEwahBitmap);
                }
                // AND
                {
                    EwahCompressedBitArray andEwahBitmap = ewahBitmap2.And(ewahBitmap1);
                    var andclrBitArray = (BitArray) clrBitArray1.Clone();
                    andclrBitArray.And(clrBitArray2);
                    assertEquals(andclrBitArray, andEwahBitmap);
                }
                // AND NOT
                {
                    EwahCompressedBitArray andNotEwahBitmap = ewahBitmap1
                        .AndNot(ewahBitmap2);
                    var andNotclrBitArray = (BitArray) clrBitArray1.Clone();
                    andNotclrBitArray.AndNot(clrBitArray2);
                    assertEquals(andNotclrBitArray, andNotEwahBitmap);
                }
                // AND NOT
                {
                    EwahCompressedBitArray andNotEwahBitmap = ewahBitmap2
                        .AndNot(ewahBitmap1);
                    var andNotclrBitArray = (BitArray) clrBitArray2.Clone();
                    andNotclrBitArray.AndNot(clrBitArray1);
                    assertEquals(andNotclrBitArray, andNotEwahBitmap);
                }
                // OR
                {
                    EwahCompressedBitArray orEwahBitmap = ewahBitmap1.Or(ewahBitmap2);
                    var orclrBitArray = (BitArray) clrBitArray1.Clone();
                    orclrBitArray.Or(clrBitArray2);
                    assertEquals(orclrBitArray, orEwahBitmap);
                }
                // OR
                {
                    EwahCompressedBitArray orEwahBitmap = ewahBitmap2.Or(ewahBitmap1);
                    var orclrBitArray = (BitArray) clrBitArray1.Clone();
                    orclrBitArray.Or(clrBitArray2);
                    assertEquals(orclrBitArray, orEwahBitmap);
                }
            }
        }

  /**
   * Assess equality between an uncompressed bitmap and a compressed one,
   *  part of a test contributed by Marc Polizzi.
   *
   * @param clrBitArray the uncompressed bitmap
   * @param ewahBitmap the compressed bitmap
   */

        private static void assertEquals(BitArray clrBitArray, EwahCompressedBitArray ewahBitmap)
        {
            assertEqualsIterator(clrBitArray, ewahBitmap);
            assertEqualsPositions(clrBitArray, ewahBitmap);
            assertCardinality(clrBitArray, ewahBitmap);
        }

  /**
   * Assess equality between an uncompressed bitmap and a compressed one,
   * part of a test contributed by Marc Polizzi
   *
   * @param clrBitArray the uncompressed bitmap
   * @param ewahBitmap the compressed bitmap
   */

        private static void assertCardinality(BitArray clrBitArray,
                                              EwahCompressedBitArray ewahBitmap)
        {
            Assert.AreEqual(ewahBitmap.GetCardinality(), clrBitArray.Cardinality());
        }

        // 
  /**
   * Assess equality between an uncompressed bitmap and a compressed one,
   * part of a test contributed by Marc Polizzi
   *
   * @param clrBitArray the clr BitArray
   * @param ewahBitmap the ewah BitArray
   */

        private static void assertEqualsIterator(BitArray clrBitArray, EwahCompressedBitArray ewahBitmap)
        {
            var positions = new List<int>();
            foreach (int bit in ewahBitmap)
            {
                Assert.IsTrue(clrBitArray.Get(bit), "enumerator: BitArray got different bits");
                positions.Add(bit);
            }

            for (int pos = clrBitArray.NextSetBit(0); pos >= 0; pos = clrBitArray.NextSetBit(pos + 1))
            {
                Assert.IsTrue(positions.Contains(pos), "enumerator: BitArray got different bits");
            }
        }

        // part of a test contributed by Marc Polizzi
  /**
   * Assert equals positions.
   *
   * @param clrBitArray the jdk bitmap
   * @param ewahBitmap the ewah bitmap
   */

        private static void assertEqualsPositions(BitArray clrBitArray,
                                                  EwahCompressedBitArray ewahBitmap)
        {
            List<int> positions = ewahBitmap.GetPositions();
            foreach (int position in positions)
            {
                Assert.IsTrue(clrBitArray.Get(position),
                              "positions: BitArray got different bits");
            }
            var ps = new HashSet<int>(positions);
            for (int pos = clrBitArray.NextSetBit(0);
                 pos >= 0;
                 pos = clrBitArray
                           .NextSetBit(pos + 1))
            {
                Assert.IsTrue(ps.Contains(pos),
                              "positions: BitArray got different bits");
            }
        }

  /**
   * Assert equals positions.
   *
   * @param ewahBitmap1 the ewah bitmap1
   * @param ewahBitmap2 the ewah bitmap2
   */

        private static void assertEqualsPositions(IList<int> positions1, IList<int> positions2)
        {
            Assert.AreEqual(positions1.Count, positions2.Count);
            for (int ii = 0; ii < positions1.Count; ii++)
            {
                Assert.AreEqual(positions1[ii], positions2[ii], "positions: alternative got different bits");
            }
        }

        [Test]
        public void EwahIteratorProblem()
        {
            Console.WriteLine("testing ArnonMoscona");
            var bitmap = new EwahCompressedBitArray();
            for (int i = 9434560; i <= 9435159; i++)
            {
                bitmap.Set(i);
            }

            List<int> v = bitmap.GetPositions();
            int k = 0;
            foreach (int ival in bitmap)
            {
                Assert.AreEqual(ival, v[k++]);
            }
            Assert.AreEqual(k, v.Count);

            for (k = 2; k <= 1024; k *= 2)
            {
                int[] bitsToSet = CreateSortedIntArrayOfBitsToSet(k, 434455 + 5*k);
                var ewah = new EwahCompressedBitArray();
                foreach (int i in bitsToSet)
                {
                    ewah.Set(i);
                }
                assertEqualsPositions(bitsToSet, ewah.GetPositions());
            }
        }
        
        
        [Test]
		  public void TayaraTest() {
		    Console.WriteLine("Tayara test");
		    for(int offset = 64; offset<(1<<30);offset*=2){
		      EwahCompressedBitArray a = new EwahCompressedBitArray();
		      EwahCompressedBitArray b = new EwahCompressedBitArray();
		      for(int k = 0; k< 64; ++k)  {
		        a.Set(offset+k);
		        b.Set(offset+k);
		      }
		      Assert.AreEqual(a.And(b).Equals(a),true);
		      Assert.AreEqual(a.Or(b).Equals(a),true);
		    }
		  }
        
        
        [Test]
        public void TestNot()
        {
           Console.WriteLine("testing not");
           var bmp= new EwahCompressedBitArray();
           for (int i = 0; i <= 184; i++) {
               bmp.Set(i);
           }
           Assert.AreEqual(185, bmp.GetCardinality());
           bmp.Not();
           Assert.AreEqual(0, bmp.GetCardinality());
           Console.WriteLine("testing not:ok");
        }

        [Test]
        public void HabermaasTest()
        {
            Console.WriteLine("testing habermaasTest");
            var bitArrayaa = new BitArray(1000131);
            var aa = new EwahCompressedBitArray();
            int[] val = {55400, 1000000, 1000128};
            foreach (int t in val)
            {
                aa.Set(t);
                bitArrayaa.Set(t, true);
            }
            assertEquals(bitArrayaa, aa);
            var bitArrayab = new BitArray(1000131);
            var ab = new EwahCompressedBitArray();
            for (int i = 4096; i < (4096 + 5); i++)
            {
                ab.Set(i);
                bitArrayab.Set(i, true);
            }
            ab.Set(99000);
            bitArrayab.Set(99000, true);
            ab.Set(1000130);
            bitArrayab.Set(1000130, true);
            assertEquals(bitArrayab, ab);
            EwahCompressedBitArray bb = aa.Or(ab);
            EwahCompressedBitArray bbAnd = aa.And(ab);
            var bitArraybb = (BitArray) bitArrayaa.Clone();
            bitArraybb.Or(bitArrayab);
            var bitArraybbAnd = (BitArray) bitArrayaa.Clone();
            bitArraybbAnd.And(bitArrayab);
            AreEqual(bbAnd, bitArraybbAnd);
            AreEqual(bb, bitArraybb);
            Console.WriteLine("testing habermaasTest:ok");
        }
        
        [Test]
        public void TestYnosa() 
        {
            Console.WriteLine("testing Ynosa");
        	var a1 = new EwahCompressedBitArray();
			var a2 = new EwahCompressedBitArray();
			a1.Set(5);
			a1.Set(15);
			a2.Set(5);
			Assert.IsTrue(a1.Intersects(a2));
            Console.WriteLine("testing Ynosa:ok");
        }
        
        [Test]
        public void TestIntersectOddNess()
        {
            Console.WriteLine("testing IntersectOddNess");
            var a1 = new EwahCompressedBitArray();
            var a2 = new EwahCompressedBitArray();
            a1.Set(12);
            a2.Set(0);
            a2.Set(1);
            a2.Set(4);
            a2.Set(14);
            Assert.IsFalse(a1.Intersects(a2));
            Console.WriteLine("testing IntersectOddNess:ok");
        }


        [Test]
        public void TestCardinality()
        {
            Console.WriteLine("testing EWAH GetCardinality");
            var bitmap = new EwahCompressedBitArray();
            bitmap.Set(int.MaxValue);
            // Console.format("Total Items %d\n", bitmap.GetCardinality());
            Assert.AreEqual(bitmap.GetCardinality(), 1);
            Console.WriteLine("testing EWAH GetCardinality:ok");
        }

        [Test]
        public void TestEwahCompressedBitArray()
        {
            Console.WriteLine("testing EWAH (basic)");
            const long zero = 0;
            const long specialval = 1L | (1L << 4) | (1L << 63);
            const long notzero = ~zero;
            var myarray1 = new EwahCompressedBitArray
                               {zero, zero, zero, specialval, specialval, notzero, zero};
            Assert.AreEqual(myarray1.GetPositions().Count, 6 + 64);
            var myarray2 = new EwahCompressedBitArray();
            myarray2.Add(zero);
            myarray2.Add(specialval);
            myarray2.Add(specialval);
            myarray2.Add(notzero);
            myarray2.Add(zero);
            myarray2.Add(zero);
            myarray2.Add(zero);
            Assert.AreEqual(myarray2.GetPositions().Count, 6 + 64);
            List<int> data1 = myarray1.GetPositions();
            List<int> data2 = myarray2.GetPositions();
            var logicalor = new List<int>();
            {
                var tmp = new HashSet<int>();
                tmp.AddRange(data1);
                tmp.AddRange(data2);
                logicalor.AddRange(tmp);
            }
            logicalor.Sort();
            var logicaland = new List<int>();
            logicaland.AddRange(data1);
            logicaland.Retain(data2);
            logicaland.Sort();
            EwahCompressedBitArray arrayand = myarray1.And(myarray2);
            AreEqual(arrayand.GetPositions(), logicaland);
            EwahCompressedBitArray arrayor = myarray1.Or(myarray2);
            AreEqual(arrayor.GetPositions(), logicalor);
            EwahCompressedBitArray arrayandbis = myarray2.And(myarray1);
            AreEqual(arrayandbis.GetPositions(), logicaland);
            EwahCompressedBitArray arrayorbis = myarray2.Or(myarray1);
            AreEqual(arrayorbis.GetPositions(), logicalor);
            var x = new EwahCompressedBitArray();
            foreach (int i in myarray1.GetPositions())
            {
                x.Set(i);
            }
            AreEqual(x.GetPositions(), myarray1.GetPositions());
            x = new EwahCompressedBitArray();
            foreach (int i in myarray2.GetPositions())
            {
                x.Set(i);
            }
            AreEqual(x.GetPositions(), myarray2.GetPositions());
            x = new EwahCompressedBitArray();
            foreach (int pos in myarray1)
            {
                x.Set(pos);
            }
            AreEqual(x.GetPositions(), myarray1.GetPositions());
            x = new EwahCompressedBitArray();
            foreach (int pos in myarray2)
            {
                x.Set(pos);
            }
            AreEqual(x.GetPositions(), myarray2.GetPositions());
            Console.WriteLine("testing EWAH (basic):ok");
        }

        [Test]
        public void TestExternalization()
        {
            Console.WriteLine("testing EWAH externalization");
            var ewcb = new EwahCompressedBitArray();
            int[] val = {5, 4400, 44600, 55400, 1000000};
            foreach (int t in val)
            {
                ewcb.Set(t);
            }

            var bos = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(bos, ewcb);
            bos.Position = 0;

            ewcb = (EwahCompressedBitArray) bf.Deserialize(bos);

            List<int> result = ewcb.GetPositions();
            AreEqual(val, result);
            Console.WriteLine("testing EWAH externalization:ok");
        }

        [Test]
        public void TestLargeEwahCompressedBitArray()
        {
            Console.WriteLine("testing EWAH over a large array");
            var myarray1 = new EwahCompressedBitArray();
            const int n = 11000000;
            for (int i = 0; i < n; ++i)
            {
                myarray1.Set(i);
            }
            Assert.AreEqual(myarray1.SizeInBits, n);
            Console.WriteLine("testing EWAH over a large array:ok");
        }

  /**
   * Test massive and.
   */

        [Test]
        public void TestMassiveAnd()
        {
            Console.WriteLine("testing massive logical and");
            var ewah = new EwahCompressedBitArray[1024];
            for (int k = 0; k < ewah.Length; ++k)
                ewah[k] = new EwahCompressedBitArray();
            for (int k = 0; k < 30000; ++k)
            {
                ewah[(k + 2*k*k)%ewah.Length].Set(k);
            }
            EwahCompressedBitArray answer = ewah[0];
            for (int k = 1; k < ewah.Length; ++k)
                answer = answer.And(ewah[k]);
            // result should be empty
            if (answer.GetPositions().Count != 0)
                Console.WriteLine(answer.ToDebugString());
            Assert.IsTrue(answer.GetPositions().Count == 0);
            Console.WriteLine("testing massive logical and:ok");
        }

  /**
   * Test massive xor.
   */

  /**
   * Test massive and not.
   */

        [Test]
        public void TestMassiveAndNot()
        {
            Console.WriteLine("testing massive and not");
            int N = 1024;
            var ewah = new EwahCompressedBitArray[N];
            for (int k = 0; k < ewah.Length; ++k)
                ewah[k] = new EwahCompressedBitArray();
            for (int k = 0; k < 30000; ++k)
            {
                ewah[(k + 2*k*k)%ewah.Length].Set(k);
            }
            EwahCompressedBitArray answer = ewah[0];
            EwahCompressedBitArray answer2 = ewah[0];
            ;
            for (int k = 1; k < ewah.Length; ++k)
            {
                answer = answer.AndNot(ewah[k]);
                EwahCompressedBitArray copy = null;
                try
                {
                    copy = (EwahCompressedBitArray) ewah[k].Clone();
                    copy.Not();
                    answer2.And(copy);
                    assertEqualsPositions(answer.GetPositions(), answer2.GetPositions());
                }
                catch (InvalidOperationException e)
                {
                    Console.Error.WriteLine(e.StackTrace);
                }
            }
            Console.WriteLine("testing massive and not:ok");
        }

  /**
   * Test massive or.
   */

        [Test]
        public void TestMassiveOr()
        {
            Console.WriteLine("testing massive logical or (can take a couple of minutes)");
            int N = 128;
            for (int howmany = 512; howmany <= 10000; howmany *= 2)
            {
                var ewah = new EwahCompressedBitArray[N];
                var bset = new BitArray[N];
                int k;
                for (k = 0; k < ewah.Length; ++k)
                    ewah[k] = new EwahCompressedBitArray();
                for (k = 0; k < bset.Length; ++k)
                    bset[k] = new BitArray(10000);
                for (k = 0; k < N; ++k)
                    assertEqualsPositions(bset[k], ewah[k]);
                for (k = 0; k < howmany; ++k)
                {
                    ewah[(k + 2*k*k)%ewah.Length].Set(k);
                    bset[(k + 2*k*k)%ewah.Length].Set(k, true);
                }
                for (k = 0; k < N; ++k)
                    assertEqualsPositions(bset[k], ewah[k]);
                EwahCompressedBitArray answer = ewah[0];
                BitArray BitArrayanswer = bset[0];
                for (k = 1; k < ewah.Length; ++k)
                {
                    EwahCompressedBitArray tmp = answer.Or(ewah[k]);
                    BitArrayanswer.Or(bset[k]);
                    answer = tmp;
                    assertEqualsPositions(BitArrayanswer, answer);
                }
                assertEqualsPositions(BitArrayanswer, answer);
                k = 0;
                foreach (int j in answer)
                {
                    if (k != j)
                        Console.WriteLine(answer.ToDebugString());
                    Assert.AreEqual(k, j);
                    k += 1;
                }
            }
            Console.WriteLine("testing massive logical or:ok");
        }

        [Test]
        public void TestMassiveXOR()
        {
            Console.WriteLine("testing massive xor (can take a couple of minutes)");
            int N = 16;
            var ewah = new EwahCompressedBitArray[N];
            var bset = new BitArray[N];
            for (int k = 0; k < ewah.Length; ++k)
                ewah[k] = new EwahCompressedBitArray();
            for (int k = 0; k < bset.Length; ++k)
                bset[k] = new BitArray(30000);
            for (int k = 0; k < 30000; ++k)
            {
                ewah[(k + 2*k*k)%ewah.Length].Set(k);
                bset[(k + 2*k*k)%ewah.Length].Set(k, true);
            }
            EwahCompressedBitArray answer = ewah[0];
            BitArray BitArrayanswer = bset[0];
            for (int k = 1; k < ewah.Length; ++k)
            {
                answer = answer.Xor(ewah[k]);
                BitArrayanswer.Xor(bset[k]);
                assertEqualsPositions(BitArrayanswer, answer);
            }
            int k2 = 0;
            foreach (int j in answer)
            {
                if (k2 != j)
                    Console.WriteLine(answer.ToDebugString());
                Assert.AreEqual(k2, j);
                k2 += 1;
            }
            Console.WriteLine("testing massive xor:ok");
        }

        [Test]
        public void TestRunningLengthWord()
        {
            Console.WriteLine("testing RunningLengthWord");
            var x = new long[1];
            var rlw = new RunningLengthWord(x, 0);
            Assert.AreEqual(0, rlw.NumberOfLiteralWords);
            Assert.AreEqual(false, rlw.RunningBit);
            Assert.AreEqual(0, rlw.RunningLength);
            rlw.RunningBit = true;
            Assert.AreEqual(0, rlw.NumberOfLiteralWords);
            Assert.AreEqual(true, rlw.RunningBit);
            Assert.AreEqual(0, rlw.RunningLength);
            rlw.RunningBit = false;
            Assert.AreEqual(0, rlw.NumberOfLiteralWords);
            Assert.AreEqual(false, rlw.RunningBit);
            Assert.AreEqual(0, rlw.RunningLength);

            for (var rl = (int) RunningLengthWord.LargestLiteralCount; rl >= 0; rl -= 64*1024)
            {
                rlw.NumberOfLiteralWords = rl;
                Assert.AreEqual(rl, rlw.NumberOfLiteralWords);
                Assert.AreEqual(false, rlw.RunningBit);
                Assert.AreEqual(0, rlw.RunningLength);
                rlw.NumberOfLiteralWords = 0;
                Assert.AreEqual(0, rlw.NumberOfLiteralWords);
                Assert.AreEqual(false, rlw.RunningBit);
                Assert.AreEqual(0, rlw.RunningLength);
            }

            for (long rl = 0; rl <= RunningLengthWord.LargestRunningLengthCount; rl += 64*1024)
            {
                rlw.RunningLength = rl;
                Assert.AreEqual(0, rlw.NumberOfLiteralWords);
                Assert.AreEqual(false, rlw.RunningBit);
                Assert.AreEqual(rl, rlw.RunningLength);
                rlw.RunningLength = 0;
                Assert.AreEqual(0, rlw.NumberOfLiteralWords);
                Assert.AreEqual(false, rlw.RunningBit);
                Assert.AreEqual(0, rlw.RunningLength);
            }
        
            rlw.RunningBit = true;
            for (long rl = 0; rl <= RunningLengthWord.LargestRunningLengthCount; rl += 64*1024)
            {
                rlw.RunningLength = rl;
                Assert.AreEqual(0, rlw.NumberOfLiteralWords);
                Assert.AreEqual(true, rlw.RunningBit);
                Assert.AreEqual(rl, rlw.RunningLength);
                rlw.RunningLength = 0;
                Assert.AreEqual(0, rlw.NumberOfLiteralWords);
                Assert.AreEqual(true, rlw.RunningBit);
                Assert.AreEqual(0, rlw.RunningLength);
            }

            for (long rl = 0; rl <= RunningLengthWord.LargestLiteralCount; rl += 64*128)
            {
                rlw.NumberOfLiteralWords = rl;
                Assert.AreEqual(rl, rlw.NumberOfLiteralWords);
                Assert.AreEqual(true, rlw.RunningBit);
                Assert.AreEqual(0, rlw.RunningLength);
                rlw.NumberOfLiteralWords = 0;
                Assert.AreEqual(0, rlw.NumberOfLiteralWords);
                Assert.AreEqual(true, rlw.RunningBit);
                Assert.AreEqual(0, rlw.RunningLength);
            }
            Console.WriteLine("testing RunningLengthWord:ok");
        }
        
  [Test]
  public void testsetSizeInBits() {
	  Console.WriteLine("testing setSizeInBits");
	  for(int k = 0; k < 4096; ++k) {
		  EwahCompressedBitArray ewah = new EwahCompressedBitArray();
		  ewah.SizeInBits = k;
		  Assert.AreEqual(ewah.SizeInBits,k);
		  Assert.AreEqual(ewah.GetCardinality(),0);
		  EwahCompressedBitArray ewah2 = new EwahCompressedBitArray();
		  ewah2.SetSizeInBits(k, false);
		  Assert.AreEqual(ewah2.SizeInBits,k);
		  Assert.AreEqual(ewah2.GetCardinality(),0);
		  EwahCompressedBitArray ewah3 = new EwahCompressedBitArray();
		  for(int i = 0; i < k ; ++i) {
			  ewah3.Set(i);
		  }
		  Assert.AreEqual(ewah3.SizeInBits,k);
		  Assert.AreEqual(ewah3.GetCardinality(),k);
		  EwahCompressedBitArray ewah4 = new EwahCompressedBitArray();
		  ewah4.SetSizeInBits(k, true);
		  Assert.AreEqual(ewah4.SizeInBits,k);
		  Assert.AreEqual(ewah4.GetCardinality(),k);
	  }
  }

		  [Test]
		  public void TestSizeInBits1() {
		  	  Console.WriteLine("testing TestSizeInBits1");
		      EwahCompressedBitArray bitmap = new EwahCompressedBitArray();
		      bitmap.SetSizeInBits(1, false);
		      Assert.AreEqual(1, bitmap.SizeInBits);
		      bitmap.Not();
		      Assert.AreEqual(1, bitmap.GetCardinality());
		  }
		
		  [Test]
		   public void TestHasNextSafe() {
		  	  Console.WriteLine("testing TestHasNextSafe");
		      EwahCompressedBitArray bitmap = new EwahCompressedBitArray();
		      bitmap.Set(0);
		      IEnumerator<int> it = ((IEnumerable<int>)bitmap).GetEnumerator();
		      Assert.AreEqual(it.MoveNext(),true);
		      Assert.AreEqual(0, it.Current);
		  }
		  
	  [Test]
	 public void testDebugSetSizeInBitsTest() {      
		  Console.WriteLine("testing DebugSetSizeInBits");
		  EwahCompressedBitArray b = new EwahCompressedBitArray();
	      
	      b.Set(4);
	      
	      b.SetSizeInBits(6, true);
	       
	      List<int> positions = b.GetPositions();
	      
	      Assert.AreEqual(2, positions.Count);
	      Assert.AreEqual(4, positions[0]);
	      Assert.AreEqual(5, positions[1]);
	      
		  IEnumerator<int> iterator = ((IEnumerable<int>)b).GetEnumerator();
	      Assert.AreEqual(true,iterator.MoveNext());
	      Assert.AreEqual(4, iterator.Current);
	      Assert.AreEqual(true,iterator.MoveNext());
	      Assert.AreEqual(5, iterator.Current);
	      Assert.AreEqual(false,iterator.MoveNext());
	      
	   }
	   
	    [Test]
			public void SsiYanKaiTest() {
		    Console.WriteLine("testing SsiYanKaiTest");
		    EwahCompressedBitArray a = EwahCompressedBitArray.BitmapOf(39935, 39936, 39937, 39938, 39939, 39940, 39941, 39942, 39943, 39944, 39945, 39946, 39947, 39948, 39949, 39950, 39951, 39952, 39953, 39954, 39955, 39956, 39957, 39958, 39959, 39960, 39961, 39962, 39963, 39964, 39965, 39966, 39967, 39968, 39969, 39970, 39971, 39972, 39973, 39974, 39975, 39976, 39977, 39978, 39979, 39980, 39981, 39982, 39983, 39984, 39985, 39986, 39987, 39988, 39989, 39990, 39991, 39992, 39993, 39994, 39995, 39996, 39997, 39998, 39999, 40000, 40001, 40002, 40003, 40004, 40005, 40006, 40007, 40008, 40009, 40010, 40011, 40012, 40013, 40014, 40015, 40016, 40017, 40018, 40019, 40020, 40021, 40022, 40023, 40024, 40025, 40026, 40027, 40028, 40029, 40030, 40031, 40032, 40033, 40034, 40035, 40036, 40037, 40038, 40039, 40040, 40041, 40042, 40043, 40044, 40045, 40046, 40047, 40048, 40049, 40050, 40051, 40052, 40053, 40054, 40055, 40056, 40057, 40058, 40059, 40060, 40061, 40062, 40063, 40064, 40065, 40066, 40067, 40068, 40069, 40070, 40071, 40072, 40073, 40074, 40075, 40076, 40077, 40078, 40079, 40080, 40081, 40082, 40083, 40084, 40085, 40086, 40087, 40088, 40089, 40090, 40091, 40092, 40093, 40094, 40095, 40096, 40097, 40098, 40099, 40100);
		    EwahCompressedBitArray b = EwahCompressedBitArray.BitmapOf(39935, 39936, 39937, 39938, 39939, 39940, 39941, 39942, 39943, 39944, 39945, 39946, 39947, 39948, 39949, 39950, 39951, 39952, 39953, 39954, 39955, 39956, 39957, 39958, 39959, 39960, 39961, 39962, 39963, 39964, 39965, 39966, 39967, 39968, 39969, 39970, 39971, 39972, 39973, 39974, 39975, 39976, 39977, 39978, 39979, 39980, 39981, 39982, 39983, 39984, 39985, 39986, 39987, 39988, 39989, 39990, 39991, 39992, 39993, 39994, 39995, 39996, 39997, 39998, 39999, 270000);
		    HashSet<int> aPositions = new HashSet<int>(a.GetPositions());    
		    int intersection = 0;
		    EwahCompressedBitArray inter = new EwahCompressedBitArray();
		    HashSet<int> bPositions = new HashSet<int>(b.GetPositions());
		    foreach (int integer in bPositions) {
		       if (aPositions.Contains(integer)) {
		         inter.Set(integer);
		          ++intersection;
		       }
		    }
		    EwahCompressedBitArray and2 = a.And(b);      
		    List<int> l1 = inter.GetPositions();
		    List<int> l2 = and2.GetPositions();
		    var ok = true;
		    if(l1.Count != l2.Count) { 
		        Console.WriteLine("cardinality differs = "+l1.Count+" "+l2.Count);
		        ok = false;
		    }
		    for(int k = 0; k< l1.Count; ++k) {
		    	if(l1[k] != l2[k]) {
		    			    Console.WriteLine("differ at "+k+" = "+l1[k]+" "+l2[k]);
		    			    ok = false;
		    	}

		    }
		    Assert.IsTrue(ok);
            Assert.AreEqual(true,and2.Equals(inter));
            Assert.AreEqual(inter.GetHashCode(),and2.GetHashCode());
		    Assert.AreEqual(intersection ,and2.GetCardinality());
		  } 
		  
        [Test]
		public void TestCloneEwahCompressedBitArray()
		{
		    Console.WriteLine("testing EWAH clone");
		    EwahCompressedBitArray a = new EwahCompressedBitArray();
		    a.Set(410018);
		    a.Set(410019);
		    a.Set(410020);
		    a.Set(410021);
		    a.Set(410022);
		    a.Set(410023);
		
		    EwahCompressedBitArray b = (EwahCompressedBitArray)a.Clone();
		
		    a.SetSizeInBits(487123, false);
		    b.SetSizeInBits(487123, false);
		
		    Assert.AreEqual(a, b);
		}

        [Test]
        public void TestSetGet()
        {
            Console.WriteLine("testing EWAH Set/get");
            var ewcb = new EwahCompressedBitArray();
            int[] val = {5, 4400, 44600, 55400, 1000000};
            for (int k = 0; k < val.Length; ++k)
            {
                ewcb.Set(val[k]);
            }
            List<int> result = ewcb.GetPositions();
            AreEqual(val, result);
            Console.WriteLine("testing EWAH Set/get:ok");
        }

  /**
   * Created: 2/4/11 6:03 PM By: Arnon Moscona.
   */

  /**
   * Test with parameters.
   *
   * @throws IOException Signals that an I/O exception has occurred.
   */

        [Test]
        public void TestWithParameters()
        {
            Console
                .WriteLine("These tests can run for several minutes. Please be patient.");
            for (int k = 2; k < 1 << 24; k *= 8)
                ShouldSetBits(k);
            Console.WriteLine("64");

            PolizziTest(64);
            PolizziTest(128);
            PolizziTest(256);
            PolizziTest(2048);
            Console.WriteLine("Your code is probably ok.");
        }

        [Test]
        public void VanSchaikTest()
        {
            Console.WriteLine("testing vanSchaikTest (this takes some time)");
            const int totalNumBits = 32768;
            const double odds = 0.9;
            var rand = new Random(323232323);
            for (int t = 0; t < 100; t++)
            {
                int numBitsSet = 0;
                var cBitMap = new EwahCompressedBitArray();
                for (int i = 0; i < totalNumBits; i++)
                {
                    if (rand.NextDouble() < odds)
                    {
                        cBitMap.Set(i);
                        numBitsSet++;
                    }
                }
                Assert.AreEqual(cBitMap.GetCardinality(), numBitsSet);
            }         
            Console.WriteLine("testing vanSchaikTest:ok");
        }
    }
}