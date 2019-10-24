// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// <para>A C# implementation of the murmur3 hash:</para>
    /// <para>Source: https://gist.github.com/automatonic/3725443 .</para>
    /// </summary>
    public static class MurMurHash3
    {
        private const uint SEED = 19120623;

        /// <summary>
        /// Hashes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>System.Int32.</returns>
        public static int Hash(string text)
        {
            using (var stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(text);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                return Hash(stream);
            }
        }

        /// <summary>
        /// Hashes the specified stream into a single value. This method DOES NOT close
        /// the stream when the operation is complete.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>System.Int32.</returns>
        public static int Hash(Stream stream)
        {
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;

            uint h1 = SEED;
            uint k1 = 0;
            uint streamLength = 0;

            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                byte[] chunk = reader.ReadBytes(4);
                while (chunk.Length > 0)
                {
                    streamLength += (uint)chunk.Length;
                    switch (chunk.Length)
                    {
                        case 4:
                            /* Get four bytes from the input into an uint */
                            k1 = (uint)(chunk[0] | chunk[1] << 8 | chunk[2] << 16 | chunk[3] << 24);

                            /* bitmagic hash */
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;

                            h1 ^= k1;
                            h1 = Rotl32(h1, 13);
                            h1 = (h1 * 5) + 0xe6546b64;
                            break;

                        case 3:
                            k1 = (uint)(chunk[0] | chunk[1] << 8 | chunk[2] << 16);
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;

                        case 2:
                            k1 = (uint)(chunk[0] | chunk[1] << 8);
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;

                        case 1:
                            k1 = (uint)chunk[0];
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;
                    }

                    chunk = reader.ReadBytes(4);
                }
            }

            // finalization, magic chants to wrap it all up
            h1 ^= streamLength;
            h1 = Fmix(h1);

            // ignore overflow
            unchecked
            {
                return (int)h1;
            }
        }

        private static uint Rotl32(uint x, byte r)
        {
            return (x << r) | (x >> (32 - r));
        }

        private static uint Fmix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }
    }
}