using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    public static class EventAggregator
    {
        private static Dictionary<Type, List<Action<object>>> _eventHandlers = new Dictionary<Type, List<Action<object>>>();

        public static void Subscribe<TEvent>(Action<TEvent> handler)
        {
            Type eventType = typeof(TEvent);
            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<Action<object>>();
            }

            _eventHandlers[eventType].Add(obj => handler((TEvent)obj));
        }

        public static void Publish<TEvent>(TEvent eventToPublish)
        {
            Type eventType = typeof(TEvent);
            if (_eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in _eventHandlers[eventType])
                {
                    handler(eventToPublish);
                }
            }
        }
    }

    // Definición de eventos
    public class MateriaPrimaActualizadaEvent { }
    public class ProductoActualizadoEvent { }
    public class ProduccionRegistradaEvent { }
    public class RecetaActualizadaEvent
    {
        public int IdProducto { get; set; }
    }
    public class CostoFijoActualizadoEvent { }
    public class AlertasActualizadasEvent { }
}