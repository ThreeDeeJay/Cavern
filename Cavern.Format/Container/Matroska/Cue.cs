﻿using System.IO;

namespace Cavern.Format.Container.Matroska {
    /// <summary>
    /// Contains seeking info in a Matroska file.
    /// </summary>
    internal class Cue {
        /// <summary>
        /// Scaled timestamp of the seek position.
        /// </summary>
        public long Time { get; }

        /// <summary>
        /// ID (not index) of the <see cref="Track"/> in <see cref="ContainerReader.Tracks"/>.
        /// </summary>
        public long Track { get; }

        /// <summary>
        /// First byte of the cluster to read from.
        /// </summary>
        public long Position { get; }

        /// <summary>
        /// Contains seeking info in a Matroska file.
        /// </summary>
        public Cue(long time, long track, long position) {
            Time = time;
            Track = track;
            Position = position;
        }

        /// <summary>
        /// Read the cues from the root cue node.
        /// </summary>
        public static Cue[] GetCues(MatroskaTree segment, Stream reader) {
            MatroskaTree cues = segment.GetChild(reader, MatroskaTree.Segment_Cues);
            if (cues == null)
                return new Cue[0];

            segment.Position(reader);
            long offset = reader.Position;

            MatroskaTree[] sources = cues.GetChildren(reader, MatroskaTree.Segment_Cues_CuePoint);
            Cue[] results = new Cue[sources.Length];
            for (int i = 0; i < sources.Length; ++i) {
                long time = sources[i].GetChildValue(reader, MatroskaTree.Segment_Cues_CuePoint_CueTime);
                MatroskaTree position = sources[i].GetChild(reader, MatroskaTree.Segment_Cues_CuePoint_CueTrackPositions);
                results[i] = new Cue(
                    time, position.GetChildValue(reader, MatroskaTree.Segment_Cues_CuePoint_CueTrackPositions_CueTrack),
                    offset +
                    position.GetChildValue(reader, MatroskaTree.Segment_Cues_CuePoint_CueTrackPositions_CueClusterPosition)
                );
            }
            return results;
        }

        /// <summary>
        /// Get the cue that holds info for the timestamp to seek to.
        /// </summary>
        public static Cue Find(Cue[] cues, long targetTime) {
            if (cues == null)
                return null;
            int result = 0;
            while (result < cues.Length) {
                if (cues[result].Time > targetTime)
                    break;
                ++result;
            }
            --result;
            return result >= 0 ? cues[result] : null;
        }

        /// <summary>
        /// Provides basic information about the cue.
        /// </summary>
        public override string ToString() => $"Matroska cue, track {Track}, cluster byte: {Position}";
    }
}