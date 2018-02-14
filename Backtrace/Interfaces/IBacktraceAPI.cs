using System;
using System.Collections.Generic;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Interfaces
{
    /// <summary>
    /// Backtrace API sender instsance interface
    /// </summary>
    /// <typeparam name="T">message type</typeparam>
    public interface IBacktraceApi<T>
    {
        /// <summary>
        /// Send a data to Backtrace API
        /// </summary>
        /// <param name="data">Data to send </param>
        /// <returns>True if report was sended to Backtrace API without any exception</returns>
        bool Send(Model.BacktraceData<T> data);
        
    }
}
