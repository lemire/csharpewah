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

        var ewahBitmap1 = new EwahCompressedBitArray();
        var ewahBitmap2 = new EwahCompressedBitArray();
        ewahBitmap1.Set(0);
        ewahBitmap1.Set(2);
        ewahBitmap1.Set(64);
        ewahBitmap1.Set(1 << 30);
        Console.WriteLine("Running demo program:");
        Console.WriteLine("bitmap 1:");
        foreach (int k in ewahBitmap1)
            Console.WriteLine(k);
        ewahBitmap2.Set(1);
        ewahBitmap2.Set(3);
        ewahBitmap2.Set(64);
        ewahBitmap2.Set(1 << 30);
        Console.WriteLine("bitmap 2:");
        foreach (int k in ewahBitmap2)
            Console.WriteLine(k);
        Console.WriteLine();
        Console.WriteLine("bitmap 1 OR bitmap 2:");
        EwahCompressedBitArray orbitmap = ewahBitmap1.Or(ewahBitmap2);
        foreach (int k in orbitmap)
            Console.WriteLine(k);
        Console.WriteLine("memory usage: " + orbitmap.SizeInBytes + " bytes");
        Console.WriteLine();
        Console.WriteLine("bitmap 1 AND bitmap 2:");
        EwahCompressedBitArray andbitmap = ewahBitmap1.And(ewahBitmap2);
        foreach (int k in andbitmap)
            Console.WriteLine(k);
        Console.WriteLine("memory usage: " + andbitmap.SizeInBytes + " bytes");
        Console.WriteLine("bitmap 1 XOR bitmap 2:");
        EwahCompressedBitArray xorbitmap = ewahBitmap1.Xor(ewahBitmap2);
        foreach (int k in xorbitmap)
            Console.WriteLine(k);
        Console.WriteLine("memory usage: " + andbitmap.SizeInBytes + " bytes");
        Console.WriteLine("End of demo.");
        Console.WriteLine("");
        var tr = new EwahCompressedBitArrayTest();/*
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
        tr.TestRunningLengthWord();*/
        tr.TestSizeInBits1();
        tr.TestHasNextSafe();
        tr.TestCloneEwahCompressedBitArray();
        tr.TestSetGet();
        tr.TestWithParameters();

        new EWAHCompressedBitArraySerializerTest().TestCustomSerializationStrategy();

    }
}