using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using VentudaVibexDemo.Models;

namespace VentudaVibexDemo.Controllers
{
    public class IOTObjectController : ApiController
    {
        // GET: api/IOTObject
        public ObjectSignals Get()
        {
            return this.Get("A", 5, 5);
        }

        // GET: api/IOTObject/5
        public ObjectSignals Get(string customer, int nrCars, int nrSignals)
        {
            Random rand = new Random();

            var signals = new ObjectSignals();
            for (int c = 0; c < nrCars; c++)
            {
                var obj = new IoTObject();
                obj.id = $"{customer}-{c.ToString()}";

                int previousTemp = 30;
                for (int s = 0; s < nrSignals; s++)
                {
                    int change = rand.Next(11) - 5;
                    IOTObjectMetric iotObject = new IOTObjectMetric()
                    {
                        IOTObjectId = s.ToString(),
                        Value = previousTemp + change
                    };
                    obj.Signals.Add(iotObject);
                    previousTemp = iotObject.Value;
                }
                signals.Objects.Add(obj);
            }

            Thread.Sleep(1000);
            return signals;
        }

        // POST: api/IOTObject
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/IOTObject/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/IOTObject/5
        public void Delete(int id)
        {
        }
    }
}
