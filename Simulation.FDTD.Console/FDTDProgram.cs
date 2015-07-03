﻿using System;
using System.Linq;

using Simulation.FDTD.Models;
using Simulation.Infrastructure;
using Simulation.Medium.Medium;
using Simulation.Medium.MediumSolver;
using Simulation.Medium.Models;
using Simulation.Models.Common;
using Simulation.Models.Constants;
using Simulation.Models.Coordinates;
using Simulation.Models.Enums;
using Simulation.Models.Extensions;
using Simulation.Models.Spectrum;

namespace Simulation.FDTD.Console
{
    internal class FDTDProgram
    {
        private static void Main(string[] args)
        {
            var result = Calculate();
            SimpleFormatter.Write(
                "rezult_ext.txt",
                result.ToDictionary(
                    x => x.Key.ToType(SpectrumUnitType.WaveLength),
                    x => x.Value.CrossSectionExtinction));
        }

        public static SimulationResultDictionary Calculate()
        {
            var ext = new FDTDSimulation();

            SimulationParameters parameters = new SimulationParameters
            {
                Indices = new IndexStore(50, 50, 50),
                CellSize = 1e-9,
                Spectrum =
                    new OpticalSpectrum(new LinearDiscreteCollection(300e-9, 700e-9, 100), SpectrumUnitType.WaveLength),
                CourantNumber = 0.5,
                PmlLength = 7,
                NumSteps = 50,
                WaveFunc = time => Math.Exp(-0.5 * Math.Pow((30 - time) / 5.0, 2.0)),
            };

            setMedium(parameters);

            return ext.Calculate(parameters);
        }

        private static void setMedium(SimulationParameters parameters)
        {
            double timeStep = parameters.CellSize * parameters.CourantNumber / (Fundamentals.SpeedOfLight);
            var vacuum = new VacuumSolver();
            var silver = new DrudeLorentz();
            double radius = 10;

            var center = new CartesianCoordinate(
                parameters.Indices.ILength / 2.0,
                parameters.Indices.JLength / 2.0,
                parameters.Indices.KLength / 2.0);

            parameters.Medium = parameters.Indices.CreateArray<IMediumSolver>(
                (i, j, k) =>
                {
                    var point = new CartesianCoordinate(i, j, k) - center;
                    if (point.Norm <= radius)
                    {
                        return new DrudeLorentzSolver(silver, timeStep) { IsBody = true };
                    }
                    return vacuum;
                });
        }
    }
}