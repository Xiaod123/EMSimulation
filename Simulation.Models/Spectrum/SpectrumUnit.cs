﻿using Simulation.Models.Enums;
using Simulation.Models.Extensions;

namespace Simulation.Models.Spectrum
{
    /// <summary>
    /// The SpectrumParameter class.
    /// </summary>
    public class SpectrumUnit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumUnit"/> class.
        /// </summary>
        /// <param name="value">The double value.</param>
        /// <param name="type">The underlying type.</param>
        public SpectrumUnit(double value, SpectrumUnitType type)
        {
            this.value = value;
            this.type = type;
        }

        private readonly SpectrumUnitType type;

        private readonly double value;

        /// <summary>
        /// Converts value to specified type.
        /// </summary>
        /// <param name="toType">To spectrum parameter type.</param>
        /// <returns>The value of spectrum parameter type.</returns>
        public double ToType(SpectrumUnitType toType)
        {
            return SpectrumUnitConverter.Convert(this.value, this.type, toType);
        }

        protected bool Equals(SpectrumUnit other)
        {
            return this.type == other.type && this.value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof (SpectrumUnit))
            {
                return false;
            }
            return this.Equals((SpectrumUnit) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) this.type * 397) ^ this.value.GetHashCode();
            }
        }
    }
}
