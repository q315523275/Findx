using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Findx.DinkToPdf.Contracts;

namespace Findx.DinkToPdf;

public class SynchronizedConverter : BasicConverter
{
    Thread _conversionThread;

    readonly BlockingCollection<Task> _conversions = new BlockingCollection<Task>();

    bool _kill = false;

    private readonly object _startLock = new();

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="tools"></param>
    public SynchronizedConverter(ITools tools) : base(tools)
    {

    }

    public override byte[] Convert(IDocument document)
    {
        return Invoke(() => base.Convert(document));
    }

    public TResult Invoke<TResult>(Func<TResult> @delegate)
    {
        StartThread();

        var task = new Task<TResult>(@delegate);

        lock (task)
        {
            //add task to blocking collection
            _conversions.Add(task);

            //wait for task to be processed by conversion thread 
            Monitor.Wait(task);
        }

        //throw exception that happened during conversion
        if (task.Exception != null)
        {
            throw task.Exception;
        }

        return task.Result;
    }

    private void StartThread()
    {
        lock (_startLock)
        {
            if (_conversionThread == null)
            {
                _conversionThread = new Thread(Run)
                {
                    IsBackground = true,
                    Name = "wkhtmltopdf worker thread"
                };

                _kill = false;

                _conversionThread.Start();
            }
        }
    }
        
    private void StopThread()
    {
        lock (_startLock)
        {
            if (_conversionThread != null)
            {
                _kill = true;

                //CompleteAdding 使阻塞在 Take() 上的 worker 线程得以退出
                _conversions.CompleteAdding();

                while (_conversionThread.ThreadState != ThreadState.Stopped)
                {
                    Thread.Sleep(10);
                }

                _conversionThread = null;
            }
        }
    }

    private void Run()
    {
        while (!_kill)
        {
            try
            {
                // get next conversion task from blocking collection
                var task = _conversions.Take();

                lock (task)
                {
                    //run taks on thread that called RunSynchronously method
                    task.RunSynchronously();

                    //notify caller thread that task is completed
                    Monitor.Pulse(task);
                }
            }
            catch (InvalidOperationException)
            {
                //BlockingCollection 已 CompleteAdding，worker 线程退出
                break;
            }
        }
    }
}