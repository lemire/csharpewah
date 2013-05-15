using System;
using Ewah;

/*
 * Copyright 2012, Kemal Erdogan, Daniel Lemire and Ciaran Jessup
 * Licensed under APL 2.0.
 */

public class example
{
    public static void Main(string[] args)
    {

        var ewahBitmap1 = EwahCompressedBitArray.BitmapOf(0,2,64,1 << 30);
        var ewahBitmap2 = EwahCompressedBitArray.BitmapOf(1,3,64,1 << 30);
        Console.WriteLine("Running demo program:");
        Console.WriteLine("bitmap 1: "+ewahBitmap1 );
        Console.WriteLine("bitmap 2:"+ewahBitmap2);
        EwahCompressedBitArray orbitmap = ewahBitmap1.Or(ewahBitmap2);
        Console.WriteLine();
        Console.WriteLine("bitmap 1 OR bitmap 2:"+orbitmap);
        Console.WriteLine("memory usage: " + orbitmap.SizeInBytes + " bytes");
        Console.WriteLine();
        EwahCompressedBitArray andbitmap = ewahBitmap1.And(ewahBitmap2);
        Console.WriteLine("bitmap 1 AND bitmap 2:"+andbitmap);
        Console.WriteLine("memory usage: " + andbitmap.SizeInBytes + " bytes");
        EwahCompressedBitArray xorbitmap = ewahBitmap1.Xor(ewahBitmap2);
        Console.WriteLine("bitmap 1 XOR bitmap 2:"+xorbitmap);
        Console.WriteLine("memory usage: " + andbitmap.SizeInBytes + " bytes");
        Console.WriteLine("End of demo.");
        Console.WriteLine("");
        var tr = new EwahCompressedBitArrayTest();
        tr.TestYnosa();
        tr.TestIntersectOddNess();
        tr.testsetSizeInBits();
        tr.SsiYanKaiTest();
        tr.testDebugSetSizeInBitsTest();
        tr.EwahIteratorProblem();
        tr.TayaraTest();
        tr.TestNot();
        tr.TestCardinality();
        tr.TestEwahCompressedBitArray();
        tr.TestExternalization();
        tr.TestLargeEwahCompressedBitArray();
        tr.TestMassiveAnd();
        tr.TestMassiveAndNot();
        tr.TestMassiveOr();
        tr.TestMassiveXOR();
        tr.HabermaasTest();
        tr.VanSchaikTest();
        tr.TestRunningLengthWord();
        tr.TestSizeInBits1();
        tr.TestHasNextSafe();
        tr.TestCloneEwahCompressedBitArray();
        tr.TestSetGet();
        tr.TestWithParameters();

        new EWAHCompressedBitArraySerializerTest().TestCustomSerializationStrategy();

    }
}