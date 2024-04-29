﻿using System.Collections.Generic;

using Cavern.Filters.Utilities;

namespace Cavern.Filters {
    /// <summary>
    /// Simple first-order lowpass filter.
    /// </summary>
    public class Lowpass : BiquadFilter {
        /// <inheritdoc/>
        public override BiquadFilterType FilterType => BiquadFilterType.Lowpass;

        /// <summary>
        /// Simple first-order lowpass filter with maximum flatness and no additional gain.
        /// </summary>
        /// <param name="sampleRate">Audio sample rate</param>
        /// <param name="centerFreq">Center frequency (-3 dB point) of the filter</param>
        public Lowpass(int sampleRate, double centerFreq) : base(sampleRate, centerFreq) { }

        /// <summary>
        /// Simple first-order lowpass filter with no additional gain.
        /// </summary>
        /// <param name="sampleRate">Audio sample rate</param>
        /// <param name="centerFreq">Center frequency (-3 dB point) of the filter</param>
        /// <param name="q">Q-factor of the filter</param>
        public Lowpass(int sampleRate, double centerFreq, double q) : base(sampleRate, centerFreq, q) { }

        /// <summary>
        /// Simple first-order lowpass filter.
        /// </summary>
        /// <param name="sampleRate">Audio sample rate</param>
        /// <param name="centerFreq">Center frequency (-3 dB point) of the filter</param>
        /// <param name="q">Q-factor of the filter</param>
        /// <param name="gain">Gain of the filter in decibels</param>
        public Lowpass(int sampleRate, double centerFreq, double q, double gain) : base(sampleRate, centerFreq, q, gain) { }

        /// <inheritdoc/>
        public override object Clone() => new Lowpass(SampleRate, centerFreq, q, gain);

        /// <inheritdoc/>
        public override object Clone(int sampleRate) => new Lowpass(sampleRate, centerFreq, q, gain);

        /// <inheritdoc/>
        public override void ExportToEqualizerAPO(List<string> wipConfig) {
            if (q == QFactor.reference) {
                wipConfig.Add($"Filter: ON LP Fc {centerFreq} Hz");
            } else {
                wipConfig.Add($"Filter: ON LPQ Fc {centerFreq} Hz Q {q}");
            }
        }

        /// <inheritdoc/>
        protected override void Reset(float cosW0, float alpha, float divisor) => SetupPass(cosW0, alpha, divisor, 1 - cosW0);
    }
}