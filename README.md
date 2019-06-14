CSharpEWAH
==

(c) Kemal Erdogan, Daniel Lemire, Ciaran Jessup, Michael Rice, Matt Warren
This code is licensed under the Apache
License, Version 2.0 (ASL2.0)

This is a compressed variant of
the standard bitarray class. It uses a 64-bit RLE-like
compression scheme. It can be used to implement
bitmap indexes.

The goal of word-aligned compression is not to
achieve the best compression, but rather to
improve query processing time. Hence, we try
to save CPU cycles, maybe at the expense of
storage. However, the EWAH scheme we implemented
is always more efficient storage-wise than an
uncompressed bitarray.


Real-world usage
----------------

CSharpEWAH has been reviewed by Matt Warren as part of his work on the Stack Overflow tag system:

http://mattwarren.org/2015/10/29/the-stack-overflow-tag-engine-part-3/

The Java counterpart of this library (JavaEWAH) is part of Apache Hive and its derivatives (e.g.,  Apache Spark) and Eclipse JGit. It has been used in production systems for many years. It is part of major Linux distributions.

EWAH is used to accelerate the distributed version control system Git (http://githubengineering.com/counting-objects/). You can find the C port of EWAH written by the Git team at https://github.com/git/git/tree/master/ewah

When should you use a bitmap?
----------------------------------------

Sets are a fundamental abstraction in
software. They can be implemented in various
ways, as hash sets, as trees, and so forth.
In databases and search engines, sets are often an integral
part of indexes. For example, we may need to maintain a set
of all documents or rows  (represented by numerical identifier)
that satisfy some property. Besides adding or removing
elements from the set, we need fast functions
to compute the intersection, the union, the difference between sets, and so on.


To implement a set
of integers, a particularly appealing strategy is the
bitmap (also called bitset or bit vector). Using n bits,
we can represent any set made of the integers from the range
[0,n): it suffices to set the ith bit is set to one if integer i is present in the set.
Commodity processors use words of W=32 or W=64 bits. By combining many such words, we can
support large values of n. Intersections, unions and differences can then be implemented
 as bitwise AND, OR and ANDNOT operations.
More complicated set functions can also be implemented as bitwise operations.

When the bitset approach is applicable, it can be orders of
magnitude faster than other possible implementation of a set (e.g., as a hash set)
while using several times less memory.


When should you use compressed bitmaps?
----------------------------------------

An uncompress BitSet can use a lot of memory. For example, if you take a BitSet
and set the bit at position 1,000,000 to true and you have just over 100kB. That's over 100kB
to store the position of one bit. This is wasteful  even if you do not care about memory:
suppose that you need to compute the intersection between this BitSet and another one
that has a bit at position 1,000,001 to true, then you need to go through all these zeroes,
whether you like it or not. That can become very wasteful.

This being said, there are definitively cases where attempting to use compressed bitmaps is wasteful.
For example, if you have a small universe size. E.g., your bitmaps represent sets of integers
from [0,n) where n is small (e.g., n=64 or n=128). If you are able to uncompressed BitSet and
it does not blow up your memory usage,  then compressed bitmaps are probably not useful
to you. In fact, if you do not need compression, then a BitSet offers remarkable speed.
One of the downsides of a compressed bitmap like those provided by JavaEWAH is slower random access:
checking whether a bit is set to true in a compressed bitmap takes longer.


How does EWAH compares with the alternatives?
-------------------------------------------

EWAH is part of a larger family of compressed bitmaps that are run-length-encoded
bitmaps. They identify long runs of 1s or 0s and they represent them with a marker word.
If you have a local mix of 1s and 0, you use an uncompressed word.

There are many formats in this family beside EWAH:

* Oracle's BBC is an obsolete format at this point: though it may provide good compression,
it is likely much slower than more recent alternatives due to excessive branching.
* WAH is a patented variation on BBC that provides better performance.
* Concise is a variation on the patented WAH. It some specific instances, it can compress
much better than WAH (up to 2x better), but it is generally slower.
* EWAH is both free of patent, and it is faster than all the above. On the downside, it
does not compress quite as well. It is faster because it allows some form of "skipping"
over uncompressed words. So though none of these formats are great at random access, EWAH
is better than the alternatives.

There are other alternatives however. For example, the Roaring
format (https://github.com/lemire/RoaringBitmap) is not a run-length-encoded hybrid. It provides faster random access
than even EWAH.


Data format
------------

For more details regarding the compression format, please
see Section 3 of the following paper:

Daniel Lemire, Owen Kaser, Kamel Aouiche, Sorting improves word-aligned bitmap indexes. Data & Knowledge Engineering 69 (1), pages 3-28, 2010.  
 http://arxiv.org/abs/0901.3751

 (The PDF file is freely available on the arXiv site.)

Unit testing
==

 Building using Mono


You can build CSharpEWAH using the open source
Mono toolchain using the msbuild command.
Then you can run the executable using
the mono command:
```
$ nuget restore EWAH.sln
$ msbuild
$ mono ./EWAH.RunTests/bin/Debug/EWAH.RunTests.exe
```

This will run unit tests.


Usage
==

See example.cs.
