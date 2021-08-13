using System;
using System.Runtime.CompilerServices;

namespace SpotifyBot.Api
{
    public static class MutateExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T With<T>(this T x, Action<T> mutator)
        {
            mutator(x);
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ignore<T>(this T _) { }

    }
}