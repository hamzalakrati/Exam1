using System;
using System.Diagnostics.Tracing;

namespace Exam1
{

    public class MyEventListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Console.WriteLine($"created {eventSource.Name} {eventSource.Guid}");
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Console.WriteLine($"event id: {eventData.EventId} source: {eventData.EventSource.Name}");
            foreach (var payload in eventData.Payload)
            {
                Console.WriteLine($"\t{payload}");
            }
        }
    }

    [EventSource(Name = "EventSourceOfExam1", Guid = "45FFF0E2-7198-4E4F-9FC3-DF6934680096")]
    public class SampleEventSource : EventSource
    {
        public class Keywords
        {
            public const EventKeywords Database = (EventKeywords)1;
            public const EventKeywords Calculation = (EventKeywords)2;
        }

        public class Tasks
        {
            public const EventTask CalculationTask = (EventTask)1;
        }

        public SampleEventSource(string name) : base(name)
        {

        }

        [Event(1, Opcode = EventOpcode.Start, Level = EventLevel.Verbose)]
        public void Startup() => WriteEvent(1);

        [Event(2, Opcode = EventOpcode.Info, Task = Tasks.CalculationTask,
          Level = EventLevel.Verbose, Keywords = Keywords.Database)]
        public void ThresholdReached(string message) => WriteEvent(2, message);

    }

    class Exam1
    {
        static void Main(string[] args)
        {
            Counter counter = new Counter();
            counter.Add(1);
            counter.Add(1);

            // Will cause the event to be invoked
            counter.Add(1);

            SampleEventSource sampleEventSource = new SampleEventSource("Exam1");
            MyEventListener listener = new MyEventListener();
            listener.EnableEvents(sampleEventSource, EventLevel.LogAlways);
            sampleEventSource.ThresholdReached("\nThe threshold was reached.");
            sampleEventSource.Write("Complete", new { Info = "\nSender: " + counter });
            sampleEventSource.Write("Complete", new { Info = "\nThreshold:  " + counter.GetCount() });
            sampleEventSource.Write("Complete", new { Info = "\nTime: " + DateTime.Now });
            sampleEventSource.Dispose();
        }
        class Counter
        {
            private int counter = 0;
            public int GetCount()
            {
                return counter;
            }

            public event EventHandler ThresholdReached;

            protected virtual void OnThresholdReached(EventArgs e)
            {
                EventHandler handler = ThresholdReached;
                handler?.Invoke(this, e);
            }

            public void Add(int x)
            {
                counter++;

                if (counter < 3)
                    Console.WriteLine("Below Threshold");
                else
                    OnThresholdReached(new ThresholdReachedEventArgs(counter, DateTime.Now));
            }

        }

        public class ThresholdReachedEventArgs : EventArgs
        {
            public ThresholdReachedEventArgs(int t, DateTime d)
            {
                Threshold = t;
                TimeReached = d;
            }
            public int Threshold { get; set; }
            public DateTime TimeReached { get; set; }
        }


        static void ThresholdReachedHandler(object sender, EventArgs e)
        {
            Console.WriteLine("The threshold was reached.");
        }


        static void CustomThresholdReachedHandler(object sender, EventArgs e)
        {
            Console.WriteLine("The threshold was reached.");

            Console.WriteLine("Sender: " + sender);

            Console.WriteLine("Threshold: " + ((ThresholdReachedEventArgs)e).Threshold);

            Console.WriteLine("Time: " + ((ThresholdReachedEventArgs)e).TimeReached);
        }
    }
}
