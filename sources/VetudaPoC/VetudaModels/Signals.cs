using System;
using System.Collections.Generic;
using System.Text;

namespace VentudaModels
{

    public class IOTObjectMetric
    {
        public string IOTObjectId { get; set; }
        public int Value { get; set; }

    }

    public class IoTObject
    {
        public IoTObject()
        {
            this.Signals = new List<IOTObjectMetric>();
        }

        public string id;
        public List<IOTObjectMetric> Signals { get; set; }
    }
    public class ObjectSignals
    {
        public ObjectSignals()
        {
            this.Objects = new List<IoTObject>();
        }
        public List<IoTObject> Objects { get; set; }
    }

}
