﻿using System;
using System.IO;
using VGMToolbox.util;

namespace VGMToolbox.format
{
    public class SonyPssStream : Mpeg2Stream
    {
        new public const string DefaultAudioExtension = ".ss2";
        new public const string DefaultVideoExtension = ".m2v";

        public SonyPssStream(string path)
            : base(path)
        {
            this.UsesSameIdForMultipleAudioTracks = true;
            base.BlockIdDictionary[BitConverter.ToUInt32(Mpeg2Stream.PacketStartBytes, 0)] = new BlockSizeStruct(PacketSizeType.Static, 0xE); // Pack Header
            base.BlockIdDictionary[BitConverter.ToUInt32(new byte[] { 0x00, 0x00, 0x01, 0xBD }, 0)] = new BlockSizeStruct(PacketSizeType.SizeBytes, 2); // Audio Stream, two bytes following equal length (Big Endian)
            base.BlockIdDictionary[BitConverter.ToUInt32(new byte[] { 0x00, 0x00, 0x01, 0xBF }, 0)] = new BlockSizeStruct(PacketSizeType.SizeBytes, 2); // Audio Stream, two bytes following equal length (Big Endian)
        }

        protected override int GetAudioPacketHeaderSize(Stream readStream, long currentOffset)
        {            
            return 0x11;
        }

        protected override byte GetStreamId(Stream readStream, long currentOffset)
        {
            byte[] streamIdBytes;
            byte streamId;

            // old way
            //streamId = ParseFile.ParseSimpleOffset(readStream, currentOffset + 0x14, 1)[0];

            // new way
            streamIdBytes = ParseFile.ParseSimpleOffset(readStream, currentOffset + 0x14, 3);
            streamId = (byte)(streamIdBytes[0] + streamIdBytes[2]);

            return streamId;
        }

        protected override bool IsThisAnAudioBlock(byte[] blockToCheck)
        {
            return ((blockToCheck[3] >= 0xBD) && 
                    (blockToCheck[3] <= 0xDF) &&
                    (blockToCheck[3] != 0xBE));
        }
        protected override bool IsThisAVideoBlock(byte[] blockToCheck)
        {
            return ((blockToCheck[3] >= 0xE0) && (blockToCheck[3] <= 0xEF));
        }

        protected override string GetAudioFileExtension(Stream readStream, long currentOffset)
        {
            string fileExtension;
            byte streamId = this.GetStreamId(readStream, currentOffset);

            if ((streamId & 0xF0) == 0xA0)
            {
                fileExtension = DefaultAudioExtension;                
            }
            else
            {
                fileExtension = ".bin";
            }


            return fileExtension;
        }
    }
}
