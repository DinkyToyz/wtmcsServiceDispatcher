using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Vehicle action result.
    /// </summary>
    public struct VehicleResult
    {
        /// <summary>
        /// The result value.
        /// </summary>
        private readonly Result resultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleResult"/> struct.
        /// </summary>
        /// <param name="result">The result.</param>
        public VehicleResult(Result result)
        {
            this.resultValue = result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleResult"/> struct.
        /// </summary>
        /// <param name="result">if set to <c>true</c> [result].</param>
        public VehicleResult(bool result)
        {
            this.resultValue = result ? Result.Unaffected : Result.Ignored;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleResult" /> struct.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="result">if set to <c>true</c> [result].</param>
        public VehicleResult(bool success, Result result)
        {
            this.resultValue = success ? result : Result.Failure;
        }

        /// <summary>
        /// Possible vehicle action results.
        /// </summary>
        [Flags]
        public enum Result
        {
            /// <summary>
            /// No result.
            /// </summary>
            None = 0,

            /// <summary>
            /// The action failed.
            /// </summary>
            Failure = 1,

            /// <summary>
            /// The action request was ignored.
            /// </summary>
            Ignored = 1 << 1,

            /// <summary>
            /// The vehicle was unaffected.
            /// </summary>
            Unaffected = 1 << 2,

            /// <summary>
            /// The vehicle was deassigned.
            /// </summary>
            DeAssigned = 1 << 3,

            /// <summary>
            /// The vehicle was recalled.
            /// </summary>
            Recalled = 1 << 4,

            /// <summary>
            /// The vehicle was despawned.
            /// </summary>
            DeSpawned = 1 << 5,

            /// <summary>
            /// The vehicle was created.
            /// </summary>
            Created = 1 << 6,

            /// <summary>
            /// The vehicle was assigned.
            /// </summary>
            Assigned = 1 << 7
        }

        /// <summary>
        /// Gets a value indicating whether the vehicle was deassigned.
        /// </summary>
        /// <value>
        ///   <c>true</c> if deassigned; otherwise, <c>false</c>.
        /// </value>
        public bool DeAssigned => (this.resultValue & (Result.DeAssigned | Result.DeSpawned | Result.Recalled)) != Result.None;

        /// <summary>
        /// Gets a value indicating whether the vehicle was despawned.
        /// </summary>
        /// <value>
        ///   <c>true</c> if despawned; otherwise, <c>false</c>.
        /// </value>
        public bool DeSpawned => (this.resultValue & Result.DeSpawned) == Result.DeSpawned;

        /// <summary>
        /// Gets a value indicating whether this <see cref="VehicleResult"/> is a failure.
        /// </summary>
        /// <value>
        ///   <c>true</c> if failure; otherwise, <c>false</c>.
        /// </value>
        public bool Failure => (this.resultValue & (Result.Failure | Result.Ignored)) != Result.None;

        /// <summary>
        /// Gets a value indicating whether this <see cref="VehicleResult"/> is a success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        public bool Success => (this.resultValue & (Result.Failure | Result.Ignored)) == Result.None;

        /// <summary>
        /// Performs an implicit conversion from <see cref="VehicleResult"/> to <see cref="System.Boolean"/>.
        /// </summary>
        /// <param name="vehicleResult">The vehicle result.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator bool(VehicleResult vehicleResult)
        {
            return vehicleResult.Success;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Boolean"/> to <see cref="VehicleResult"/>.
        /// </summary>
        /// <param name="result">if set to <c>true</c> returns Unaffected; otherwise returns Ignored.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator VehicleResult(bool result)
        {
            return new VehicleResult(result);
        }

        /// <summary>
        /// Implements the operator false.
        /// </summary>
        /// <param name="vehicleResult">The vehicle result.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator false(VehicleResult vehicleResult)
        {
            return vehicleResult.Failure;
        }

        /// <summary>
        /// Implements the operator true.
        /// </summary>
        /// <param name="vehicleResult">The vehicle result.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator true(VehicleResult vehicleResult)
        {
            return vehicleResult.Success;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return (this.Failure ? "Failure" : this.Success ? "Success" : "Undefined") + "(" + this.resultValue.ToString() + ")";
        }
    }
}