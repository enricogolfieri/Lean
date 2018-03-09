﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using Newtonsoft.Json;
using QuantConnect.Algorithm.Framework.Alphas.Serialization;

namespace QuantConnect.Algorithm.Framework.Alphas
{
    /// <summary>
    /// Defines a prediction alpha for a single symbol generated by the algorithm
    /// </summary>
    /// <remarks>
    /// Serialization of this type is delegated to the <see cref="InsightJsonConverter"/> which uses the <see cref="SerializedInsight"/> as a model.
    /// </remarks>
    [JsonConverter(typeof(InsightJsonConverter))]
    public class Alpha : IEquatable<Alpha>
    {
        /// <summary>
        /// Gets the unique identifier for this alpha
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the utc time this alpha was generated
        /// </summary>
        /// <remarks>
        /// The algorithm framework handles setting this value appropriately.
        /// If providing custom <see cref="Alpha"/> implementation, be sure
        /// to set this value to algorithm.UtcTime when the alpha is generated.
        /// </remarks>
        public DateTime GeneratedTimeUtc { get; internal set; }

        /// <summary>
        /// Gets the alpha's prediction end time. This is the time when this
        /// alpha prediction is expected to be fulfilled. This time takes into
        /// account market hours, weekends, as well as the symbol's data resolution
        /// </summary>
        public DateTime CloseTimeUtc { get; internal set; }

        /// <summary>
        /// Gets the symbol this alpha is for
        /// </summary>
        public Symbol Symbol { get; private set; }

        /// <summary>
        /// Gets the type of alpha, for example, price alpha or volatility alpha
        /// </summary>
        public AlphaType Type { get; private set; }

        /// <summary>
        /// Gets the reference value this alpha is predicting against. The value is dependent on the specified <see cref="AlphaType"/>
        /// </summary>
        public decimal ReferenceValue { get; internal set; }

        /// <summary>
        /// Gets the predicted direction, down, flat or up
        /// </summary>
        public AlphaDirection Direction { get; private set; }

        /// <summary>
        /// Gets the period over which this alpha is expected to come to fruition
        /// </summary>
        public TimeSpan Period { get; private set; }

        /// <summary>
        /// Gets the predicted percent change in the alpha type (price/volatility)
        /// </summary>
        public double? Magnitude { get; private set; }

        /// <summary>
        /// Gets the confidence in this alpha
        /// </summary>
        public double? Confidence { get; private set; }

        /// <summary>
        /// Gets the most recent scores for this alpha
        /// </summary>
        public AlphaScore Score { get; private set; }

        /// <summary>
        /// Gets the estimated value of this alpha in the account currency
        /// </summary>
        public decimal EstimatedValue { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Alpha"/> class
        /// </summary>
        /// <param name="symbol">The symbol this alpha is for</param>
        /// <param name="type">The type of alpha, price/volatility</param>
        /// <param name="direction">The predicted direction</param>
        /// <param name="period">The period over which the prediction will come true</param>
        public Alpha(Symbol symbol, AlphaType type, AlphaDirection direction, TimeSpan period)
            : this(symbol, type, direction, period, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Alpha"/> class
        /// </summary>
        /// <param name="symbol">The symbol this alpha is for</param>
        /// <param name="type">The type of alpha, price/volatility</param>
        /// <param name="direction">The predicted direction</param>
        /// <param name="period">The period over which the prediction will come true</param>
        /// <param name="magnitude">The predicted magnitude as a percentage change</param>
        /// <param name="confidence">The confidence in this alpha</param>
        public Alpha(Symbol symbol, AlphaType type, AlphaDirection direction, TimeSpan period, double? magnitude, double? confidence)
        {
            Id = Guid.NewGuid();
            Score = new AlphaScore();

            Symbol = symbol;
            Type = type;
            Direction = direction;
            Period = period;

            // Optional
            Magnitude = magnitude;
            Confidence = confidence;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Alpha"/> class.
        /// This constructor is provided mostly for testing purposes. When running inside an algorithm,
        /// the generated and close times are set based on the algorithm's time.
        /// </summary>
        /// <param name="generatedTimeUtc">The time this alpha was generated in utc</param>
        /// <param name="symbol">The symbol this alpha is for</param>
        /// <param name="type">The type of alpha, price/volatility</param>
        /// <param name="direction">The predicted direction</param>
        /// <param name="period">The period over which the prediction will come true</param>
        /// <param name="magnitude">The predicted magnitude as a percentage change</param>
        /// <param name="confidence">The confidence in this alpha</param>
        public Alpha(DateTime generatedTimeUtc, Symbol symbol, AlphaType type, AlphaDirection direction, TimeSpan period, double? magnitude, double? confidence)
            : this(symbol,type, direction, period, magnitude, confidence)
        {
            GeneratedTimeUtc = generatedTimeUtc;
            CloseTimeUtc = generatedTimeUtc + period;
        }

        /// <summary>
        /// Creates a deep clone of this alpha instance
        /// </summary>
        /// <returns>A new alpha with identical values, but new instances</returns>
        public Alpha Clone()
        {
            return new Alpha(Symbol, Type, Direction, Period, Magnitude, Confidence)
            {
                GeneratedTimeUtc = GeneratedTimeUtc,
                CloseTimeUtc = CloseTimeUtc,
                Score = Score,
                Id = Id,
                EstimatedValue = EstimatedValue,
                ReferenceValue = ReferenceValue
            };
        }

        /// <summary>
        /// Creates a new alpha for predicting the percent change in price over the specified period
        /// </summary>
        /// <param name="symbol">The symbol this alpha is for</param>
        /// <param name="period">The period over which the prediction will come true</param>
        /// <param name="magnitude">The predicted magnitude as a percent change</param>
        /// <param name="confidence">The confidence in this alpha</param>
        /// <returns>A new alpha object for the specified parameters</returns>
        public static Alpha PriceMagnitude(Symbol symbol, double magnitude, TimeSpan period, double? confidence = null)
        {
            var direction = (AlphaDirection) Math.Sign(magnitude);
            return new Alpha(symbol, AlphaType.Price, direction, period, magnitude, confidence);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            var str = $"{Id}: {Symbol} {Type} {Direction} within {Period}";
            if (Magnitude.HasValue)
            {
                str += $" by {Magnitude.Value}%";
            }
            if (Confidence.HasValue)
            {
                str += $" with {Math.Round(100 * Confidence.Value, 1)}% confidence";
            }

            return str;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Alpha other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (Id == other.Id) return true;

            return Equals(Symbol, other.Symbol) &&
                Direction == other.Direction &&
                Type == other.Type &&
                Confidence.Equals(other.Confidence) &&
                Magnitude.Equals(other.Magnitude) &&
                Period.Equals(other.Period);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Alpha)obj);
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Symbol != null ? Symbol.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Type;
                hashCode = (hashCode * 397) ^ (int)Direction;
                hashCode = (hashCode * 397) ^ Magnitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Confidence.GetHashCode();
                hashCode = (hashCode * 397) ^ Period.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines if the two alphas are equal
        /// </summary>
        public static bool operator ==(Alpha left, Alpha right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines if the two alphas are not equal
        /// </summary>
        public static bool operator !=(Alpha left, Alpha right)
        {
            return !Equals(left, right);
        }
    }
}