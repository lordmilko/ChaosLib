﻿using System.Text;

namespace ChaosLib.Metadata
{
    public interface ISigMethod
    {
        string Name { get; }

        /// <summary>
        /// Gets the calling convention of the method.
        /// </summary>
        CallingConvention CallingConvention { get; }

        ISigType RetType { get; }

        ISigParameter[] Parameters { get; }
    }

    public abstract class SigMethod : ISigMethod
    {
        public string Name { get; }

        /// <inheritdoc />
        public CallingConvention CallingConvention { get; }

        public ISigType RetType { get; }

        public ISigParameter[] Parameters { get; }

        protected SigMethod(string name, CallingConvention callingConvention, SigType retType, ISigParameter[] methodParams)
        {
            Name = name;
            CallingConvention = callingConvention;
            RetType = retType;
            Parameters = methodParams;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(RetType).Append(" ").Append(Name);

            builder.Append("(");

            for (var i = 0; i < Parameters.Length; i++)
            {
                builder.Append(Parameters[i]);

                if (i < Parameters.Length - 1)
                    builder.Append(", ");
            }

            builder.Append(")");

            return builder.ToString();
        }
    }
}
