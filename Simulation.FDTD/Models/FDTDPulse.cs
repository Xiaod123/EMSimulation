﻿using System;
using System.Numerics;

using Simulation.Models.Enums;
using Simulation.Models.Spectrum;

namespace Simulation.FDTD.Models
{
    /// <summary>
    /// The FDTDPulse class.
    /// </summary>
    public class FDTDPulse
    {
        private readonly double courantNumber;

        private readonly int medLength; // length of 1D medium

        private readonly Func<int, double> pulseFunc;

        private readonly OpticalSpectrum spectrum;

        private double eMh1;

        private double eMh2;

        private double eMl1;

        private double eMl2;

        /// <summary>
        /// Initializes a new instance of the <see cref="FDTDPulse"/> class.
        /// </summary>
        /// <param name="waveFunc">The wave function.</param>
        /// <param name="length">The length.</param>
        /// <param name="spectrum">The spectrum.</param>
        /// <param name="courantNumber">The courant number.</param>
        public FDTDPulse(Func<int, double> waveFunc, int length, OpticalSpectrum spectrum, double courantNumber)
        {
            this.courantNumber = courantNumber;
            this.medLength = 2 * length;
            this.E = new double[this.medLength]; // electric field
            this.H = new double[this.medLength]; // magnetic field

            this.eMh2 = this.eMh1 = 0.0;
            this.eMl2 = this.eMl1 = 0.0;

            this.pulseFunc = waveFunc;
            this.spectrum = spectrum;

            this.initFourier();
        }

        /// <summary>
        /// Gets or sets the electric component of pulse.
        /// </summary>
        /// <value>
        /// The electric component of pulse.
        /// </value>
        public double[] E { get; set; }

        /// <summary>
        /// Gets or sets the magnetized component of pulse.
        /// </summary>
        /// <value>
        /// The magnetized component of pulse.
        /// </value>
        public double[] H { get; set; }

        /// <summary>
        /// Gets or sets the fourier series of pulse.
        /// </summary>
        /// <value>
        /// The fourier series of pulse.
        /// </value>
        public FourierSeries<Complex>[] FourierPulse { get; set; }

        /// <summary>
        /// Calculate the electric field step.
        /// </summary>
        /// <param name="time">The time value.</param>
        public void ElectricFieldStepCalc(int time)
        {
            // E-field calculation
            for (int i = 1; i < this.medLength; i++)
            {
                this.E[i] += this.courantNumber * (this.H[i - 1] - this.H[i]);
            }

            double pulse = this.pulseFunc(time);

            this.E[2] = pulse;

            /*граничні умови*/
            this.E[this.medLength - 1] = this.eMh2;
            this.eMh2 = this.eMh1;
            this.eMh1 = this.E[this.medLength - 2];

            this.E[0] = this.eMl2;
            this.eMl2 = this.eMl1;
            this.eMl1 = this.E[1];

            // Подавлення випадкового шуму при обрахунку експоненти після проходження більшої
            // частини сигналу через досліджуванну область
            if (time >= 300)
            {
                this.eMh2 = 0.0;
                this.eMh1 = 0.0;
                this.eMl2 = 0.0;
                this.eMl1 = 0.0;

                for (int i = this.medLength / 2; i < this.medLength; i++)
                {
                    this.E[i] = 0.0;
                    this.H[i] = 0.0;
                }
            }
        }

        /// <summary>
        /// Magnetics the field step calculate.
        /// </summary>
        public void MagneticFieldStepCalc()
        {
            // H-field calculation
            for (int i = 0; i < this.medLength - 1; i++)
            {
                this.H[i] += this.courantNumber * (this.E[i] - this.E[i + 1]);
            }
        }

        /// <summary>
        /// Does the fourier pulse.
        /// </summary>
        /// <param name="time">The time step.</param>
        public void DoFourierPulse(double time)
        {
            // Fourier transform
            foreach (SpectrumUnit cycleFreq in this.spectrum)
            {
                double angle = cycleFreq.ToType(SpectrumUnitType.CycleFrequency) * time;
                for (int m = 0; m < this.medLength; m++)
                {
                    this.FourierPulse[m].Add(cycleFreq, Complex.FromPolarCoordinates(this.E[m], angle));
                }
            }
        }

        private void initFourier()
        {
            this.FourierPulse = new FourierSeries<Complex>[this.medLength];
            this.FourierPulse.Initialize();
        }
    }
}