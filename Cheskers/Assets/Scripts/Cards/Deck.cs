using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    //Encoding for a hand
    //Hand is the product of primes -> Unique number
    // Start is called before the first frame update
    enum Rank
    {
        A = 0, K = 1, Q = 2, J = 4, r10 = 8, r9 = 16, r8 = 32, r7 = 64, 
        r6 = 128, r5 = 256, r4 = 512, r3 = 1024,r2 = 2056
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public static class BinaryConverter
{
    public static BitArray ToBinary(this int numeral)
    {
        return new BitArray(new[] { numeral });
    }

    public static int ToNumeral(this BitArray binary)
    {
        if (binary == null)
            throw new ArgumentNullException("binary");
        if (binary.Length > 32)
            throw new ArgumentException("must be at most 32 bits long");

        var result = new int[1];
        binary.CopyTo(result, 0);
        return result[0];
    }
}