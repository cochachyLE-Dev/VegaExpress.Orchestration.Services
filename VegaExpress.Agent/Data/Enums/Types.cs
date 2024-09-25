namespace VegaExpress.Agent.Data.Enums
{
    public enum ServiceStartType
    {
        Automatic = 0,  // El servicio se inicia automáticamente cuando el sistema se inicia.
        Manual = 1,     // El servicio debe ser iniciado manualmente por el usuario.
        Disabled = 2    // el servicio está deshabilitado y no puede ser iniciado hasta que cambie su estado.
    }    

    #region Bind service
    public enum ServiceType
    {
        Unknown = 0,
        GatewayRoute = 1,
        LoadBalancer = 2,
        Worker = 3,
        Client = 4
    }
    public enum ServiceState
    {
        Unknown = 0,         // El servicio se encuentra con estado desconocido.
        Running = 1,         // El servicio está en ejecución.
        Started = 2,         // El servicio ha iniciado.
        Stopped = 3,         // El servicio ha parado.
        Disposed = 4,        // El servicio ha sido desechado.
        Error = 5            // El servicio ha encontrado un error.
    }
    public enum LoadBalancingMode
    {
        LeastRequests = 0   // Dirigir las nuevas solicitudes al destino que tiene menos solicitudes activas.        
    }

    public enum MessageType
    {
        Unknown = 0,
        Instruction = 1,
        Occurrence = 2,
        Metrics = 3
    }
    public enum InstructionType
    {
        StartService = 0,
        StopService = 1,
        PauseService = 2,
        ResumeService = 3,
        UpdateService = 4,
        DeleteService = 5
    }
    public enum OccurrenceType
    {
        Information = 0,
        Warning = 1,
        Error = 2,
        Critical = 3
    }
    public enum ConditionType
    {
        Equals = 0,
        Not_equals = 1,
        Greater_than = 2,
        Less_than = 3,
        Greater_than_or_equal = 4,
        Less_than_or_equal = 5,
        Contains = 6,
        Starts_with = 7,
        Ends_with = 8,
        In = 9
    }
    public enum SessionEventType
    {
        StartingService = 0,        // cuando el servicio esta iniciando.    
        ServiceStarted = 1,         // cuando el servicio se inicia.
        StoppingService = 2,        // cuando el servicio esta deteniendo.
        ServiceStopped = 3,         // cuando el servicio se detiene.
        RebootingService = 4,       // cuando el servicio esta reinicinado.
        ServiceRebooted = 5,        // cuando el servicio se reinicia.
        BeginningStream = 6,        // cuando una transmisión está comenzando.
        StreamStarted = 7,          // cuando una transmisión comienza.
        PausingStream = 8,          // cuando una transmisión se está pausando.
        StreamPaused = 9,           // cuando una transmisión se pausa.
        ResumingStream = 10,        // cuando una transmisión se está reanudando.
        StreamResumed = 11,         // cuando una transmisión se reanuda.
        EndingStream = 12,          // cuando una transmisión se está deteniendo.
        StreamEnded = 13,           // cuando una transmisión se detiene.
        FinishingStream = 14,       // cuando una transmisión se está finalizando.
        FinishedStream = 15,        // cuando una transmisión se finaliza.
        ConnectionErrorEvent = 16   // cuando ocurre un error en el servicio.
    }

    public enum RoutingEventType
    {
        RouteAdded = 0,                  // cuando se agrega una nueva ruta.
        RouteRemoved = 1,                // cuando se elimina una ruta.
        RouteUpdated = 2,                // cuando se actualiza una ruta.
        RequestRouted = 3,               // cuando una solicitud es enrutada.
        RequestReceived = 4,             // cuando se recibe una solicitud.
        ResponseSent = 5,                // cuando se envía una respuesta.
        RouteError = 6,                  // cuando ocuurre un error durante el enrutamiento.
        ServiceUnavailable = 7,          // cuando un servicio al que se intenta enrutar no está disponible.
        FallbackActivated = 8,           // cuando se activa un mecanismo de fallback debido a un error o un servicio no disponible.
        CircuitBreakerTriggered = 9,     // cuando se activa un circuit breaker.
    }
    public enum LoadBalancerEventType
    {
        ServerAdded = 0,                   // cuando se agrega un nuevo servidor al grupo de balanceo de carga.
        ServerRemoved = 1,                 // cuando se elimina un servidor del grupo de balanceo de carga.
        ServerUnavailable = 2,             // cuando un servidor en el grupo de balanceo de carga no está disponible.
        LoadBalancingRuleChanged = 3,      // cuando se cambia una regla de balanceo de carga.
        TrafficIncreased = 4,              // cuando el tráfico a los servidores aumenta sifnigicativamente.
        TrafficDecreased = 5,              // cuando el tráfico a los servidores disminuye significatevamente.
        BalancerError = 6,                 // cuando ocurre un error durante el balanceo de carga.
        BealthCheckFailed = 7,             // cuando una verificación de salud falla para un servidor en el grupo de balanceo.
        HealthCheckPassed = 8,             // cuando una verificación de salud pasa para un servidor en el grupo de balanceo de carga.
    }
    #endregion
}