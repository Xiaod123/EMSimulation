﻿using System;
using System.Collections.Generic;
using System.Numerics;

using Simulation.Medium.Models;
using Simulation.Models.Enums;
using Simulation.Models.Spectrum;

namespace Simulation.DDA
{
    /// <summary>
    /// The OpticalConstants class.
    /// </summary>
    public class OpticalConstants : BaseMedium
    {
        /// <summary>
        /// The wave length multiplier
        /// </summary>
        public const double WaveLengthMultiplier = 1e9;

        /// <summary>
        /// Gets the wave length list.
        /// </summary>
        /// <value>
        /// The wave length list.
        /// </value>
        public List<double> WaveLengthList { get; private set; }

        /// <summary>
        /// Gets the permittivity list.
        /// </summary>
        /// <value>
        /// The permittivity list.
        /// </value>
        public List<Complex> PermittivityList { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpticalConstants" /> class.
        /// </summary>
        /// <param name="waveLengthList">The wave length list.</param>
        /// <param name="permittivityList">The permittivity list.</param>
        /// <exception cref="System.ArgumentException">Lists have different count.</exception>
        public OpticalConstants(List<double> waveLengthList, List<Complex> permittivityList)
        {
            if (waveLengthList.Count != permittivityList.Count)
            {
                throw new ArgumentException("Lists have different count.");
            }

            this.WaveLengthList = waveLengthList;
            this.PermittivityList = permittivityList;
        }

        private Tuple<int, int> getNearestIndexes(double waveLength)
        {
            int i;
            for (i = 0; i < this.WaveLengthList.Count - 1; i++)
            {
                if (waveLength >= this.WaveLengthList[i] && waveLength < this.WaveLengthList[i + 1])
                {
                    return new Tuple<int, int>(i, i + 1);
                }
            }
            return new Tuple<int, int>(this.WaveLengthList.Count - 1, this.WaveLengthList.Count - 1);
        }

        /// <summary>
        /// Gets the permittivity.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The complex permittivity.</returns>
        public override Complex GetPermittivity(SpectrumUnit parameter)
        {
            var waveLength = parameter.ToType(SpectrumUnitType.WaveLength) * WaveLengthMultiplier;
            var tuple = this.getNearestIndexes(waveLength);
            var lower = tuple.Item1;
            var upper = tuple.Item2;

            var deltaWaveLength = waveLength - this.WaveLengthList[lower];
            var stepWaveLength = this.WaveLengthList[upper] - this.WaveLengthList[lower];
            var coefWaveLength = deltaWaveLength / stepWaveLength;

            double epsRe = this.PermittivityList[lower].Real + coefWaveLength *
                           (this.PermittivityList[upper].Real - this.PermittivityList[lower].Real);

            double epsIm = this.PermittivityList[lower].Imaginary + coefWaveLength *
                           (this.PermittivityList[upper].Imaginary - this.PermittivityList[lower].Imaginary);

            return new Complex(epsRe, epsIm);

            ////eps_re = 3.9943 - (13.29e+15*13.29e+15)/((4.0*Pi*Pi*3.0e8*3e8/WaveLength/WaveLength*1e18)+(0.1128e+15*0.1128e+15));
            ////eps_im = (13.29e+15*13.29e+15*0.1128e+15)/((4.0*Pi*Pi*3e8*3e8/WaveLength/WaveLength*1e18)+(0.1128e+15*0.1128e+15))/(2.0*Pi*3e8/WaveLength*1e9);
        }
    }
}