﻿using System.IO;

using Cavern.Format.Utilities;

using static Cavern.Format.Consts.MP4Consts;

namespace Cavern.Format.Container.MP4 {
    /// <summary>
    /// Track metadata block of an MP4 container.
    /// </summary>
    /// <see href="https://developer.apple.com/library/archive/documentation/QuickTime/QTFF/QTFFChap2/qtff2.html"/>
    internal class TrackBox : NestedBox {
        /// <summary>
        /// Partially parsed track metadata. Has to be filled by the root parser.
        /// </summary>
        public MP4Track Track { get; }

        /// <summary>
        /// Track metadata block of an MP4 container.
        /// </summary>
        public TrackBox(uint length, Stream reader) : base(length, trackBox, reader) {
            Box headerBox = this[trackHeaderBox];
            if (headerBox == null) {
                ThrowCorruption(trackHeaderBox);
            }
            NestedBox mediaMeta = (NestedBox)this[mediaBox];
            if (mediaMeta == null) {
                ThrowCorruption(mediaBox);
            }

            byte[] mediaHeader = mediaMeta[mediaHeaderBox]?.GetRawData(reader);
            if (mediaHeader == null) {
                ThrowCorruption(mediaHeaderBox);
            }

            byte[] trackHeader = headerBox.GetRawData(reader);
            Track = new MP4Track((mediaMeta[mediaInfoBox] as NestedBox)?[sampleTableBox] as NestedBox, mediaHeader.ReadUInt32BE(12)) {
                ID = trackHeader.ReadInt32BE(12)
            };

            LanguageCode languageCode = (LanguageCode)mediaHeader.ReadUInt16BE(20);
            if (languageCode < LanguageCode.Unspecified) {
                Track.Language = languageCode.ToString();
            }
        }
    }
}