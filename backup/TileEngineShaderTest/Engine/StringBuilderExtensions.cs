using System;
using System.Diagnostics;
using System.Text;

namespace TileEngineShaderTest.Engine
{
    /// <summary>
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        ///     These digits are here in a static array to support hex with simple, easily-understandable code.
        ///     Since A-Z don't sit next to 0-9 in the ascii table.
        /// </summary>
        private static readonly char[] msDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        ///     5 = Matches standard .NET formatting dp's
        /// </summary>
        private const uint MsDefaultDecimalPlaces = 5;

        /// <summary>
        /// </summary>
        private const char MsDefaultPadChar = '0';

        /// <summary>
        ///     Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Any base value allowed.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="uintVal"></param>
        /// <param name="padAmount"></param>
        /// <param name="padChar"></param>
        /// <param name="baseVal"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, uint uintVal, uint padAmount, char padChar, uint baseVal)
        {
            Debug.Assert(baseVal > 0 && baseVal <= 16);

            // Calculate length of integer when written out
            uint length = 0;
            var lengthCalc = uintVal;

            do
            {
                lengthCalc /= baseVal;
                length++;
            }
            while (lengthCalc > 0);

            // Pad out space for writing.
            stringBuilder.Append(padChar, (int)Math.Max(padAmount, length));

            var strpos = stringBuilder.Length;

            // We're writing backwards, one character at a time.
            while (length > 0)
            {
                strpos--;

                // Lookup from static char array, to cover hex values too
                stringBuilder[strpos] = msDigits[uintVal % baseVal];

                uintVal /= baseVal;
                length--;
            }

            return stringBuilder;
        }

        /// <summary>
        ///     Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Assume no padding and
        ///     base ten.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="uintVal"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, uint uintVal)
        {
            stringBuilder.Concat(uintVal, 0, MsDefaultPadChar, 10);
            return stringBuilder;
        }

        /// <summary>
        ///     Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Assume base ten.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="uintVal"></param>
        /// <param name="padAmount"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, uint uintVal, uint padAmount)
        {
            stringBuilder.Concat(uintVal, padAmount, MsDefaultPadChar, 10);
            return stringBuilder;
        }

        /// <summary>
        ///     Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Assume base ten.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="uintVal"></param>
        /// <param name="padAmount"></param>
        /// <param name="padChar"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, uint uintVal, uint padAmount, char padChar)
        {
            stringBuilder.Concat(uintVal, padAmount, padChar, 10);
            return stringBuilder;
        }

        /// <summary>
        ///     Convert a given signed integer value to a string and concatenate onto the stringbuilder. Any base value allowed.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="intVal"></param>
        /// <param name="padAmount"></param>
        /// <param name="padChar"></param>
        /// <param name="baseVal"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, int intVal, uint padAmount, char padChar, uint baseVal)
        {
            Debug.Assert(baseVal > 0 && baseVal <= 16);

            // Deal with negative numbers
            if (intVal < 0)
            {
                stringBuilder.Append('-');
                var uintVal = uint.MaxValue - (uint)intVal + 1; //< This is to deal with Int32.MinValue
                stringBuilder.Concat(uintVal, padAmount, padChar, baseVal);
            }
            else
            {
                stringBuilder.Concat((uint)intVal, padAmount, padChar, baseVal);
            }

            return stringBuilder;
        }

        /// <summary>
        ///     Convert a given signed integer value to a string and concatenate onto the stringbuilder. Assume no padding and base
        ///     ten.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="intVal"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, int intVal)
        {
            stringBuilder.Concat(intVal, 0, MsDefaultPadChar, 10);
            return stringBuilder;
        }

        /// <summary>
        ///     Convert a given signed integer value to a string and concatenate onto the stringbuilder. Assume base ten.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="intVal"></param>
        /// <param name="padAmount"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, int intVal, uint padAmount)
        {
            stringBuilder.Concat(intVal, padAmount, MsDefaultPadChar, 10);
            return stringBuilder;
        }

        /// <summary>
        ///     Convert a given signed integer value to a string and concatenate onto the stringbuilder. Assume base ten.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="intVal"></param>
        /// <param name="padAmount"></param>
        /// <param name="padChar"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, int intVal, uint padAmount, char padChar)
        {
            stringBuilder.Concat(intVal, padAmount, padChar, 10);
            return stringBuilder;
        }

        /// <summary>
        ///     Convert a given float value to a string and concatenate onto the stringbuilder
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="floatVal"></param>
        /// <param name="decimalPlaces"></param>
        /// <param name="padAmount"></param>
        /// <param name="padChar"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces, uint padAmount, char padChar)
        {
            if (decimalPlaces == 0)
            {
                // No decimal places, just round up and print it as an int

                // Agh, Math.Floor() just works on doubles/decimals. Don't want to cast! Let's do this the old-fashioned way.
                int intVal;
                if (floatVal >= 0.0f)
                {
                    // Round up
                    intVal = (int)(floatVal + 0.5f);
                }
                else
                {
                    // Round down for negative numbers
                    intVal = (int)(floatVal - 0.5f);
                }

                stringBuilder.Concat(intVal, padAmount, padChar, 10);
            }
            else
            {
                var intPart = (int)floatVal;

                // First part is easy, just cast to an integer
                stringBuilder.Concat(intPart, padAmount, padChar, 10);

                // Decimal point
                stringBuilder.Append('.');

                // Work out remainder we need to print after the d.p.
                var remainder = Math.Abs(floatVal - intPart);

                // ACM: Fix for leading zeros in the decimal portion
                remainder *= 10;
                decimalPlaces--;

                while (decimalPlaces > 0 && (uint)remainder % 10 == 0)
                {
                    remainder *= 10;
                    decimalPlaces--;
                    stringBuilder.Append('0');
                }

                // Multiply up to become an int that we can print
                while (decimalPlaces > 0)
                {
                    remainder *= 10;
                    decimalPlaces--;
                }

                // Round up. It's guaranteed to be a positive number, so no extra work required here.
                remainder += 0.5f;

                // All done, print that as an int!
                stringBuilder.Concat((uint)remainder, 0, '0', 10);
            }
            return stringBuilder;
        }

        /// <summary>
        ///     Convert a given float value to a string and concatenate onto the stringbuilder. Assumes five decimal places, and no
        ///     padding.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="floatVal"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, float floatVal)
        {
            stringBuilder.Concat(floatVal, MsDefaultDecimalPlaces, 0, MsDefaultPadChar);
            return stringBuilder;
        }

        /// <summary>
        ///     Convert a given float value to a string and concatenate onto the stringbuilder. Assumes no padding.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="floatVal"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces)
        {
            stringBuilder.Concat(floatVal, decimalPlaces, 0, MsDefaultPadChar);
            return stringBuilder;
        }

        /// <summary>
        ///     Convert a given float value to a string and concatenate onto the stringbuilder.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="floatVal"></param>
        /// <param name="decimalPlaces"></param>
        /// <param name="padAmount"></param>
        /// <returns></returns>
        public static StringBuilder Concat(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces, uint padAmount)
        {
            stringBuilder.Concat(floatVal, decimalPlaces, padAmount, MsDefaultPadChar);
            return stringBuilder;
        }
    }
}