using System;
using System.Security.Cryptography;
using System.Text;

namespace Gluid
{
    /// <summary>
    /// A special representation of a Guid that can contain (or internalise) one or two 4-byte integers or a single long in a reversible way against a namespace
    /// The Guid uses a reserved version number (0xf) to ensure it doesn't clash with most system generated Guids.
    /// Beyond that it's up to you to ensure that your integers and namespaces are unique.
    /// </summary>
    public static class Gluid
    {
        // Using Guid Version 1 (0x1) which is undefined within the variant used.
        private const byte GuidVersion = 0x10;
        private const byte GuidVersionMask = 0xf0;
        private const int GuidVersionByteNumber = 7;

        // Using Guid Variant 8 (0xe) which is currently marked as reserved so shouldn't clash with existing Guids.
        private const byte GuidVariant = 0xe0;
        private const byte GuidVariantMask = 0xe0;
        private const int GuidVariantByteNumber = 8;

        // This value is the number of bytes of hash to use and represents the amount of entropy available in the used hash.
        // Maximum value is 15. Smaller values will mean that less uniqueness is possible however more of the underlying
        // integer information will be directly available. For example, an entropy of 11 will expose the Int value inside
        // the Gluid. This would allow the Guid generated to be directly sortable in a database column.
        private const int Entropy = 15;

        /// <summary>
        /// A unique 3 byte array that allows the Gluid to ensure that a namespace is decoded correctly before extracting the
        /// inetrnal integer information.
        /// </summary>
        private static readonly byte[] _identifier = { 0xcc, 0xcc, 0xcc };

        /// <summary>
        /// Construct a Gluid from a Namespace and existing Id value
        /// </summary>
        /// <param name="value">The original Id value</param>
        /// <param name="namespace">A string to uniquely seperate regular Id values, if null is used no encoding will be used</param>
        public static Guid NewGluid(int value, string @namespace = null) =>
            NewGluid(value, 0, @namespace);

        /// <summary>
        /// Construct a Gluid from a Namespace and two existing Id values
        /// </summary>
        /// <param name="value1">One of the original Id values</param>
        /// <param name="value2">One of the original Id values</param>
        /// <param name="namespace">A string to uniquely seperate regular Id values, if null is used no encoding will be used</param>
        public static Guid NewGluid(int value1, int value2, string @namespace = null)
        {
            var intBytes1 = BitConverter.GetBytes(value1);
            var intBytes2 = BitConverter.GetBytes(value2);

            var guidBytes = new byte[16];

            PutLowerBytes(intBytes1, guidBytes);
            PutUpperBytes(intBytes2, guidBytes);

            Buffer.BlockCopy(_identifier, 0, guidBytes, 9, 3);

            var transcodedBytes = TranscodeNamespace(@namespace, guidBytes);

            var subjectGuid = new Guid(transcodedBytes);
            return subjectGuid;
        }

        /// <summary>
        /// Construct a Gluid from a Namespace and existing Id value
        /// </summary>
        /// <param name="value">The original Id value</param>
        /// <param name="namespace">A string to uniquely seperate regular Id values, if null is used no encoding will be used</param>
        public static Guid NewGluid(long value, string @namespace = null)
        {
            var longBytes = BitConverter.GetBytes(value);
            var upperBytes = new byte[4];

            Buffer.BlockCopy(longBytes, 4, upperBytes, 0, 4);

            var guidBytes = new byte[16];

            PutLowerBytes(longBytes, guidBytes);
            PutUpperBytes(upperBytes, guidBytes);

            Buffer.BlockCopy(_identifier, 0, guidBytes, 9, 3);

            var transcodedBytes = TranscodeNamespace(@namespace, guidBytes);

            var subjectGuid = new Guid(transcodedBytes);
            return subjectGuid;
        }

        /// <summary>
        /// Indicates if this Guid is actually a Gluid
        /// </summary>
        /// <returns>A bool indicating if the Guid is a Gluid</returns>
        public static bool IsGluid(this Guid guid)
        {
            var guidBytes = guid.ToByteArray();
            
            return GetVersion(guidBytes) == GuidVersion &&
                GetVariant(guidBytes) == GuidVariant;
        }

        /// <summary>
        /// Indicates if this Gluid is linked to a regular Id value on the specified namespace
        /// </summary>
        /// <param name="namespace">The namespace to check</param>
        /// <returns>A bool indicating if the Gluid has a regular Id value in the namespace</returns>
        public static bool IsLinked(this Guid guid, string @namespace = null) =>
            TryTranscodeAndCheck(guid, @namespace, out _);

        /// <summary>
        /// Get the associated original Id from the Gluid based on a provided namespace
        /// </summary>
        /// <param name="namespace">The namespace to filter the Id against</param>
        /// <returns>The original Int32 Id or null if this is a regular Guid or doesn't come from the supplied namespace</returns>
        public static int? ToInt32(this Guid guid, string @namespace = null) =>
            TryTranscodeAndCheck(guid, @namespace, out var guidBytes) ?
            BitConverter.ToInt32(GetLowerBytes(guidBytes), 0) :
            (int?)null;

        /// <summary>
        /// Get the associated original long Id from the Gluid based on a provided namespace
        /// </summary>
        /// <param name="namespace">The namespace to filter the Id against</param>
        /// <returns>The original Int64 Id or null if this is a regular Guid or doesn't come from the supplied namespace</returns>
        public static long? ToInt64(this Guid guid, string @namespace = null) =>
            TryTranscodeAndCheck(guid, @namespace, out var guidBytes) ?
            GetInt64(guidBytes) :
            (long?)null;

        /// <summary>
        /// Get the associated original second int Id from the Gluid
        /// </summary>
        /// <param name="namespace">The namespace to filter the Id against</param>
        /// <returns>The original second int32 Id or null if this is just a regular Guid</returns>
        public static int? GetSecondInt32(this Guid guid, string @namespace = null) =>
            TryTranscodeAndCheck(guid, @namespace, out var guidBytes) ?
            BitConverter.ToInt32(GetUpperBytes(guidBytes), 0) :
            (int?)null;

        #region Private Static Helper Methods
        private static long GetInt64(byte[] guidBytes)
        {
            var longBytes = new byte[8];

            Buffer.BlockCopy(GetLowerBytes(guidBytes), 0, longBytes, 0, 4);
            Buffer.BlockCopy(GetUpperBytes(guidBytes), 0, longBytes, 4, 4);

            return BitConverter.ToInt64(longBytes, 0);
        }

        private static byte GetVersion(byte[] guidBytes) =>
            (byte)(guidBytes[GuidVersionByteNumber] & GuidVersionMask);

        private static byte GetVariant(byte[] guidBytes) =>
            (byte)(guidBytes[GuidVariantByteNumber] & GuidVariantMask);

        private static bool TryTranscodeAndCheck(Guid guid, string @namespace, out byte[] transcodedBytes)
        {
            transcodedBytes = guid.ToByteArray();

            if (!IsGluid(guid))
                return false;

            transcodedBytes = TranscodeNamespace(@namespace, transcodedBytes);

            return transcodedBytes[9] == _identifier[0] &&
                transcodedBytes[10] == _identifier[1] &&
                transcodedBytes[11] == _identifier[2];
        }

        private static byte[] GetLowerBytes(byte[] guidBytes)
        {
            var lowerBytes = new byte[4];

            lowerBytes[0] = guidBytes[0];
            lowerBytes[1] = guidBytes[1];
            lowerBytes[2] = guidBytes[2];
            lowerBytes[3] = guidBytes[3];

            return lowerBytes;
        }

        private static byte[] GetUpperBytes(byte[] guidBytes)
        {
            var upperBytes = new byte[4];

            upperBytes[0] = guidBytes[4];
            upperBytes[1] = guidBytes[5];
            upperBytes[2] = (byte)(((guidBytes[7] & 0x0f) << 4) | (guidBytes[8] & 0x0f));
            upperBytes[3] = guidBytes[6];

            return upperBytes;
        }

        private static void PutLowerBytes(byte[] lowerBytes, byte[] guidBytes)
        {
            guidBytes[0] = lowerBytes[0];
            guidBytes[1] = lowerBytes[1];
            guidBytes[2] = lowerBytes[2];
            guidBytes[3] = lowerBytes[3];
        }

        private static void PutUpperBytes(byte[] upperBytes, byte[] guidBytes)
        {
            var versionByte = (byte)(GuidVersion & GuidVersionMask);
            var variantByte = (byte)(GuidVariant & GuidVariantMask);

            guidBytes[4] = upperBytes[0];
            guidBytes[5] = upperBytes[1];
            guidBytes[7] = (byte)(versionByte | ((upperBytes[2] & 0xf0) >> 4));
            guidBytes[6] = upperBytes[3];
            guidBytes[8] = (byte)(variantByte | (upperBytes[2] & 0x0f));
        }

        private static byte[] TranscodeNamespace(string @namespace, byte[] guidBytes)
        {
            if (@namespace == null)
                return guidBytes;

            var hash = GetHash(@namespace);

            if (hash.Length != guidBytes.Length)
                return guidBytes;

            var result = new byte[16];
            Buffer.BlockCopy(guidBytes, 0, result, 0, 16);

            for (int idx = 0; idx < guidBytes.Length; idx++)
            {
                result[idx] ^= hash[idx];
            }

            return result;
        }

        private static byte[] GetHash(string @namespace)
        {
            var result = new byte[16];
            var encArray = new byte[15];

            var hashArray = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(@namespace));
            Buffer.BlockCopy(hashArray, 0, encArray, 15 - Entropy, Entropy);

            Buffer.BlockCopy(encArray, 0, result, 0, 7);
            result[7] = (byte)(encArray[7] & 0x0f);
            result[8] = (byte)((encArray[7] & 0xf0) >> 4);
            Buffer.BlockCopy(encArray, 8, result, 9, 7);
            
            return result;
        }
        #endregion
    }
}
