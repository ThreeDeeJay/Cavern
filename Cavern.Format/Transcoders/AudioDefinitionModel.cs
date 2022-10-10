﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using Cavern.Format.Common;
using Cavern.Format.Transcoders.AudioDefinitionModelElements;
using Cavern.Format.Utilities;

namespace Cavern.Format.Transcoders {
    /// <summary>
    /// An XML file with channel and object information.
    /// </summary>
    public sealed class AudioDefinitionModel : IXmlSerializable {
        /// <summary>
        /// Complete presentations.
        /// </summary>
        public IReadOnlyList<ADMProgramme> Programs { get; private set; }

        /// <summary>
        /// Object groupings.
        /// </summary>
        public IReadOnlyList<ADMContent> Contents { get; private set; }

        /// <summary>
        /// Single/multitrack object roots.
        /// </summary>
        public IReadOnlyList<ADMObject> Objects { get; private set; }

        /// <summary>
        /// Object categorizers.
        /// </summary>
        public IReadOnlyList<ADMPackFormat> PackFormats { get; private set; }

        /// <summary>
        /// Channel positions and object movements.
        /// </summary>
        public IReadOnlyList<ADMChannelFormat> ChannelFormats { get; private set; }

        /// <summary>
        /// Format information of each discrete audio source.
        /// </summary>
        public IReadOnlyList<ADMTrack> Tracks { get; private set; }

        /// <summary>
        /// Coding information of each discrete audio source.
        /// </summary>
        public IReadOnlyList<ADMTrackFormat> TrackFormats { get; private set; }

        /// <summary>
        /// Merging of format information elements.
        /// </summary>
        public IReadOnlyList<ADMStreamFormat> StreamFormats { get; private set; }

        /// <summary>
        /// Positional data for all channels/objects.
        /// </summary>
        public IReadOnlyList<ADMChannelFormat> Movements => movements;
        IReadOnlyList<ADMChannelFormat> movements = new List<ADMChannelFormat>();

        /// <summary>
        /// Only read what's absolutely needed for rendering, optimizing memory use but breaking transcodability.
        /// </summary>
        readonly bool minimal;

        /// <summary>
        /// Parses an XML file with channel and object information.
        /// </summary>
        /// <param name="reader">Stream to read the AXML from</param>
        /// <param name="length">Length of the AXML stream in bytes</param>
        /// <param name="minimal">Only read what's absolutely needed for rendering,
        /// optimizing memory use but breaking transcodability</param>
        public AudioDefinitionModel(Stream reader, long length, bool minimal) {
            this.minimal = minimal;
            byte[] data = new byte[length];
            reader.Read(data, 0, length);
            File.WriteAllBytes("X:\\xd.xml", data);
            using XmlReader xmlReader = XmlReader.Create(new MemoryStream(data));
            ReadXml(xmlReader);
        }

        /// <summary>
        /// Creates an ADM for export by a program list created in code.
        /// </summary>
        /// <param name="programs">Complete presentations</param>
        /// <param name="contents">Object groupings</param>
        /// <param name="objects">Single/multitrack object roots</param>
        /// <param name="packFormats">Object categorizers</param>
        /// <param name="channelFormats">Channel positions and object movements in the order of the source tracks</param>
        /// <param name="tracks">Coding information of a track</param>
        /// <param name="trackFormats">Name, format, and reference information of a track</param>
        /// <param name="streamFormats">Merging of format information elements</param>
        public AudioDefinitionModel(IReadOnlyList<ADMProgramme> programs, IReadOnlyList<ADMContent> contents,
            IReadOnlyList<ADMObject> objects, IReadOnlyList<ADMPackFormat> packFormats,
            IReadOnlyList<ADMChannelFormat> channelFormats, IReadOnlyList<ADMTrack> tracks,
            IReadOnlyList<ADMTrackFormat> trackFormats, IReadOnlyList<ADMStreamFormat> streamFormats) {
            Programs = programs;
            Contents = contents;
            Objects = objects;
            PackFormats = packFormats;
            ChannelFormats = channelFormats;
            Tracks = tracks;
            TrackFormats = trackFormats;
            StreamFormats = streamFormats;
            movements = channelFormats;
        }

        /// <summary>
        /// Change object order to reference the BWF file's correct channels.
        /// </summary>
        public void Assign(ChannelAssignment chna) {
            List<ADMChannelFormat> assignment = new List<ADMChannelFormat>(movements.Count);
            for (int i = 0; i < chna.Assignment.Length; i++) {
                ADMChannelFormat part = FindMovement(chna.Assignment[i]);
                if (part != null) {
                    assignment.Add(part);
                } else {
                    return;
                }
            }
            if (movements.Count == assignment.Count) {
                movements = assignment;
            }
        }

        /// <summary>
        /// Extracts the ADM metadata from an XML file.
        /// </summary>
        public void ReadXml(XmlReader reader) {
            List<ADMProgramme> programs = new List<ADMProgramme>();
            List<ADMContent> contents = new List<ADMContent>();
            List<ADMObject> objects = new List<ADMObject>();
            List<ADMPackFormat> packFormats = new List<ADMPackFormat>();
            List<ADMChannelFormat> channelFormats = new List<ADMChannelFormat>();
            List<ADMTrack> tracks = new List<ADMTrack>();
            List<ADMTrackFormat> trackFormats = new List<ADMTrackFormat>();
            List<ADMStreamFormat> streamFormats = new List<ADMStreamFormat>();
            Programs = programs;
            Contents = contents;
            Objects = objects;
            PackFormats = packFormats;
            ChannelFormats = channelFormats;
            Tracks = tracks;
            TrackFormats = trackFormats;
            StreamFormats = streamFormats;
            movements = channelFormats;

            XDocument doc = XDocument.Load(reader);
            IEnumerable<XElement> descendants = doc.Descendants();
            using IEnumerator<XElement> enumerator = descendants.GetEnumerator();

            if (minimal &&false) {
                while (enumerator.MoveNext()) {
                    switch (enumerator.Current.Name.LocalName) {
                        case ADMTags.channelFormatTag:
                            channelFormats.Add(new ADMChannelFormat(enumerator.Current));
                            break;
                        case ADMTags.streamFormatTag:
                            streamFormats.Add(new ADMStreamFormat(enumerator.Current));
                            break;
                    }
                }
                return;
            }

            while (enumerator.MoveNext()) {
                switch (enumerator.Current.Name.LocalName) {
                    case ADMTags.programTag:
                        programs.Add(new ADMProgramme(enumerator.Current));
                        break;
                    case ADMTags.contentTag:
                        contents.Add(new ADMContent(enumerator.Current));
                        break;
                    case ADMTags.objectTag:
                        objects.Add(new ADMObject(enumerator.Current));
                        break;
                    case ADMTags.packFormatTag:
                        packFormats.Add(new ADMPackFormat(enumerator.Current));
                        break;
                    case ADMTags.channelFormatTag:
                        channelFormats.Add(new ADMChannelFormat(enumerator.Current));
                        break;
                    case ADMTags.trackTag:
                        tracks.Add(new ADMTrack(enumerator.Current));
                        break;
                    case ADMTags.trackFormatTag:
                        trackFormats.Add(new ADMTrackFormat(enumerator.Current));
                        break;
                    case ADMTags.streamFormatTag:
                        streamFormats.Add(new ADMStreamFormat(enumerator.Current));
                        break;
                }
            }
        }

        /// <summary>
        /// Writes the ADM metadata to an XML file.
        /// </summary>
        public void WriteXml(XmlWriter writer) {
            XNamespace xmlns = XNamespace.Get(ADMTags.rootNamespace);
            XNamespace xsi = XNamespace.Get(ADMTags.instanceNamespace);
            XElement root = new XElement(xmlns + ADMTags.rootTag,
                new XAttribute(XNamespace.Xmlns + ADMTags.instanceNamespaceAttribute, xsi),
                new XAttribute(xsi + ADMTags.schemaLocationAttribute, ADMTags.rootNamespace + ADMTags.schemaLocation),
                new XAttribute(XNamespace.Xml + ADMTags.languageAttribute, ADMTags.language));
            XDocument doc = new XDocument(root);
            for (int i = 0; i < ADMTags.subTags.Length; i++) {
                XElement subTag = new XElement(xmlns + ADMTags.subTags[i]);
                root.Add(subTag);
                root = subTag;
            }

            SerializeGroup(Programs, root);
            SerializeGroup(Contents, root);
            SerializeGroup(Objects, root);
            SerializeGroup(PackFormats, root);
            SerializeGroup(ChannelFormats, root);
            SerializeGroup(Tracks, root);
            SerializeGroup(TrackFormats, root);
            SerializeGroup(StreamFormats, root);

            doc.WriteTo(writer);
        }

        /// <summary>
        /// Null by definition.
        /// </summary>
        public XmlSchema GetSchema() => null;

        /// <summary>
        /// Find a movement information by channel assignment data.
        /// </summary>
        ADMChannelFormat FindMovement(Tuple<short, string> assignment) {
            for (int i = 0, c = StreamFormats.Count; i < c; i++) {
                if (assignment.Item2.Contains(StreamFormats[i].TrackFormat + StreamFormats[i].PackFormat)) {
                    for (int j = 0, c2 = ChannelFormats.Count; j < c2; j++) {
                        if (ChannelFormats[j].ID.Equals(StreamFormats[i].ChannelFormat)) {
                            return ChannelFormats[j];
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Exports all elements of a group.
        /// </summary>
        void SerializeGroup(IReadOnlyList<IXDocumentSerializable> from, XElement to) {
            XNamespace ns = to.Name.Namespace;
            IEnumerator<IXDocumentSerializable> enumerator = from.GetEnumerator();
            while (enumerator.MoveNext()) {
                to.Add(enumerator.Current.Serialize(ns));
            }
        }
    }
}