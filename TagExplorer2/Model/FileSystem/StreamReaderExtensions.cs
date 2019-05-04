using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TagExplorer2.Model.FileSystem
{
    public static class StreamReaderExtensions
    {
        readonly static FieldInfo charPosField = typeof(StreamReader).GetField("charPos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly);
        readonly static FieldInfo charLenField = typeof(StreamReader).GetField("charLen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly);
        readonly static FieldInfo charBufferField = typeof(StreamReader).GetField("charBuffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly);


        readonly static FieldInfo byteLenField = typeof(StreamReader).GetField("byteLen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly);
        readonly static FieldInfo byteBufferField = typeof(StreamReader).GetField("byteBuffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly);
        public static long GetPosition(this StreamReader reader)
        {


            // The current buffer of decoded characters
            char[] charBuffer = (char[])charBufferField.GetValue(reader);

            // The index of the next char to be read from charBuffer
            int charPos = (int)charPosField.GetValue(reader);

            // The number of decoded chars presently used in charBuffer
            int charLen = (int)charLenField.GetValue(reader);

            // The current buffer of read bytes (byteBuffer.Length = 1024; this is critical).
            byte[] byteBuffer = (byte[])byteBufferField.GetValue(reader);

            // The number of bytes read while advancing reader.BaseStream.Position to (re)fill charBuffer
            int byteLen = (int)byteLenField.GetValue(reader);

            // The number of bytes the remaining chars use in the original encoding.
            int numBytesLeft = reader.CurrentEncoding.GetByteCount(charBuffer, charPos, charLen - charPos);

            // For variable-byte encodings, deal with partial chars at the end of the buffer
            int numFragments = 0;
            if (byteLen > 0 && !reader.CurrentEncoding.IsSingleByte)
            {
                if (reader.CurrentEncoding.CodePage == 65001) // UTF-8
                {
                    byte byteCountMask = 0;
                    while ((byteBuffer[byteLen - numFragments - 1] >> 6) == 2) // if the byte is "10xx xxxx", it's a continuation-byte
                        byteCountMask |= (byte)(1 << ++numFragments); // count bytes & build the "complete char" mask
                    if ((byteBuffer[byteLen - numFragments - 1] >> 6) == 3) // if the byte is "11xx xxxx", it starts a multi-byte char.
                        byteCountMask |= (byte)(1 << ++numFragments); // count bytes & build the "complete char" mask
                                                                      // see if we found as many bytes as the leading-byte says to expect
                    if (numFragments > 1 && ((byteBuffer[byteLen - numFragments] >> 7 - numFragments) == byteCountMask))
                        numFragments = 0; // no partial-char in the byte-buffer to account for
                }
                else if (reader.CurrentEncoding.CodePage == 1200) // UTF-16LE
                {
                    if (byteBuffer[byteLen - 1] >= 0xd8) // high-surrogate
                        numFragments = 2; // account for the partial character
                }
                else if (reader.CurrentEncoding.CodePage == 1201) // UTF-16BE
                {
                    if (byteBuffer[byteLen - 2] >= 0xd8) // high-surrogate
                        numFragments = 2; // account for the partial character
                }
            }
            return reader.BaseStream.Position - numBytesLeft - numFragments;
        }
        public static long GetPosition2(this StreamReader reader)
        {
            //shift position back from BaseStream.Position by the number of bytes read
            //into internal buffer.
            int byteLen = (int)byteLenField.GetValue(reader);
            var position = reader.BaseStream.Position - byteLen;

            //if we have consumed chars from the buffer we need to calculate how many
            //bytes they represent in the current encoding and add that to the position.
            int charPos = (int)charPosField.GetValue(reader);
            if (charPos > 0)
            {
                var charBuffer = (char[])charBufferField.GetValue(reader);
                var encoding = reader.CurrentEncoding;
                var bytesConsumed = encoding.GetBytes(charBuffer, 0, charPos).Length;
                position += bytesConsumed;
            }




            return position;
        }

        public static void SetPosition(this StreamReader reader, long position)
        {
            reader.DiscardBufferedData();
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }
    }
}
